using AudibleDownloader.Utils;
using NLog;
using AudibleDownloader.Models;

namespace AudibleDownloader.DAL.Services;

public class UserService
{
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public async Task FinishJob(string jobId)
    {
        log.Debug("Finishing job id {0}", jobId);
        await MSU.Execute("DELETE FROM `users_jobs` WHERE `id` = @jobId",
            new Dictionary<string, object> { { "@jobId", jobId } });
    }

    public async Task AddBookToUser(string userId, int bookId)
    {
        log.Trace("Adding book {0} to user {1}", bookId, userId);

        var exists = await MSU.Query("SELECT * FROM `users_books` WHERE `book_id` = @bookId AND `user_id` = @userId",
            new Dictionary<string, object> { { "@bookId", bookId }, { "@userId", userId } },
            async reader => { return await reader.ReadAsync(); });
        if (exists)
        {
            log.Trace("Book {0} already exists for user {1}", bookId, userId);
            return;
        }

        await MSU.Execute("INSERT INTO `users_books` (`user_id`, `book_id`) VALUES (@userId, @bookId)",
            new Dictionary<string, object> { { "@bookId", bookId }, { "@userId", userId } });
    }

    public Task<int> CreateJob(string userId, string type, string data)
    {
        log.Debug("Creating new {1} job for user {0}", userId, type);
        return MSU.QueryWithCommand(
            "INSERT INTO `users_jobs` (`user_id`, `created`, `type`, `payload`) VALUES (@userId, @created, @type, @payload)",
            new Dictionary<string, object>
            {
                { "@userId", userId }, { "@created", DateTimeOffset.Now.ToUnixTimeSeconds() }, { "@type", type },
                { "@payload", data }
            },
            async (reader, cmd) => { return (int)cmd.LastInsertedId; });
    }
}