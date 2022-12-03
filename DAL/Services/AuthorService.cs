using AudibleDownloader.DAL.Models;
using AudibleDownloader.Exceptions;
using AudibleDownloader.Models;
using AudibleDownloader.Protobuf;
using AudibleDownloader.Utils;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace AudibleDownloader.DAL.Services; 

public class AuthorService {
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public async Task<AudibleAuthor?> GetAuthorAsin(string asin) {
        log.Trace("Getting author by asin: {0}", asin);
        Preconditions.CheckNotNullOrEmpty(asin, nameof(asin));

        using (AudibleContext context = new AudibleContext()) {
            return await context.Authors.Where(a => a.Asin == asin).Select(a => a.ToInternal()).FirstOrDefaultAsync();
        }
    }

    public async Task<AudibleAuthor?> GetAuthor(int id) {
        log.Trace("Getting author by id: {0}", id);
        using (AudibleContext context = new AudibleContext()) {
            return await context.Authors.Where(a => a.Id == id).Select(a => a.ToInternal()).FirstOrDefaultAsync();
        }
    }

    public async Task<List<AudibleAuthor>> GetAuthorsForBook(int bookId) {
        log.Trace("Getting authors by book with id: {0}", bookId);
        using (AudibleContext context = new AudibleContext()) {
            return await (from a in context.Authors
                          join ab in context.AuthorsBooks on a.Id equals ab.AuthorId
                          where ab.BookId == bookId
                          select a.ToInternal()).ToListAsync();
        }
    }

    public async Task<bool> AuthorHasBook(int bookId, int authorId) {
        log.Trace("Checking is author {1} is attached to book {0}", bookId, authorId);
        using (AudibleContext context = new AudibleContext()) {
            return await context.AuthorsBooks
                                .Where(ab => ab.AuthorId == authorId && ab.BookId == bookId)
                                .AnyAsync();
        }
    }

    public async Task AddBookToAuthor(int bookId, AudibleAuthor author) {
        log.Trace("Adding author {1} to book {0}", bookId, author.Id);
        Preconditions.CheckNotNull(author, nameof(author));
        Preconditions.CheckNotNullOrEmpty(author.Name, "author.Name");

        bool exists = await AuthorHasBook(bookId, author.Id);
        if (!exists)
            using (AudibleContext context = new AudibleContext()) {
                log.Trace("Fetching book dal object for book id {0}", bookId);
                Book? book = await context.Books.Where(b => b.Id == bookId).FirstOrDefaultAsync();
                if (book == null) throw new FatalException("Book " + bookId + " dose not exists");

                log.Debug($"Adding book {bookId} to author {author.Id}");
                await context.AuthorsBooks.AddAsync(new AuthorsBook {
                                                                        BookId = bookId,
                                                                        AuthorId = author.Id
                                                                    });
                string part = MapUtil.CreateMapPart(new IdValueInfo {
                                                                        Id = author.Id,
                                                                        Value = author.Name
                                                                    });
                book.AuthorsCache = string.IsNullOrWhiteSpace(book.AuthorsCache) ? part : book.AuthorsCache + part;
                await context.SaveChangesAsync();
            }
        else
            log.Trace("Book {0} has already been added to author {1}", bookId, author.Id);
    }

    public async Task<AudibleAuthor> SaveOrGetAuthor(string? asin, string name, string? link) {
        log.Trace("Saving or getting author {0}", name);
        Preconditions.CheckNotNullOrEmpty(name, nameof(name));

        AudibleAuthor? check = null;
        if (asin != null) check = await GetAuthorAsin(asin);
        if (check == null) check = await GetAuthorByName(name);

        if (check != null) {
            if ((string.IsNullOrWhiteSpace(check.Asin) && asin != null) || (string.IsNullOrWhiteSpace(check.Link) && link != null))
                using (AudibleContext context = new AudibleContext()) {
                    Author author = await context.Authors.Where(a => a.Id == check.Id).FirstAsync();
                    author.Asin = asin;
                    author.Link = link;
                    await context.SaveChangesAsync();
                    return author.ToInternal();
                }

            log.Trace("Author {0} already exists", name);
            return check;
        }

        log.Info("Creating new author {0}", name);
        using (AudibleContext context = new AudibleContext()) {
            Author authorDal = new Author {
                                              Asin = asin,
                                              Name = name,
                                              Link = link,
                                              Created = DateTimeOffset.Now.ToUnixTimeSeconds()
                                          };
            await context.Authors.AddAsync(authorDal);
            await context.SaveChangesAsync();
            return authorDal.ToInternal();
        }
    }

    private async Task<AudibleAuthor?> GetAuthorByName(string name) {
        log.Trace("Getting author by name: {0}", name);
        using (AudibleContext context = new AudibleContext()) {
            return await context.Authors
                                .Where(a => a.Name.ToLower() == name.ToLower())
                                .Select(a => a.ToInternal())
                                .FirstOrDefaultAsync();
        }
    }
}