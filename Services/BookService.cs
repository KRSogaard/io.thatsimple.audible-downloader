using MySql.Data.MySqlClient;
using AudibleDownloader.Models;

namespace AudibleDownloader.Services {
  public class BookService {
    public async Task<AudibleBook> getBookByASIN(string bookASIN) {
      using(MySqlConnection conn = MySQLWrapper.GetConnection()) {
        await conn.OpenAsync();

        using(MySqlCommand cmd = new MySqlCommand("SELECT * FROM `books` WHERE `asin` = @asin", conn)) {
          cmd.Parameters.AddWithValue("@asin", bookASIN);
          using(MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync()) {
            if (await reader.ReadAsync()) {
              return await parseBookResult(reader, conn);
            }
          }
        }
      }
      return null;
    }

    private async Task<AudibleBook> parseBookResult(MySqlDataReader reader, MySqlConnection conn) {



              return new AudibleBook {
                id = reader.GetInt32("id"),
                asin = reader.GetString("asin"),
                link = reader.GetString("link"),
                title = reader.GetString("title"),
                length = reader.GetInt32("length"),
                released = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("lastUpreleaseddated")).DateTime,
                summary = reader.GetString("summary"),
                lastUpdated = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("lastUpdated")).DateTime,
                series = null,
                authors = null,
                tags = null,
                narrators = null,
                categories = null
              };
    }
  }
}
