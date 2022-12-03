using AudibleDownloader.DAL.Models;
using AudibleDownloader.Utils;
using MySql.Data.MySqlClient;
using NLog;
using AudibleDownloader.Models;
using Microsoft.EntityFrameworkCore;

namespace AudibleDownloader.DAL.Services;

public class BookService
{
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public async Task<AudibleBook?> getBookASIN(string asin)
    {
        log.Trace("Getting book by asin: {0}", asin);
        using (var context = new AudibleContext())
        {
            return await context.Books
                .Where(b => b.Asin == asin)
                .Select(b => b.ToInternal())
                .FirstOrDefaultAsync();
        }
    }

    public async Task<AudibleBook?> getBook(int id)
    {
        log.Trace("Getting book by id: {0}", id);
        using (var context = new AudibleContext())
        {
            return await context.Books
                .Where(b => b.Id == id)
                .Select(b => b.ToInternal())
                .FirstOrDefaultAsync();
        }
    }

    public async Task<AudibleBook> SaveBook(string asin, long? isbn, string link, string title, int? runtime, long? released,
        string? summary, int? publisherId)
    {
        log.Trace("Saving book {0}", title);

        using (var context = new AudibleContext())
        {
            var book = await context.Books
                .Where(b => b.Asin == asin)
                .FirstOrDefaultAsync();

            if (book != null)
            {
                log.Debug("Book {0} already exists updating", title);
                book.Link = link;
                book.Isbn = isbn;
                book.Title = title;
                book.Length = runtime;
                book.Released = released;
                book.Summary = summary;
                book.PublisherId = publisherId;
                book.LastUpdated = DateTimeOffset.Now.ToUnixTimeSeconds();
                book.ShouldDownload = false;
                book.IsTemp = false;
                await context.SaveChangesAsync();
                return book.ToInternal();
            }

            book = new Book()
            {
                Asin = asin,
                Link = link,
                Isbn = isbn,
                Title = title,
                Length = runtime,
                Released = released,
                Summary = summary,
                PublisherId = publisherId,
                Created = DateTimeOffset.Now.ToUnixTimeSeconds(),
                LastUpdated = DateTimeOffset.Now.ToUnixTimeSeconds(),
                ShouldDownload = false,
                IsTemp = false
            };
            await context.Books.AddAsync(book);
            await context.SaveChangesAsync();
            return book.ToInternal();
        }
    }

    public async Task<AudibleBook> CreateTempBook(string asin)
    {
        log.Debug("Creating temp book with asin {0}", asin);
        using (var context = new AudibleContext())
        {
            var book = new Book()
            {
                Asin = asin,
                Title = "Pending Download",
                Created = DateTimeOffset.Now.ToUnixTimeSeconds(),
                LastUpdated = DateTimeOffset.Now.ToUnixTimeSeconds(),
                ShouldDownload = true,
                IsTemp = true
            };
            await context.Books.AddAsync(book);
            await context.SaveChangesAsync();
            return book.ToInternal();
        }
    }

    public async Task DeleteBookAsin(string asin)
    {
        log.Debug("Deleting book with asin {0}", asin);
        using (var context = new AudibleContext())
        {
            var book = await context.Books.Where(b => b.Asin == asin).FirstOrDefaultAsync();
            if (book != null)
            {
                context.Books.Remove(book);
                await context.SaveChangesAsync();
            }
        }
    }
}