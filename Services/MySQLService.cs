using MySql.Data.MySqlClient;
using NLog;

namespace AudibleDownloader.Services;

public class MySQLService
{
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    private static string? connectionString;

    public static MySqlConnection GetConnection()
    {
        if (connectionString == null)
        {
            var host = Config.Get("DB_HOST");
            var port = Config.Get("DB_PORT");
            var user = Config.Get("DB_USER");
            var password = Config.Get("DB_PASSWORD");
            var database = Config.Get("DB_NAME");
            connectionString = $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};Pooling=True;";
            log.Info("Creating new MySQL connection to \"{0}:{1}\" user: \"{2}\", database: \"{3}\"", host, port, user, database);
        }

        return new MySqlConnection(connectionString);
    }
}