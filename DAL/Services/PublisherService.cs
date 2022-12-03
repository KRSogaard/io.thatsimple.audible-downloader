using AudibleDownloader.Models;
using AudibleDownloader.Utils;
using Microsoft.EntityFrameworkCore;
using NLog;
using AudibleDownloader.DAL.Models;
using AudibleDownloader.Exceptions;
using AudibleDownloader.Protobuf;

namespace AudibleDownloader.DAL.Services
{
    public class PublisherService
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();


        public async Task<AudiblePublisher?> GetAuthor(int id)
        {
            log.Trace("Getting publisher by id: {0}", id);
            using (var context = new AudibleContext())
            {
                return await context.Publishers
                    .Where(a => a.Id == id)
                    .Select(a => a.ToInternal())
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<List<AudiblePublisher>> GetPublisherForBook(int bookId)
        {
            log.Trace("Getting authors by book with id: {0}", bookId);
            using (var context = new AudibleContext())
            {
                return await (from p in context.Publishers
                    join b in context.Books on p.Id equals b.PublisherId
                    where b.Id == bookId
                    select p.ToInternal()).ToListAsync();
            }
        }

        public async Task<AudiblePublisher> SaveOrGetPublisher(string name)
        {
            log.Trace("Saving or getting publisher {0}", name);
            Preconditions.CheckNotNullOrEmpty(name, nameof(name));
            
            AudiblePublisher? check = await GetPublisherByName(name);

            if (check != null)
            {
                log.Trace("Publisher {0} already exists", name);
                return check;
            }

            log.Info("Creating new publisher {0}", name);
            using (var context = new AudibleContext())
            {
                var publisherDal = new Publisher()
                {
                    Name = name,
                    Created = DateTimeOffset.Now.ToUnixTimeSeconds()
                };
                await context.Publishers.AddAsync(publisherDal);
                await context.SaveChangesAsync();
                return publisherDal.ToInternal();
            }
        }

        private async Task<AudiblePublisher?> GetPublisherByName(string name)
        {
            using(var context = new AudibleContext())
            {
                return await context.Publishers
                    .Where(a => a.Name == name)
                    .Select(a => a.ToInternal())
                    .FirstOrDefaultAsync();
            }
        }
    }
}