using System.Xml;
using AngleSharp;
using AngleSharp.Dom;
using AudibleDownloader.Exceptions;
using AudibleDownloader.Utils;
using NLog;

namespace AudibleDownloader.Parser
{
    public static class AudibleParser
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private static IConfiguration config = Configuration.Default.WithDefaultLoader();
        private static IBrowsingContext context = BrowsingContext.New(config);

        public static async Task<ParseAudioBook> ParseBook(string html)
        {
            IDocument document = await context.OpenAsync(req => req.Content(html));
            string pageText = document.QuerySelector("#center-1").Text()?.Trim();
            if (pageText != null && pageText.Contains("Pre-order:"))
            {
                log.Debug("Book is pre-order, skipping parsing");
                return null;
            }

            string title = document.QuerySelector("h1.bc-heading").Text()?.Trim();
            if (title == null || title.Trim().Length == 0)
            {
                title = document.QuerySelector("meta[property='og:title']")?.GetAttribute("content")?.Trim();
            }

            string subtitle = document.QuerySelector("h1.bc-heading").Parent.NextSibling.Text()?.Trim();
            if (subtitle == null || subtitle.Trim().Length == 0)
            {
                subtitle = null;
            }

            long released = 0;
            string jsonData = null;
            string dataPublishedMatched = RegexHelper.Match(@"datePublished.:\s+""([^""]+)", html);
            if (dataPublishedMatched != null && dataPublishedMatched.Trim().Length > 0)
            {
                DateTimeOffset releasedDate;
                if (DateTimeOffset.TryParse(dataPublishedMatched.Trim(), out releasedDate))
                {
                    released = releasedDate.ToUnixTimeSeconds();
                }
            }

            if (released == 0)
            {
                log.Error("Could not find datePublished for " + title);
                throw new FatalException("Could not find datePublished for " + title);
            }

            string link = document.QuerySelector("link[rel='canonical']")?.GetAttribute("href");
            string amazon_id = link?.Split("/").Last();

            List<ParseAudioBookPerson> authors = new List<ParseAudioBookPerson>();
            foreach (IElement elem in document.QuerySelectorAll(".authorLabel a"))
            {
                string authorLink = elem.GetAttribute("href")?.Split("?")?.First();
                if (authorLink == null)
                {
                    log.Warn("Failed to find Author Link for " + elem.Text());
                    continue;
                }

                authors.Add(new ParseAudioBookPerson()
                {
                    Name = elem.Text().Trim(),
                    Link = "https://www.audible.com" + authorLink,
                    Asin = authorLink.Split("/").Last()
                });
            }

            List<string> narrators = new List<string>();
            foreach (IElement elem in document.QuerySelectorAll(".narratorLabel a"))
            {
                narrators.Add(elem.Text().Trim());
            }

            int runtimeSeconds = 0;
            string durationDom = RegexHelper.Match(@"""duration"":\s+""([^""]+)", html);
            if (durationDom != null && durationDom.Length > 0)
            {
                TimeSpan test = XmlConvert.ToTimeSpan(durationDom.Trim());
                runtimeSeconds = (int)test.TotalSeconds;
            }
            else
            {
                log.Debug("Did not find ISO time duration, trying to parse from text");
                string runtimeSplit = document.QuerySelector("li.runtimeLabel").Text().Split(":")[1];
                TimeSpan ts = GetTimeSpanFromText(runtimeSplit);
                runtimeSeconds = (int)ts.TotalSeconds;
            }

            string summary = "";
            foreach (IElement elem in document.QuerySelectorAll(".bc-product-description p"))
            {
                summary += elem.Text().Trim() + "\n";
            }
            summary = summary.Trim();
            
            if (summary.Length == 0)
            {
                log.Debug("Did not find summary, trying to parse from productPublisherSummary");
                foreach (IElement elem in document.QuerySelectorAll(".productPublisherSummary p"))
                {
                    summary += elem.Text().Trim() + "\n";
                }
                summary = summary.Trim();
            }

            if (summary.Trim().Length == 0) {
                log.Warn("Was unable to find a summary for " + title + " " + amazon_id);
            }

            IElement parentDom = document.QuerySelector("li.seriesLabel");
            List<string> bookNumbersTryGet = RegexHelper.MatchAll(@",\s+[Bb]ook\s+([^\n,]+)", parentDom.Text());

            List<ParseAudioBookSeries> series = new List<ParseAudioBookSeries>();
            var seriesDom = parentDom.QuerySelectorAll("a");
            for (int i = 0; i < seriesDom.Length; i++)
            {
                IElement dom = seriesDom[i];
                log.Debug("Found series: " + dom.Text());
                string bookNumber = null;
                string seriesLink = "https://www.audible.com" + dom.GetAttribute("href").Split('?')[0];
                string seriesName = dom.Text();
                string seriesId = seriesLink.Split("/").Last();
                
                if (bookNumbersTryGet != null && bookNumbersTryGet.Count >= i + 1)
                {
                    bookNumber = bookNumbersTryGet[i].Trim();
                }
                
                series.Add(new ParseAudioBookSeries()
                {
                    Name = seriesName,
                    Link = seriesLink,
                    Asin = seriesId,
                    BookNumber = bookNumber
                });
            }

            List<ParseAudioBookCategory> categories = new List<ParseAudioBookCategory>();
            foreach (IElement dom in document.QuerySelectorAll("li.categoriesLabel a"))
            {
                string catLink = "https://www.audible.com" + dom.GetAttribute("href").Split('?')[0];
                string catName = dom.Text().Trim();
                string catId = catLink.Split("/").Last();
                categories.Add(new ParseAudioBookCategory()
                {
                    Name = catName,
                    Link = catLink,
                    Id = catId
                });

            }
            
            List<string> tags = new List<string>();
            foreach (IElement dom in document.QuerySelectorAll(".product-topic-tags span.bc-chip-text"))
            {
                tags.Add(dom.Text().Trim());
            }

            string image = document.QuerySelector(".hero-content img.bc-pub-block").GetAttribute("src");
            if (image == null || image.Trim().Length == 0)
            {
                image = RegexHelper.Match(@"""image"":\\s+""([^""]+)", html)?.Trim();
            }
            
            return new ParseAudioBook()
            {
                Title = title,
                Link = link,
                Asin = amazon_id,
                Authors = authors,
                Narrators = narrators,
                Released = released,
                Runtime = runtimeSeconds,
                Summary = summary,
                Series = series,
                Categories = categories,
                Tags = tags,
                Image = image
            };
        }

