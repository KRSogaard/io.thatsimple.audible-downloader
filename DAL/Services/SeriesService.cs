using AudibleDownloader.Utils;
using MySql.Data.MySqlClient;
using NLog;
using AudibleDownloader.Models;

namespace AudibleDownloader.DAL.Services;

public class SeriesService
{
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public async Task AddBookToSeries(int bookId, int seriesId, string bookNumber)
    {
        var storedBookNumber = await MSU.Query(
            "SELECT * FROM `series_books` WHERE `book_id` = @bookId AND `series_id` = @seriesId",
            new Dictionary<string, object> { { "@bookId", bookId }, { "@seriesId", seriesId } }, async reader =>
            {
                if (!await reader.ReadAsync())
                    return null;
                return new Tuple<string>(MSU.GetStringOrNull(reader, "book_number"));
            });

        if (storedBookNumber == null)
        {
            log.Trace("Adding book {0} to series {1} with book number {2}", bookId, seriesId, bookNumber);
            await MSU.Execute(
                "INSERT INTO `series_books` (`book_id`, `series_id`, `book_number`, `created`) VALUES (@bookId, @seriesId, @bookNumber, @created)",
                new Dictionary<string, object>
                {
                    { "@bookId", bookId },
                    { "@seriesId", seriesId },
                    { "@bookNumber", string.IsNullOrWhiteSpace(bookNumber) ? null : bookNumber.Trim() },
                    { "@created", DateTimeOffset.Now.ToUnixTimeSeconds() }
                });
            await UpdateSeries(seriesId);
        }
        else
        {
            if (storedBookNumber.Item1 != null && storedBookNumber.Item1 != bookNumber)
            {
                log.Debug("Updating the book number to {0} for book {1} in series {2}", bookNumber, bookId, seriesId);
                await MSU.Execute(
                    "UPDATE `series_books` SET `book_number` = @bookNumber WHERE `book_id` = @bookId AND `series_id` = @seriesId",
                    new Dictionary<string, object>
                    {
                        { "@bookId", bookId },
                        { "@seriesId", seriesId },
                        { "@bookNumber", string.IsNullOrWhiteSpace(bookNumber) ? null : bookNumber.Trim() }
                    });
                await UpdateSeries(seriesId);
            }
        }
    }

    private Task UpdateSeries(int seriesId)
    {
        return MSU.Execute("UPDATE `series` SET `last_updated` = @lastUpdated WHERE `id` = @seriesId",
            new Dictionary<string, object>
                { { "@lastUpdated", DateTimeOffset.Now.ToUnixTimeSeconds() }, { "@seriesId", seriesId } });
    }

    public Task<AudibleSeries?> GetSeriesAsin(string seriesAsin)
    {
        return MSU.Query("SELECT * FROM `series` WHERE `series`.asin = @asin",
            new Dictionary<string, object> { { "@asin", seriesAsin } }, async reader =>
            {
                if (!await reader.ReadAsync())
                    return null;
                return ParseSeriesReader(reader);
            });
    }

    private AudibleSeries ParseSeriesReader(MySqlDataReader reader)
    {
        return new AudibleSeries
        {
            Id = reader.GetInt32("id"),
            Asin = reader.GetString("asin"),
            Name = reader.GetString("name"),
            Link = reader.GetString("link"),
            Summary = MSU.GetStringOrNull(reader, "summary"),
            LastUpdated = reader.GetInt64("last_updated"),
            LastChecked = MSU.GetInt64OrNull(reader, "last_checked"),
            Created = reader.GetInt64("created"),
            ShouldDownload = MSU.GetInt32OrNull(reader, "should_download") == 1
        };
    }

    public async Task<AudibleSeries> SaveOrGetSeries(string asin, string name, string link, string? summary)
    {
        var check = await GetSeriesAsin(asin);
        if (check != null)
        {
            log.Trace("Series {0} ({1}) already exists", name, asin);
            if (summary != null && summary.Length > 0 && check.Summary != summary)
            {
                log.Debug("Updating summary for series {0} summary", name);
                await MSU.Execute(
                    "UPDATE `series` SET `summary` = @summary, `last_updated` = @lastUpdated, `should_download` = @shouldDownload WHERE `id` = @seriesId",
                    new Dictionary<string, object>
                    {
                        { "@summary", summary },
                        { "@seriesId", check.Id },
                        { "@lastUpdated", DateTimeOffset.Now.ToUnixTimeSeconds() },
                        { "@shouldDownload", false }
                    });
            }

            return check;
        }

        log.Info("Creating new series {0} ({1})", name, asin);
        await MSU.Execute(
            "INSERT INTO `series` (`asin`, `link`, `name`, `last_updated`, `last_checked`, `summary`, `created`, `should_download`) VALUES (@asin, @link, @name, @lastUpdated, @lastChecked, @summary, @created, @shouldDownload)",
            new Dictionary<string, object>
            {
                { "@asin", asin },
                { "@link", link },
                { "@name", name },
                { "@lastUpdated", DateTimeOffset.Now.ToUnixTimeSeconds() },
                { "@lastChecked", DateTimeOffset.Now.ToUnixTimeSeconds() },
                { "@summary", summary },
                { "@created", DateTimeOffset.Now.ToUnixTimeSeconds() },
                { "@shouldDownload", true }
            });
        return await GetSeriesAsin(asin);
    }

    public Task SetSeriesShouldDownload(int id, bool v)
    {
        return MSU.Execute("UPDATE `series` SET `should_download` = @download WHERE `id` = @id",
            new Dictionary<string, object> { { "@download", v }, { "@id", id } });
    }

    public Task UpdateBookNumber(int bookId, int seriesId, string bookNumber)
    {
        return MSU.Execute(
            "UPDATE `series_books` SET `book_number` = @bookNumber WHERE `book_id` = @bookId AND `series_id` = @seriesId",
            new Dictionary<string, object>
                { { "@bookNumber", bookNumber }, { "@bookId", bookId }, { "@seriesId", seriesId } });
    }

    public Task<List<AudibleSeries>> GetSeriesToRefresh()
    {
        return MSU.Query("SELECT * FROM `series` WHERE `last_checked` < @lastChecked OR `last_checked`IS NULL",
            new Dictionary<string, object> { { "@lastChecked", DateTimeOffset.Now.AddDays(-7).ToUnixTimeSeconds() } },
            async reader =>
            {
                var series = new List<AudibleSeries>();
                while (await reader.ReadAsync()) series.Add(ParseSeriesReader(reader));

                return series;
            });
    }

    public Task SetSeriesChecked(int seriesId)
    {
        return MSU.Execute("UPDATE `series` SET `last_checked` = @lastChecked WHERE `id` = @id",
            new Dictionary<string, object>
                { { "@lastChecked", DateTimeOffset.Now.ToUnixTimeSeconds() }, { "@id", seriesId } });
    }
}