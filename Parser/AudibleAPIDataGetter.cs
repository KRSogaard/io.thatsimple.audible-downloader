using System.Dynamic;
using System.Text;
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
    // "rights",
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

            ParseAudioBook book = new ParseAudioBook();
            book.Asin = asin;
            book.Title = json.product.title;

            DateTimeOffset released = json.product.publication_datetime;
            book.Released = released.ToUnixTimeSeconds();

            book.Link = "https://www.audible.com/pd/" + asin;
            JObject productImages = json.product.product_images;
            book.Image = productImages?.First?.First?.Value<string>();

            book.Subtitle = json.product.subtitle;

            book.RuntimeSeconds = json.product.runtime_length_min * 60;

            book.Summary = json.product.publisher_summary;
            if (book.Summary != null)
            {
                book.Summary = book.Summary.Replace("</p>", "\n")
                    .Replace("<br />", "\n")
                    .Replace("<p>", "").Trim();
            }

            book.Authors = new List<ParseAudioBookPerson>();
            foreach (dynamic jsonAuthor in json.product.authors)
            {
                ParseAudioBookPerson author = new ParseAudioBookPerson();
                author.Name = jsonAuthor.name;
                author.Asin = jsonAuthor.asin;
                book.Authors.Add(author);
            }

            book.Narrators = new List<string>();
            foreach (var jsonNarrator in json.product.narrators)
            {
                var narrator = jsonNarrator.name.Value;
                book.Narrators.Add(narrator);
            }

            book.Tags = new List<string>();
            book.Categories = new List<ParseAudioBookCategory>();
            Dictionary<string, ParseAudioBookCategory> CategoriesToAdd = new Dictionary<string, ParseAudioBookCategory>();
            foreach (dynamic ladder in json.product.category_ladders)
            {
                string latestCateGoryLink = null;
                string latestCategory = null;
                foreach (dynamic category in ladder.ladder)
                {
                    string id = category.id.Value;
                    string name = category.name.Value;
                    string currentLink = RegexHelper.Replace( @"[^\w]+", "-", name.Replace("'", ""));

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
                                                                  + "/" + seriesAsin;
                series.BookNumber = relationship.sequence.Value;
            }

            return book;
        }
        catch (Exception e)
        {
            log.Error(e, "Error parsing json Book");
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