        internal static async Task<ParseSeries> ParseSeries(string html)
        {
            IDocument document = await context.OpenAsync(req => req.Content(html));
            List<ParseSeriesBook> booksList = new List<ParseSeriesBook>();

            try
            {
                var test = document.QuerySelector("h1.bc-heading");
                string name = document.QuerySelector("h1.bc-heading")?.Text().Trim();
                string summary = document.QuerySelector(".bc-expander-content").Text().Trim();
                string link = document.QuerySelector("link[rel='canonical']").GetAttribute("href");

                foreach (IElement dom in document.QuerySelectorAll("li.productListItem"))
                {
                    if (dom.Text().Contains("'Not available on audible.com'"))
                    {
                        log.Warn("Book is not available on audible.com, skipping");
                        continue;
                    }

                    if (dom.Text().Contains("Pre-order"))
                    {
                        log.Warn("Book is not released yet, skipping");
                        continue;
                    }

                    string urlElement = dom.QuerySelector("h3 a").GetAttribute("href")?.Split("?")[0];
                    if (urlElement == null)
                    {
                        log.Warn("Could not find book url in series page, book may be unavalible skipping");
                        continue;
                    }

                    string url = "https://www.audible.com" + urlElement;
                    string text = dom.QuerySelector("h2.bc-heading").Text().Trim();
                    string bookNumber = null;
                    if (text != null && text.ToLower().Contains("book"))
                    {
                        bookNumber = text.Substring(text.ToLower().IndexOf("book") + "book".Length + 1).Trim();
                    }

                    string title = dom.QuerySelector("h3").Text().Trim();
                    log.Debug("Title: " + title);
                    if (String.IsNullOrWhiteSpace(title))
                    {
                        title = "Unknown title";
                    }
                    
                    int runtimeSeconds = 0;
                    string runtimeText = dom.QuerySelector(".runtimeLabel").Text().Trim();

                    if (!String.IsNullOrWhiteSpace(runtimeText) && runtimeText.Contains("Length:"))
                    {
                        TimeSpan timeSpan = GetTimeSpanFromText(runtimeText.Substring(runtimeText.IndexOf("Length:") + "Length:".Length + 1).Trim());
                        runtimeSeconds = (int)timeSpan.TotalSeconds;
                    }
                    else
                    {
                        log.Warn("Failed to parse runtime: " + runtimeText);
                    }
                    
                    long releasedSeconds = 0;
                    string releasedText = dom.QuerySelector(".releaseDateLabel").Text().Trim();
                    if (!String.IsNullOrWhiteSpace(releasedText) && releasedText.Contains("date:"))
                    {
                        releasedText = releasedText.Substring(releasedText.IndexOf("date:") + "date:".Length + 1).Trim();
                        DateTimeOffset releasedDate;
                        if (DateTimeOffset.TryParse(releasedText.Trim(), out releasedDate))
                        {
                            releasedSeconds = releasedDate.ToUnixTimeSeconds();
                        }
                    }
                    
                    booksList.Add(new ParseSeriesBook()
                    {
                        Link = url,
                        Asin = url.Split("?")[0].Split("/")?.Last(),
                        BookNumber = bookNumber,
                        LengthSeconds = runtimeSeconds,
                        ReleaseDate = releasedSeconds,
                        Title = title
                    });
                }

                string asin = document.QuerySelector("input[name='asin']").GetAttribute("value");
                return new ParseSeries()
                {
                    Link = link,
                    Name = name,
                    Summary = summary,
                    Asin = asin,
                    Books = booksList
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }

        public static TimeSpan GetTimeSpanFromText(string text)
        {
            string runtimeSplit = text.Replace("hrs", "h").Replace("mins", "m").Replace("and", "").Trim();
            string hoursText = RegexHelper.Match(@"([0-9]+)\s*h", runtimeSplit);
            int hours = 0;
            if (hoursText != null && hoursText.Length > 0)
            {
                hours = int.Parse(hoursText);
            }

            string minsText = RegexHelper.Match(@"([0-9]+)\s*m", runtimeSplit);
            int mins = 0;
            if (minsText != null && minsText.Length > 0)
            {
                mins = int.Parse(minsText);
            }

            return new TimeSpan(hours, mins, 0);
        }
    }
}
