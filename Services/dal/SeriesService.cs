using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudibleDownloader.Services.dal
{
    public class SeriesService
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        public Task AddBookToSeries(int bookId, int id, string bookNumber)
        {
            throw new NotImplementedException();
        }

        public Task<AudibleSeries> GetSeriesAsin(string seriesAsin)
        {
            throw new NotImplementedException();
        }

        public Task<AudibleSeries> SaveOrGetSeries(string asin, string name, string link, string? summary)
        {
            throw new NotImplementedException();
        }

        public Task SetSeriesShouldDownload(int id, bool v)
        {
            throw new NotImplementedException();
        }

        public Task UpdateBookNumber(int id1, int id2, string bookNumber)
        {
            throw new NotImplementedException();
        }
    }
}
