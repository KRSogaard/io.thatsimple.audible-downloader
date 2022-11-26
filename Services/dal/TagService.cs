using NLog;
using System.Text.Json;
using AudibleDownloader.Utils;

namespace AudibleDownloader.Services.dal
{
    public class TagService
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        public Task<List<AudibleTag>> GetTagsForBook(int bookId)
        {
            return MSU.Query(
                "SELECT `tags`.* FROM `tags` LEFT JOIN `tags_books` ON `tags_books`.tag_id = `tags`.id WHERE `tags_books`.book_id = @bookId",
                new Dictionary<string, object>() { { "@bookId", bookId } }, async reader =>
                {
                    List<AudibleTag> tags = new List<AudibleTag>();
                    while (await reader.ReadAsync())
                    {
                        tags.Add(new AudibleTag()
                        {
                            Id = reader.GetInt32("id"),
                            Tag = reader.GetString("tag"),
                            Created = reader.GetInt64("created"),
                        });
                    }

                    return tags;
                });
        }

        public async Task AddTagToBook(int bookId, AudibleTag savedTag)
        {
            bool check = await MSU.Query("SELECT * FROM `tags_books` WHERE `book_id` = @bookId AND `tag_id` = @tagId",
                new Dictionary<string, object>()
                {
                    { "@bookId", bookId },
                    { "@tagId", savedTag.Id }
                }, async reader =>
                {
                    return await reader.ReadAsync();
                });
            if (check)
            {
                log.Trace("Tag {0} already exists for book {1}", savedTag.Tag, bookId);
                return;
            }
            
            await MSU.Execute("INSERT INTO `tags_books` (`book_id`, `tag_id`, `created`) VALUES (@bookId, @tagId, @created)",
                new Dictionary<string, object>()
                {
                    { "@bookId", bookId },
                    { "@tagId", savedTag.Id },
                    { "@created", DateTimeOffset.Now.ToUnixTimeSeconds() }
                });

            IdValueInfo mapPart = new IdValueInfo()
            {
                Id = savedTag.Id,
                Value = savedTag.Tag
            };
            await MSU.Execute("UPDATE `books` SET `last_updated` = @lastUpdated, `tags_cache` = concat(ifnull(`tags_cache`,\"\"), @cache) WHERE `id` = @id",
                new Dictionary<string, object>()
                {
                    { "@id", bookId },
                    { "@lastUpdated", DateTimeOffset.Now.ToUnixTimeSeconds() },
                    { "@cache", JsonSerializer.Serialize(mapPart) }
                });
        }

        public async Task<AudibleTag> SaveOrGetTag(string tag)
        {
            log.Trace("Saving tag {0}", tag);

            AudibleTag existingTag = await GetTagByName(tag);
            if (existingTag != null)
            {
                log.Trace("Tag {0} already exists", tag);
                return existingTag;
            }

            long time = DateTimeOffset.Now.ToUnixTimeSeconds();
            return await MSU.QueryWithCommand("INSERT INTO `tags` (`tag`, `created`) VALUES (@tag, @created)",
                new Dictionary<string, object>()
                {
                    { "@tag", tag },
                    { "@created", time }
                }, async (reader, cmd) =>
                {
                    return new AudibleTag()
                    {
                        Id = (int)cmd.LastInsertedId,
                        Tag = tag,
                        Created = time
                    };
                });
        }

        private Task<AudibleTag?> GetTagByName(string tag)
        {
            return MSU.Query("SELECT * FROM `tags` WHERE `tag` = @tag",
                new Dictionary<string, object>() { { "@tag", tag } }, async reader =>
                {
                    if (await reader.ReadAsync())
                    {
                        return new AudibleTag()
                        {
                            Id = reader.GetInt32("id"),
                            Tag = reader.GetString("tag"),
                            Created = reader.GetInt64("created"),
                        };
                    }

                    return null;
                });
        }
    }
}
