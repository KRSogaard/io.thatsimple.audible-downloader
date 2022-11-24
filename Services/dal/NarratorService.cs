using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudibleDownloader.Services.dal
{
    public class NarratorService
    {
        public Task<List<AudibleNarrator>> getNarratorsForBook(int bookId)
        {
            throw new NotImplementedException();
        }

        public Task AddNarratorToBook(int bookId, AudibleNarrator savedNarrator)
        {
            throw new NotImplementedException();
        }

        public Task<AudibleNarrator> SaveOrGetNarrator(string narrator)
        {
            throw new NotImplementedException();
        }
    }
}
