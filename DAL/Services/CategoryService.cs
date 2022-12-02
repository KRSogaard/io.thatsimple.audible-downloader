using AudibleDownloader.Utils;
using NLog;
using AudibleDownloader.Models;

namespace AudibleDownloader.DAL.Services;

public class CategoryService
{
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public Task<List<AudibleCategory>> getCategoriesForBook(int bookId)
    {
        return MSU.Query(
            "SELECT `authors`.* FROM `authors` LEFT JOIN `authors_books` ON `authors_books`.author_id = `authors`.id WHERE `authors_books`.book_id = @bookId",
            new Dictionary<string, object> { { "@bookId", bookId } }, async reader =>
            {
                var categories = new List<AudibleCategory>();
                while (await reader.ReadAsync())
                    categories.Add(new AudibleCategory
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Link = reader.GetString("link"),
                        Created = reader.GetInt32("created")
                    });
                return categories;
            });
    }

    public async Task AddCategoryToBook(int bookId, AudibleCategory category)
    {
        var exists = await MSU.Query(
            "SELECT * FROM `categories_books` WHERE `book_id` = @bookId AND `category_id` = @catId",
            new Dictionary<string, object> { { "@bookId", bookId }, { "@catId", category.Id } },
            async reader => { return await reader.ReadAsync(); });
        if (!exists)
        {
            log.Trace("Adding category {0} to book {1}", category.Id, bookId);
            await MSU.Execute(
                "INSERT INTO `categories_books` (`book_id`, `category_id`, `created`) VALUES (@bookId, @categoryId, @created)",
                new Dictionary<string, object>
                {
                    { "@bookId", bookId }, { "@categoryId", category.Id },
                    { "@created", DateTimeOffset.Now.ToUnixTimeSeconds() }
                });
            var mapPart = new IdValueInfo
            {
                Id = category.Id,
                Value = category.Name
            };
            await MSU.Execute(
                "UPDATE `books` SET `last_updated` = @lastUpdated, `categories_cache` = concat(ifnull(`categories_cache`,\"\"), @cache) WHERE `id` = @bookId",
                new Dictionary<string, object>
                {
                    { "@lastUpdated", DateTimeOffset.Now.ToUnixTimeSeconds() },
                    { "@cache", MapUtil.CreateMapPart(mapPart) }, { "@bookId", bookId }
                });
        }
        else
        {
            log.Trace("Category {0} already attached to book {1}", category.Name, bookId);
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
        var created = DateTimeOffset.Now.ToUnixTimeSeconds();
        return await MSU.QueryWithCommand(
            "INSERT INTO `categories` (`name`, `link`, `created`) VALUES (@name, @link, @created)",
            new Dictionary<string, object> { { "@name", name }, { "@link", link }, { "@created", created } },
            async (reader, cmd) =>
            {
                return new AudibleCategory
                {
                    Id = (int)cmd.LastInsertedId,
                    Name = name,
                    Link = link,
                    Created = created
                };
            });
    }

    public Task<AudibleCategory?> GetGategoryByName(string name)
    {
        log.Trace("Getting category by name: {0}", name);

        return MSU.Query("SELECT * FROM `categories` WHERE `name` = @name",
            new Dictionary<string, object> { { "@name", name } }, async reader =>
            {
                if (await reader.ReadAsync())
                    return new AudibleCategory
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Link = reader.GetString("link"),
                        Created = reader.GetInt32("created")
                    };
                return null;
            });
    }
}