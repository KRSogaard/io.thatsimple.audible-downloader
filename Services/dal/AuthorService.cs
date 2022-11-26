using AudibleDownloader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using NLog.Fluent;
using NLog;

namespace AudibleDownloader.Services.dal
{
    public class AuthorService
    {
        private Logger log = LogManager.GetCurrentClassLogger();


        public Task<AudibleAuthor?> getAuthorASIN(string asin)
        {
            return MSU.Query("SELECT * FROM `authors` WHERE `asin` = @asin", new Dictionary<string, object> { { "@asin", asin } },
                async (MySqlDataReader reader) =>
                {
                    if (reader != null && await reader.ReadAsync())
                    {
                        return ParseAuthorResult(reader);
                    }
                    return null;
                });
        }
        public Task<AudibleAuthor?> getAuthor(int id)
        {
            return MSU.Query("SELECT * FROM `authors` WHERE `id` = @id", new Dictionary<string, object> { { "@id", id } },
                async (MySqlDataReader reader) =>
                {
                    if (reader != null && await reader.ReadAsync())
                    {
                        return ParseAuthorResult(reader);
                    }
                    return null;
                });
        }

        private AudibleAuthor ParseAuthorResult(MySqlDataReader reader)
        {
            return new AudibleAuthor
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                Asin = reader.GetString("asin"),
                Link = reader.GetString("link"),
            };
        }

        public Task<List<AudibleAuthor>> getAuthorsForBook(int bookId)
        {
            return MSU.Query("SELECT `authors`.* FROM `authors` LEFT JOIN `authors_books` ON `authors_books`.author_id = `authors`.id WHERE `authors_books`.book_id = @bookId", new Dictionary<string, object> { { "@bookId", bookId } },
                async (MySqlDataReader reader) =>
            {
                List<AudibleAuthor> authors = new List<AudibleAuthor>();
                while (await reader.ReadAsync())
                {
                    authors.Add(new AudibleAuthor
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Link = reader.GetString("link"),
                        Asin = reader.GetString("asin")
                    });
                }
                return authors;
            });
        }

        public async Task AddBookToAuthor(int bookId, AudibleAuthor author)
        {
            log.Debug($"Adding book {bookId} to author {author.Id}");
            bool exists = await MSU.Query("SELECT * FROM `authors_books` WHERE `book_id` = @bookId AND `author_id` = @authorId",
                new Dictionary<string, object> { { "@bookId", bookId }, { "@authorId", author.Id } },
                async (MySqlDataReader reader) =>
            {
                return await reader.ReadAsync() == true;
            });
            if (!exists)
            {
                await MSU.Execute("INSERT INTO `authors_books` (`book_id`, `author_id`, `created`) VALUES (@bookId, @authorId, @created)",
                    new Dictionary<string, object> { { "@bookId", bookId }, { "@authorId", author.Id }, { "@created", DateTimeOffset.Now.ToUnixTimeSeconds() } });

                IdValueInfo mapPart = new IdValueInfo
                {
                    Id = author.Id,
                    Value = author.Name
                };
                await MSU.Execute("UPDATE `books` SET `last_updated` = @lastUpdate, `authors_cache` = concat(ifnull(`authors_cache`,\"\"), @cache) WHERE `id` = @bookId",
                    new Dictionary<string, object> { { "@lastUpdate", DateTimeOffset.Now.ToUnixTimeSeconds() }, { "@cache", MapUtil.CreateMapPart(mapPart) }, { "@bookId", bookId } });
            } else {
                log.Trace("Book {0} has already been added to author {1}", bookId, author.Id);
            }
        }

        public async Task<AudibleAuthor> SaveOrGetAuthor(string asin, string name, string link)
        {
            log.Trace("Saving author {0}", name);
            var check = await getAuthorASIN(asin);
            if (check != null)
            {
                log.Trace("Author {0} already exists", name);
                return check;
            }

            log.Info("Creating new author {0}", name);
            return await MSU.QueryWithCommand("INSERT INTO `authors` (`name`, `asin`, `link`, `created`) VALUES (@name, @asin, @link, @created)", 
                new Dictionary<string, object> { { "@name", name }, { "@asin", asin }, { "@link", link }, { "@created", DateTimeOffset.Now.ToUnixTimeSeconds() } },
                async (MySqlDataReader reader, MySqlCommand cmd) =>
                {
                    return new AudibleAuthor
                    {
                        Id = (int)cmd.LastInsertedId,
                        Name = name,
                        Asin = asin,
                        Link = link,
                    };
                });
        }
    }
}
