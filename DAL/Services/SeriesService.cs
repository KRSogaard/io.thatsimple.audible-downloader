using AudibleDownloader.DAL.Models;
using AudibleDownloader.Utils;
using MySql.Data.MySqlClient;
using NLog;
using AudibleDownloader.Models;
using Microsoft.EntityFrameworkCore;

namespace AudibleDownloader.DAL.Services;

public class SeriesService
{
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public async Task AddBookToSeries(int bookId, int seriesId, string? bookNumber, int? sort)
    {
        log.Trace($"Adding book {bookId} to series {seriesId} with book number {bookNumber} and sort {sort}");
        using (var context = new AudibleContext())
        {
            var seriesBook = context.SeriesBooks.Where(sb => sb.BookId == bookId && sb.SeriesId == seriesId).FirstOrDefault();
            if (seriesBook != null)
            {
                bool changes = false;
                if (seriesBook.BookNumber != bookNumber && !string.IsNullOrEmpty(bookNumber))
                {
                    seriesBook.BookNumber = bookNumber;
                    changes = true;
                }
                if (seriesBook.Sort != sort && sort != null)
                {
                    seriesBook.Sort = sort;
                    changes = true;
                }
                if (changes)
                {
                    await context.SaveChangesAsync();
                }
                return;
            }
            
            seriesBook = new SeriesBook()
            {
                BookId = bookId,
                SeriesId = seriesId,
                BookNumber = string.IsNullOrWhiteSpace(bookNumber) ? null : bookNumber.Trim(),
                Sort = sort,
                Created = DateTimeOffset.Now.ToUnixTimeSeconds()
            };
            await context.SeriesBooks.AddAsync(seriesBook);
            await context.SaveChangesAsync();
        }
    }

    public async Task<AudibleSeries?> GetSeriesAsin(string seriesAsin)
    {
        log.Trace($"Getting series by asin {seriesAsin}");
        using (var context = new AudibleContext())
        {
            return await context.Series.Where(s => s.Asin == seriesAsin).Select(s => s.ToInternal())
                .FirstOrDefaultAsync();
        }
    }

    public async Task<AudibleSeries> SaveOrGetSeries(string asin, string name, string link)
    {
        log.Trace($"Saving or getting series {asin} name {name}");
        var check = await GetSeriesAsin(asin);
        if (check != null)
        {
            return check;
        }
        
        using (var context = new AudibleContext())
        {
            var series = new Series()
            {
                Asin = asin,
                Name = name,
                Link = link,
                Created = DateTimeOffset.Now.ToUnixTimeSeconds(),
                LastUpdated = DateTimeOffset.Now.ToUnixTimeSeconds(),
                LastChecked = DateTimeOffset.Now.ToUnixTimeSeconds(),
                ShouldDownload = true,
            };
            await context.Series.AddAsync(series);
            await context.SaveChangesAsync();
            return series.ToInternal();
        }
    }

    public async Task SetSeriesShouldDownload(int id, bool shouldDownload)
    {
        log.Trace($"Setting series {id} should download to {shouldDownload}");
        using (var context = new AudibleContext())
        {
            var series = await context.Series.Where(s => s.Id == id).FirstOrDefaultAsync();
            if (series == null)
            {
                log.Warn("Series {0} not found, can't update should download", id);
                return;
            }
            series.ShouldDownload = shouldDownload;
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<AudibleSeries>> GetSeriesToRefresh()
    {
        log.Trace("Getting series to be refresh");
        using (var context = new AudibleContext())
        {
            long beforeChecked = DateTimeOffset.Now.AddDays(-7).ToUnixTimeSeconds();
            return await context.Series
                .Where(s => s.LastChecked < beforeChecked)
                .Select(s => s.ToInternal())
                .ToListAsync();
        }
    }

    public async Task SetSeriesChecked(int seriesId)
    {
        log.Trace($"Setting series {seriesId} checked");
        using (var context = new AudibleContext())
        {
            var series = await context.Series.Where(s => s.Id == seriesId).FirstOrDefaultAsync();
            if (series == null)
            {
                log.Warn("Series {0} not found, can't update last checked", seriesId);
                return;
            }
            series.LastChecked = DateTimeOffset.Now.ToUnixTimeSeconds();
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<AudibleSeries>> GetSeriesForBook(int bookId)
    {
        log.Trace($"Getting all series for book {bookId}");
        using (var context = new AudibleContext())
        {
            return await (from s in context.Series
                join sb in context.SeriesBooks on s.Id equals sb.SeriesId
                where sb.BookId == bookId
                select s.ToInternal()).ToListAsync();
        }
    }

    public async Task<bool> IsBookInSeriesByAsin(int bookId, string seriesAsin)
    {
        using (var context = new AudibleContext())
        {
            return await (from s in context.Series
                join sb in context.SeriesBooks on s.Id equals sb.SeriesId
                where sb.BookId == bookId && s.Asin == seriesAsin
                select s).AnyAsync();
        }
    }

    public async Task UpdateSeriesBookData(int bookId, int seriesId, string? bookBookNumber, int? bookSort)
    {
        log.Trace("Updating series book data for book {0} series {1} book number {2} sort {3}", bookId, seriesId, bookBookNumber, bookSort);
        using (var context = new AudibleContext())
        {
            var current = await context.SeriesBooks.Where(sb => sb.BookId == bookId && sb.SeriesId == seriesId).FirstOrDefaultAsync();
            if (current == null)
            {
                log.Warn("Series book {0} {1} not found, can't update", bookId, seriesId);
                return;
            }
            
            bool changes = false;
            if (current.BookNumber != bookBookNumber && bookBookNumber != null)
            {
                current.BookNumber = bookBookNumber;
                changes = true;
            }
            if (current.Sort != bookSort && bookSort != null)
            {
                current.Sort = bookSort;
                changes = true;
            }
            if (changes)
            {
                await context.SaveChangesAsync();
            }
        }
    }
}