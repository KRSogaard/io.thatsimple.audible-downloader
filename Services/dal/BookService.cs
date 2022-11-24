using System;
using NLog;
using MySql.Data.MySqlClient;
using AudibleDownloader.Utils;
using System.Collections.Generic;

namespace AudibleDownloader.Services.dal
{
    public class BookService
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        private AuthorService authorService;
        private NarratorService narratorService;
        private CategoryService categoryService;
        private TagService tagService;

        public BookService(AuthorService authorService, NarratorService narratorService, CategoryService categoryService, TagService tagService)
        {
            this.authorService = authorService;
            this.narratorService = narratorService;
            this.categoryService = categoryService;
            this.tagService = tagService;
        }

        public Task<AudibleBook?> getBookASIN(string asin)
        {
            return MSU.Query("SELECT * FROM `books` WHERE `asin` = @asin", new Dictionary<string, object> {{ "@asin", asin }}, 
                async (MySqlDataReader reader) =>
                {
                    if (reader != null && await reader.ReadAsync())
                    {
                        return await ParseBookResult(reader);
                    }
                    return null;
                });
        }

        public async Task<AudibleBook?> getBook(int id)
        {
            return await MSU.Query("SELECT * FROM `books` WHERE `id` = @id", new Dictionary<string, object> { { "@id", id } },
                async (MySqlDataReader reader) =>
                {
                    if (reader != null && await reader.ReadAsync())
                    {
                        return await ParseBookResult(reader);
                    }
                    return null;
                });
        }
        
        private async Task<AudibleBook> ParseBookResult(MySqlDataReader reader, bool getSeries = false)
        {
            int bookId = reader.GetInt32("id");

            List<SimpleSeries> series = new List<SimpleSeries>();
            if (getSeries)
            {
                series = await getSimpleSeriesForBook(bookId);
            }

            var authors = authorService.getAuthorsForBook(bookId);
            var tags = tagService.getTagsForBook(bookId);
            var narrators = narratorService.getNarratorsForBook(bookId);
            var categories = categoryService.getCategoriesForBook(bookId);

            return new AudibleBook
            {
                Id = bookId,
                Asin = reader.GetString("asin"),
                Link = reader.GetString("link"),
                Title = reader.GetString("title"),
                Length = reader.GetInt32("length"),
                Released = reader.GetInt32("released"),
                Summary = reader.GetString("summary"),
                LastUpdated = reader.GetInt32("lastUpdated"),
                Series = series,
                Authors = await authors,
                Tags = await tags,
                Narrators = await narrators,
                Categories = await categories
            };
        }

        private async Task<List<SimpleSeries>> getSimpleSeriesForBook(int bookId)
        {
            return await MSU.Query("SELECT `series`.*, `series_books`.book_number FROM `series` " +
                                    "LEFT JOIN `series_books` ON `series_books`.series_id = `series`.id " +
                                    "WHERE `series_books`.book_id = @id", new Dictionary<string, object> { { "@id", bookId } }, 
                async (MySqlDataReader reader) =>
                {
                    List<SimpleSeries> series = new List<SimpleSeries>();
                    while(await reader.ReadAsync())
                    {
                        series.Add(new SimpleSeries()
                        {
                            Id = reader.GetInt32("id"),
                            Asin = reader.GetString("asin"),
                            Link = reader.GetString("link"),
                            Name = reader.GetString("name"),
                            BookNumber = reader.GetString("book_number")
                        });
                    }
                    return series;
                });
        }

        public async Task<AudibleBook> SaveBook(string asin, string link, string title, int runtime, int released, string summary)
        {
            log.Trace("Saving book {0}", title);
            var checkBook = await getBookASIN(asin);
            int bookId = 0;
            
            if (checkBook != null)
            {
                log.Info("Book {0} already exists updating", title);
                await MSU.Execute("UPDATE `books` SET `link` = @link, `title` = @title, `length` = @length, `released` = @released, `summary` = @summary, `last_updated` = @lastUpdated WHERE `asin` = @asin",
                    new Dictionary<string, object> { { "@link", link }, { "@title", title }, { "@length", runtime }, { "@released", released }, { "@summary", summary }, { "@asin", asin } });
                bookId = checkBook.Id;
            } else
            {
                log.Info("Book {0} does not exist, creating", title);
                bookId = await MSU.QueryWithCommand("INSERT INTO `books` (`asin`, `link`, `title`, `length`, `released`, `summary`, `last_updated`, `created`) VALUES (@asin, @link, @title, @length, @released, @summary, @created, @created)",
                    new Dictionary<string, object> { { "@asin", asin }, { "@link", link }, { "@title", title }, { "@length", runtime }, { "@released", released }, { "@summary", summary }, { "@created", (int)DateTimeOffset.Now.ToUnixTimeSeconds() } },
                    async (reader, cmd) =>
                    {
                        return (int)cmd.LastInsertedId;
                    });
            }
            if (bookId == 0)
            {
                log.Error("Failed to save book {0}", title);
                throw new Exception("Failed to save book");
            }
            return await getBook(bookId);
        }
        
        public Task<int> CreateTempBook(string asin, string link)
        {
            log.Trace("Creating temp book with asin {0} and link {1}", asin, link);
            return MSU.QueryWithCommand("INSERT INTO `books` (`asin`, `link`, `created`, `last_updated`) VALUES (@asin, @link, @created, @created)",
                new Dictionary<string, object> { { "@asin", asin }, { "@link", link }, { "@created", (int)DateTimeOffset.Now.ToUnixTimeSeconds() } },
                async (reader, cmd) =>
                {
                    return (int)cmd.LastInsertedId;
                }
                );
        }
    }
}
