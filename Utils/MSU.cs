using MySql.Data.MySqlClient;
using AudibleDownloader.Services;

namespace AudibleDownloader.Utils;

public static class MSU
{
    public static async Task<T> QueryWithCommand<T>(string sql, Dictionary<string, object> parameters,
        Func<MySqlDataReader, MySqlCommand, Task<T>> func)
    {
        using (var conn = MySQLService.GetConnection())
        {
            await conn.OpenAsync();
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (parameters != null)
                    foreach (var key in parameters.Keys)
                        cmd.Parameters.AddWithValue(key, parameters[key]);

                using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    return await func(reader, cmd);
                }
            }
        }
    }

    public static Task<T> Query<T>(string sql, Dictionary<string, object> parameters,
        Func<MySqlDataReader, Task<T>> func)
    {
        return QueryWithCommand(sql, parameters, (reader, cmd) => func(reader));
    }

    public static async Task Execute(string sql, Dictionary<string, object> parameters)
    {
        using (var conn = MySQLService.GetConnection())
        {
            await conn.OpenAsync();
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (parameters != null)
                    foreach (var key in parameters.Keys)
                        cmd.Parameters.AddWithValue(key, parameters[key]);

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public static string? GetStringOrNull(MySqlDataReader reader, string key)
    {
        return reader.IsDBNull(reader.GetOrdinal(key)) ? null : reader.GetString(key);
    }

    public static int? GetInt32OrNull(MySqlDataReader reader, string key)
    {
        return reader.IsDBNull(reader.GetOrdinal(key)) ? null : reader.GetInt32(key);
    }

    public static long? GetInt64OrNull(MySqlDataReader reader, string key)
    {
        return reader.IsDBNull(reader.GetOrdinal(key)) ? null : reader.GetInt64(key);
    }
}