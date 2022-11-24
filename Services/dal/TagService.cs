using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudibleDownloader.Services.dal
{
    public class TagService
    {
        public Task<List<AudibleTag>> getTagsForBook(int bookId)
        {
            throw new NotImplementedException();
        }

        public Task AddTagToBook(int bookId, AudibleTag savedTag)
        {
            throw new NotImplementedException();
        }

        public Task<AudibleTag> SaveOrGetTag(string tag)
        {
            throw new NotImplementedException();
        }
    }
}
