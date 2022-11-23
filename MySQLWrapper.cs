using MySql.Data.MySqlClient;
using NLog;

namespace AudibleDownloader {
  public class MySQLWrapper {
    private static Logger log = LogManager.GetCurrentClassLogger();
    private static string? connectionString = null;

    public static MySqlConnection GetConnection() {
      if (connectionString == null) {
        string host = Config.Get("DB_HOST");
        string port = Config.Get("DB_PORT");
        string user = Config.Get("DB_USER");
        string password = Config.Get("DB_PASSWORD");
        string database = Config.Get("DB_NAME");
        connectionString = $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};Pooling=True;";
        log.Info("Creating new connection");
      }
      return new MySqlConnection(connectionString);
    }
  }
}
