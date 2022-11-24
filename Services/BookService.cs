using AudibleDownloader.Models;
using MySql.Data.MySqlClient;

namespace AudibleDownloader.Services
{
    public class BookService
    {
        public async Task<AudibleBook> getBookByASIN(string bookASIN)
        {
            using (MySqlConnection conn = MySQLWrapper.GetConnection())
            {
                await conn.OpenAsync();

                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM `books` WHERE `asin` = @asin", conn))
                {
                    cmd.Parameters.AddWithValue("@asin", bookASIN);
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return await parseBookResult(reader, conn);
                        }
                    }
                }
            }
            return null;
        }

        private async Task<AudibleBook> parseBookResult(MySqlDataReader reader, MySqlConnection conn)
        {
            Task<AudibleSeriesBook[]> series = getSeriesWithBookNumber(reader.GetInt32("id"));

            return new AudibleBook
            {
                id = reader.GetInt32("id"),
                asin = reader.GetString("asin"),
                link = reader.GetString("link"),
                title = reader.GetString("title"),
                length = reader.GetInt32("length"),
                released = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("lastUpreleaseddated")).DateTime,
                summary = reader.GetString("summary"),
                lastUpdated = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("lastUpdated")).DateTime,
                series = await series,
                authors = null,
                tags = null,
                narrators = null,
                categories = null
            };
        }

        private async Task<AudibleSeriesBook[]> getSeriesWithBookNumber(int seriesId)
        {
            List<AudibleSeriesBook> seriesBooks = new List<AudibleSeriesBook>();
            using (MySqlConnection conn = MySQLWrapper.GetConnection())
            {
                await conn.OpenAsync();

                using (MySqlCommand cmd =
                    new MySqlCommand("SELECT `series`.*, `series_books`.book_number FROM `series` " +
                      "LEFT JOIN `series_books` ON `series_books`.series_id = `series`.id " +
                      "WHERE `series_books`.book_id = @bookId",
                      conn))
                {
                    cmd.Parameters.AddWithValue("@bookId", seriesId);
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            seriesBooks.Add(parseSeriesBookResult(reader));
                        }
                    }
                }
            }
            return seriesBooks.ToArray();
        }

        private AudibleSeriesBook parseSeriesBookResult(MySqlDataReader reader)
        {
            return new AudibleSeriesBook
            {
                id = reader.GetInt32("id"),
                asin = reader.GetString("asin"),
                link = reader.GetString("link"),
                summary = reader.GetString("summary"),
                name = reader.GetString("name"),
                lastUpdated = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("lastUpdated")).DateTime,
                bookNumber = reader.GetString("book_number")
            };
        }

        private async Task<AudibleAuthor> getAuthorForBook(int bookId)
        {
            using (MySqlConnection conn = MySQLWrapper.GetConnection())
            {
                await conn.OpenAsync();

                using (MySqlCommand cmd =
                    new MySqlCommand("SELECT `authors`.* FROM `authors` " + 
                    "LEFT JOIN `books_authors` ON `books_authors`.author_id = `authors`.id " + 
                    "WHERE `books_authors`.book_id = @bookId",
                      conn))
                {
                    cmd.Parameters.AddWithValue("@bookId", bookId);
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return parseAuthorResult(reader);
                        }
                    }
                }
            }
            return null;
        }
    }
}
