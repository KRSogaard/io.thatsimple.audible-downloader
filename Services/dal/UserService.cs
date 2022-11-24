using AudibleDownloader.Utils;
using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudibleDownloader.Services.dal
{
    public class UserService
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        public async Task FinishJob(string jobId)
        {
            await MSU.Execute("DELETE FROM `users_jobs` WHERE `id` = @jobId", new Dictionary<string, object> { { "@jobId", jobId } });  
        }

        public Task AddBookToUser(string userId, int bookId)
        {
            throw new NotImplementedException();
        }

        public int? CreateJob(string userId, string type, string data)
        {
            throw new NotImplementedException();
        }
    }
}
