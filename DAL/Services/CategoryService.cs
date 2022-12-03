using AudibleDownloader.DAL.Models;
using AudibleDownloader.Exceptions;
using AudibleDownloader.Utils;
using NLog;
using AudibleDownloader.Models;
using Microsoft.EntityFrameworkCore;

namespace AudibleDownloader.DAL.Services;

public class CategoryService
{
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public async Task<List<AudibleCategory>> getCategoriesForBook(int bookId)
    {
        log.Trace($"Getting all categories for book: {bookId}");
        using (var context = new AudibleContext())
        {
            return await (from c in context.Categories
                    join cb in context.CategoriesBooks on c.Id equals cb.CategoryId
                    where cb.BookId == bookId
                    select c.ToInternal()).ToListAsync();
        }
    }

    public async Task AddCategoryToBook(int bookId, AudibleCategory category)
    {
        log.Trace($"Adding category {category.Name} to book: {bookId}");
        using (var context = new AudibleContext())
        {
            if (await context.CategoriesBooks.AnyAsync(cb => cb.BookId == bookId && cb.CategoryId == category.Id))
            {
                log.Trace($"Category {category.Name} already exists for book {bookId}");
                return;
            }
            
            var book = await context.Books.Where(b => b.Id == bookId).FirstOrDefaultAsync();
            if (book == null)
            {
                log.Error("Unable to find book with id {0}", bookId);
                throw new FatalException("Unable to find book");
            }

            var cb = new CategoriesBook()
            {
                BookId = bookId,
                CategoryId = category.Id,
                Created = DateTimeOffset.Now.ToUnixTimeSeconds()
            };
            await context.CategoriesBooks.AddAsync(cb);
            book.LastUpdated = DateTimeOffset.Now.ToUnixTimeSeconds();
            book.CategoriesCache = (book.CategoriesCache ?? "") + MapUtil.CreateMapPart(new IdValueInfo()
            {
                Id = category.Id,
                Value = category.Name
            });
            await context.SaveChangesAsync();
        }
    }

    public async Task<AudibleCategory> SaveOrGetCategory(string name, string link)
    {
        log.Trace("Saving category {0}", name);
        var check = await GetGategoryByName(name);
        if (check != null)
        {
            log.Trace("Category {0} already exists");
            return check;
        }

        log.Info("Saving new category {0}", name);
        using (var context = new AudibleContext())
        {
            var category = new Category()
            {
                Name = name,
                Link = link,
                Created = DateTimeOffset.Now.ToUnixTimeSeconds()
            };
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();
            return category.ToInternal();
        }
    }

    public async Task<AudibleCategory?> GetGategoryByName(string name)
    {
        log.Trace("Getting category by name: {0}", name);

        using (var context = new AudibleContext())
        {
            return await context.Categories
                .Where(c => c.Name == name)
                .Select(c => c.ToInternal())
                .FirstOrDefaultAsync();
        }
    }
}