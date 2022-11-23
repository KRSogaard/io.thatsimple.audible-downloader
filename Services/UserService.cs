using MySql.Data.MySqlClient;

namespace AudibleDownloader.Services {
  public class UserService {
    public async Task FinishJob(string jobId) {
      using(MySqlConnection conn = MySQLWrapper.GetConnection()) {
        await conn.OpenAsync();
        using(MySqlCommand cmd = new MySqlCommand("DELETE FROM `users_jobs` WHERE `id` = @jobId", conn)) {
          cmd.Parameters.AddWithValue("@jobId", jobId);
          await cmd.ExecuteNonQueryAsync();
        }
      }
    }
  }
}
