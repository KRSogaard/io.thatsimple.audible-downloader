using AudibleDownloader.DAL.Models;
using AudibleDownloader.Exceptions;
using AudibleDownloader.Models;
using AudibleDownloader.Utils;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace AudibleDownloader.DAL.Services;

public class NarratorService {
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public async Task<List<AudibleNarrator>> getNarratorsForBook(int bookId) {
        log.Trace("Getting narrators for book {0}", bookId);
        using (AudibleContext context = new AudibleContext()) {
            return await (from n in context.Narrators
                          join nb in context.NarratorsBooks on n.Id equals nb.NarratorId
                          where nb.BookId == bookId
                          select n.ToInternal()).ToListAsync();
        }
    }

    public async Task AddNarratorToBook(int bookId, AudibleNarrator narrator) {
        log.Trace("Adding narrator {0} to book {1}", narrator.Name, bookId);
        using (AudibleContext context = new AudibleContext()) {
            if (await context.NarratorsBooks.AnyAsync(nb => nb.BookId == bookId && nb.NarratorId == narrator.Id)) {
                log.Trace("Narrator {0} already attached to book {1}", narrator.Name, bookId);
                return;
            }

            Book? book = await context.Books.Where(b => b.Id == bookId).FirstOrDefaultAsync();
            if (book == null) {
                log.Error("Book {0} not found", bookId);
                throw new FatalException("Book not found");
            }

            NarratorsBook nb = new NarratorsBook {
                                                     BookId = bookId,
                                                     NarratorId = narrator.Id,
                                                     Created = DateTimeOffset.Now.ToUnixTimeSeconds()
                                                 };
            await context.NarratorsBooks.AddAsync(nb);
            book.NarratorsCache = (book.NarratorsCache ?? "") + MapUtil.CreateMapPart(new IdValueInfo {
                                                                                          Id = narrator.Id,
                                                                                          Value = narrator.Name
                                                                                      });
            book.LastUpdated = DateTimeOffset.Now.ToUnixTimeSeconds();
            await context.SaveChangesAsync();
        }
    }

    public async Task<AudibleNarrator> SaveOrGetNarrator(string narrator) {
        AudibleNarrator? check = await GetNarratorByName(narrator);
        if (check != null) {
            log.Trace("Narrator {0} already exists", narrator);
            return check;
        }

        using (AudibleContext context = new AudibleContext()) {
            log.Info("Saving new narrator {0}", narrator);
            Narrator n = new Narrator {
                                          Name = narrator,
                                          Created = DateTimeOffset.Now.ToUnixTimeSeconds()
                                      };
            await context.Narrators.AddAsync(n);
            await context.SaveChangesAsync();
            return n.ToInternal();
        }
    }

    public async Task<AudibleNarrator?> GetNarratorByName(string name) {
        log.Trace("Getting narrator by name {0}", name);
        using (AudibleContext context = new AudibleContext()) {
            return await context.Narrators
                                .Where(n => n.Name == name)
                                .Select(n => n.ToInternal())
                                .FirstOrDefaultAsync();
        }
    }
}