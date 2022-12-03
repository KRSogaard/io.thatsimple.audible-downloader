using System.Dynamic;
using System.Text;
using System.Text.Json;
using AudibleDownloader.Exceptions;
using Newtonsoft.Json;
using AudibleDownloader.Services;
using AudibleDownloader.Utils;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Fluent;

namespace AudibleDownloader.Parser;

public class AudibleAPIDataGetter : AudibleDataGetter
{
    private readonly Logger log = LogManager.GetCurrentClassLogger();
    
    private static string BookResponseGroups = String.Join(",", new List<string>()
    {
     "category_media",
     "category_metadata",
     "category_ladders",
     "product_attrs",
     "media",
     "contributors",
     "product_desc",
     "product_extended_attrs",
     "sample",
     "sku",
     "offers",
     "periodicals",
     "product_details",
     "relationships",
     "price_fast",
     "library",
     "seo",
    // "search",
    // "product_plans",
    // "ayce_availability",
    "rights",
    });

    private readonly DownloadService downloadService;

    public AudibleAPIDataGetter(DownloadService downloadService)
    {
        this.downloadService = downloadService;    
    }
    
    public async Task<ParseAudioBook> ParseBook(string asin)
    {
        try
        {
            string url = GetAPIUrlBook(asin);
            string jsonText = await downloadService.DownLoadJson(url);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonText);

            if (json.product.is_buyable != true)
            {
                log.Info("Book asin {0} is not buyable, skipping", asin);
                throw new FatalException("Book is not buyable");
            }

            if (json.product.distribution_rights_region != null)
            {
                bool isAvailable = false;
                foreach (var locale in json.product.distribution_rights_region)
                {
                    if (locale == "US")
                    {
                        isAvailable = true;
                        break;
                    }
                }

                if (!isAvailable)
                {
                    log.Warn($"Book {asin} is not available in US, skipping");
                    throw new FatalException("Book is not available in US");
                }
            }

            ParseAudioBook book = new ParseAudioBook();
            book.Asin = asin;
            book.Title = json.product.title;

            string isbnTry = json.product.isbn;
            if (isbnTry != null)
            {
                if (long.TryParse(isbnTry, out long isbn))
                {
                    book.Isbn = isbn;
                }
            }


            DateTimeOffset released = json.product.publication_datetime;
            book.Released = released.ToUnixTimeSeconds();

            book.Link = "https://www.audible.com/pd/" + asin;
            JObject productImages = json.product.product_images;
            book.Image = productImages?.First?.First?.Value<string>();

            book.Subtitle = json.product.subtitle;

            int? runTimeMinutes = json.product.runtime_minutes;
            if (runTimeMinutes == null)
            {
                runTimeMinutes = json.product.runtime_length_min;
            }
            else
            {
                log.Warn("Got it from runtime_minutes");
            }
            if (runTimeMinutes != null)
            {
                book.RuntimeSeconds = (int)runTimeMinutes * 60;
            }

            book.Summary = json.product.publisher_summary;
            if (book.Summary != null)
            {
                book.Summary = book.Summary.Replace("</p>", "\n")
                    .Replace("<br />", "\n")
                    .Replace("<p>", "").Trim();
                book.Summary = RegexHelper.Replace("<[^>]*>", "", book.Summary);
            }

            book.Publisher = json.product.publisher_name;

            book.Authors = new List<ParseAudioBookPerson>();
            if (json.product.authors != null) {
                foreach (dynamic jsonAuthor in json.product.authors)
                {
                    ParseAudioBookPerson author = new ParseAudioBookPerson();
                    author.Name = jsonAuthor.name;
                    author.Asin = jsonAuthor.asin;
                    if (!string.IsNullOrWhiteSpace(author.Asin))
                    {
                        author.Link = "https://www.audible.com/author/" +
                                      RegexHelper.Replace(@"\s+", "-",
                                          RegexHelper.Replace(@"[^\w\s]+", "", author.Name)) + "/" + author.Asin;
                    }
                    book.Authors.Add(author);
                }
            }
            else
            {
                log.Warn("There was not narrator for book {0}, asin {1}", book.Title, asin);
            }

            book.Narrators = new List<string>();
            if (json.product.narrators != null)
            {
                foreach (var jsonNarrator in json.product.narrators)
                {
                    var narrator = jsonNarrator.name.Value;
                    book.Narrators.Add(narrator);
                }
            }
            else
            {
                log.Warn("There was not narrator for book {0}, asin {1}", book.Title, asin);
            }

            book.Tags = new List<string>();
            book.Categories = new List<ParseAudioBookCategory>();
            Dictionary<string, ParseAudioBookCategory> CategoriesToAdd = new Dictionary<string, ParseAudioBookCategory>();
            if (json.product.category_ladders != null)
            {
                foreach (dynamic ladder in json.product.category_ladders)
                {
                    string latestCateGoryLink = null;
                    string latestCategory = null;
                    foreach (dynamic category in ladder.ladder)
                    {
                        string id = category.id.Value;
                        string name = category.name.Value;
                        string currentLink = RegexHelper.Replace(@"[^\w]+", "-", name.Replace("'", ""));

                        ParseAudioBookCategory bookCategory = new ParseAudioBookCategory();
                        bookCategory.Id = id;
                        bookCategory.Name = name;

                        StringBuilder builder = new StringBuilder();
                        if (latestCateGoryLink != null)
                        {
                            builder.Append(latestCateGoryLink);
                            builder.Append("/");
                        }

                        builder.Append(currentLink);
                        builder.Append("-Audiobooks");

                        bookCategory.Link = "https://www.audible.com/cat/" + builder + "/" + id;
                        if (!CategoriesToAdd.ContainsKey(id))
                        {
                            CategoriesToAdd.Add(id, bookCategory);
                        }

                        latestCategory = name;
                        latestCateGoryLink = currentLink;
                    }

                    if (!book.Tags.Contains(latestCategory))
                    {
                        book.Tags.Add(latestCategory);
                    }
                }
            }
            else
            {
                log.Warn("There was not category for book {0}, asin {1}", book.Title, asin);
            }

            book.Categories.AddRange(CategoriesToAdd.Values);

            book.Series = new List<ParseAudioBookSeries>();
            foreach (dynamic relationship in json.product.relationships)
            {
                if (relationship.relationship_type.Value != "series")
                {
                    continue;
                }

                string name = relationship.title.Value;
                string seriesAsin = relationship.asin.Value;
                ParseAudioBookSeries series = new ParseAudioBookSeries();
                series.Name = name;
                series.Asin = seriesAsin;
                series.Link = "https://www.audible.com/series/" +
                              RegexHelper.Replace(@"[^\w]+", "-", name.Replace("'", ""))
                                                                  + "-Audiobooks/" + seriesAsin;
                series.BookNumber = relationship.sequence?.Value;
                if (int.TryParse(relationship.sort?.Value, out int sort))
                {
                    series.Sort = sort;
                }
                else
                {
                    log.Warn("Failed to parse \"{2}\" for series {0}, asin {1}", name, seriesAsin, relationship.sort?.Value);
                }
                book.Series.Add(series);
            }

            return book;
        }
        catch (Exception e)
        {
            log.Warn(e, "Error parsing json Book");
            throw e;
        }
    }

    public async Task<ParseSeries> ParseSeries(string asin)
    {
        try
        {
            string url = GetAPIUrlBook(asin);
            string jsonText = await downloadService.DownLoadJson(url);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonText);
            
            ParseSeries series = new ParseSeries();
            series.Name = json.product.title;
            series.Link = "https://www.audible.com/series/" +
                          RegexHelper.Replace(@"[^\w]+", "-", series.Name.Replace("'", ""))
                          + "/" + asin;
            series.Asin = asin;
            
            series.Books = new List<ParseSeriesBook>();
            foreach (var relationship in json.product.relationships)
            {
                if (relationship.relationship_type.Value != "series" && 
                    relationship.relationship_to_product.Value != "child")
                {
                    continue;
                }
                
                ParseSeriesBook book = new ParseSeriesBook();
                book.Asin = relationship.asin.Value;
                book.Title = "Pending";
                book.BookNumber = relationship.sequence.Value;
                if (int.TryParse(relationship.sort?.Value, out int sort))
                {
                    book.Sort = sort;
                }
                else
                {
                    log.Warn("Failed to parse \"{1}\" for book {0}", asin, relationship.sort?.Value);
                }
                series.Books.Add(book);
            }

            return series;
        }
        catch (Exception e)
        {
            log.Error(e, "Error parsing json Book");
            throw e;
        }
    }

    private string GetAPIUrlBook(string asin)
    {
        
        return
            "https://api.audible.com/1.0/catalog/products/"+asin+"?image_sizes=500&response_groups=" + BookResponseGroups;
    }
    
}