﻿using AudibleDownloader.Utils;
using MySql.Data.MySqlClient;
using NLog;
using AudibleDownloader.Models;

namespace AudibleDownloader.DAL.Services;

public class NarratorService
{
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public Task<List<AudibleNarrator>> getNarratorsForBook(int bookId)
    {
        return MSU.Query(
            "SELECT n.* FROM `narrators` AS n LEFT JOIN `narrators_books` AS nb ON nb.narrator_id = n.id WHERE nb.book_id = @bookId",
            new Dictionary<string, object> { { "@bookId", bookId } }, async reader =>
            {
                var narrators = new List<AudibleNarrator>();

                while (await reader.ReadAsync()) narrators.Add(ParseNarratorResult(reader));

                return narrators;
            });
    }

    private AudibleNarrator ParseNarratorResult(MySqlDataReader reader)
    {
        return new AudibleNarrator
        {
            Id = reader.GetInt32("id"),
            Name = reader.GetString("name"),
            Created = reader.GetInt32("created")
        };
    }

    public async Task AddNarratorToBook(int bookId, AudibleNarrator narrator)
    {
        var exists = await MSU.Query(
            "SELECT * FROM `narrators_books` WHERE `book_id` = @bookId AND `narrator_id` = @narratorId",
            new Dictionary<string, object> { { "@bookId", bookId }, { "@narratorId", narrator.Id } },
            async reader => { return await reader.ReadAsync(); });
        if (!exists)
        {
            log.Trace("Adding narrator {0} to book {1}", narrator.Id, bookId);
            await MSU.Execute(
                "INSERT INTO `narrators_books` (`book_id`, `narrator_id`, `created`) VALUES (@bookId, @narratorId, @created)",
                new Dictionary<string, object>
                {
                    { "@bookId", bookId }, { "@narratorId", narrator.Id },
                    { "@created", DateTimeOffset.Now.ToUnixTimeSeconds() }
                });
            var mapPart = new IdValueInfo
            {
                Id = narrator.Id,
                Value = narrator.Name
            };
            await MSU.Execute(
                "UPDATE `books` SET `last_updated` = @lastUpdated, `narrators_cache` = concat(ifnull(`narrators_cache`,\"\"), @cache) WHERE `id` = @bookId",
                new Dictionary<string, object>
                {
                    { "@lastUpdated", DateTimeOffset.Now.ToUnixTimeSeconds() },
                    { "@cache", MapUtil.CreateMapPart(mapPart) }, { "@bookId", bookId }
                });
        }
        else
        {
            log.Trace("Narrator {0} already attached to book {1}", narrator.Name, bookId);
        }
    }

    public async Task<AudibleNarrator> SaveOrGetNarrator(string narrator)
    {
        log.Trace("Saving narrator {0}", narrator);

        var check = await GetNarratorByName(narrator);
        if (check != null)
        {
            log.Debug("Narrator {0} already exists", narrator);
            return check;
        }

        log.Info("Saving new narrator {0}", narrator);
        var time = DateTimeOffset.Now.ToUnixTimeSeconds();
        return await MSU.QueryWithCommand("INSERT INTO `narrators` (`name`, `created`) VALUES (@name, @created)",
            new Dictionary<string, object>
            {
                { "@name", narrator },
                { "@created", time }
            }, async (reader, command) =>
            {
                return new AudibleNarrator
                {
                    Id = (int)command.LastInsertedId,
                    Name = narrator,
                    Created = time
                };
            });
    }

    public Task<AudibleNarrator?> GetNarratorByName(string name)
    {
        log.Trace("Getting narrator by name");

        return MSU.Query("SELECT * FROM `narrators` WHERE `name` = @name",
            new Dictionary<string, object> { { "@name", name } },
            async reader =>
            {
                if (!await reader.ReadAsync()) return null;

                return new AudibleNarrator
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Created = reader.GetInt32("created")
                };
            });
    }
}