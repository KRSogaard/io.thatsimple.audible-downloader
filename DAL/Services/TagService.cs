using AudibleDownloader.DAL.Models;
using AudibleDownloader.Exceptions;
using AudibleDownloader.Models;
using AudibleDownloader.Utils;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace AudibleDownloader.DAL.Services;

public class TagService {
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public async Task<List<AudibleTag>> GetTagsForBook(int bookId) {
        log.Trace($"Geting tags for book {bookId}");
        using (AudibleContext context = new AudibleContext()) {
            return await (from t in context.Tags
                          join bt in context.TagsBooks on t.Id equals bt.TagId
                          where bt.BookId == bookId
                          select t.ToInternal()).ToListAsync();
        }
    }

    public async Task AddTagToBook(int bookId, AudibleTag tag) {
        log.Trace($"Adding tag {tag.Tag} to book {bookId}");
        using (AudibleContext context = new AudibleContext()) {
            bool check = await context.TagsBooks.AnyAsync(tb => tb.BookId == bookId && tb.TagId == tag.Id);
            if (check) {
                log.Debug($"Tag {tag.Tag} already exists for book {bookId}");
                return;
            }

            Book? book = await context.Books.Where(b => b.Id == bookId).FirstOrDefaultAsync();
            if (book == null) {
                log.Error("Unable to find book with id {0}", bookId);
                throw new FatalException("Unable to find book");
            }

            TagsBook tagBook = new TagsBook {
                                                BookId = bookId,
                                                TagId = tag.Id,
                                                Created = DateTimeOffset.Now.ToUnixTimeSeconds()
                                            };
            await context.TagsBooks.AddAsync(tagBook);
            book.LastUpdated = DateTimeOffset.Now.ToUnixTimeSeconds();
            book.TagsCache = (book.TagsCache ?? "") + MapUtil.CreateMapPart(new IdValueInfo {
                                                                                                Id = tag.Id,
                                                                                                Value = tag.Tag
                                                                                            });
            await context.SaveChangesAsync();
        }
    }

    public async Task<AudibleTag> SaveOrGetTag(string tag) {
        AudibleTag? existingTag = await GetTagByName(tag);
        if (existingTag != null) {
            log.Trace("Tag {0} already exists", tag);
            return existingTag;
        }

        log.Info("Creating new tag {0}", tag);
        using (AudibleContext context = new AudibleContext()) {
            Tag newTag = new Tag {
                                     Name = tag,
                                     Created = DateTimeOffset.Now.ToUnixTimeSeconds()
                                 };
            await context.Tags.AddAsync(newTag);
            await context.SaveChangesAsync();
            return newTag.ToInternal();
        }
    }

    private async Task<AudibleTag?> GetTagByName(string tag) {
        log.Trace("Getting tag by name {0}", tag);
        using (AudibleContext context = new AudibleContext()) {
            return await context.Tags
                                .Where(t => t.Name == tag).Select(t => t.ToInternal())
                                .FirstOrDefaultAsync();
        }
    }
}