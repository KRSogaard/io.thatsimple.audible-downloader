using AudibleDownloader.DAL.Models;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace AudibleDownloader.DAL.Services;

public class UserService {
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public async Task FinishJob(int jobId) {
        log.Trace($"Finishing job {jobId}");
        using (AudibleContext context = new AudibleContext()) {
            UsersJob? job = await context.UsersJobs.Where(j => j.Id == jobId).FirstOrDefaultAsync();
            if (job != null) {
                context.UsersJobs.Remove(job);
                await context.SaveChangesAsync();
            }
        }
    }

    public async Task AddBookToUser(int userId, int bookId) {
        log.Trace("Adding book {0} to user {1}", bookId, userId);
        using (AudibleContext context = new AudibleContext()) {
            if (await context.UsersBooks.AnyAsync(b => b.UserId == userId && b.BookId == bookId)) {
                log.Trace("Book {0} already exists for user {1}", bookId, userId);
                return;
            }

            UsersBook ub = new UsersBook {
                                             UserId = userId,
                                             BookId = bookId
                                         };
            await context.UsersBooks.AddAsync(ub);
            await context.SaveChangesAsync();
        }
    }

    public async Task<int> CreateJob(int userId, string type, string data) {
        log.Debug("Creating new {1} job for user {0}", userId, type);
        using (AudibleContext context = new AudibleContext()) {
            UsersJob job = new UsersJob {
                                            UserId = userId,
                                            Type = type,
                                            Payload = data,
                                            Created = DateTimeOffset.Now.ToUnixTimeSeconds()
                                        };
            await context.UsersJobs.AddAsync(job);
            await context.SaveChangesAsync();
            return job.Id;
        }
    }
}