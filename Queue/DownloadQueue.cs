using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NLog;
using RabbitMQ.Client;

namespace AudibleDownloader.Queue
{
    public class DownloadQueue
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        private ConnectionFactory factory;
        private string channelName;
        
        public DownloadQueue()
        {
            var host = Config.Get("RABBITMQ_HOST");
            var user = Config.Get("RABBITMQ_USER");
            var pass = Config.Get("RABBITMQ_PASS");
            channelName = Config.Get("RABBITMQ_AUDIBLE_CHANNEL");

            log.Info($"Creating RabbitMQ Factor with \"{host}\" user {user} channel {channelName}");

            factory = new ConnectionFactory()
            {
                HostName = host, UserName = user, Password = pass,
            };
        }
        
        public Task SendDownloadSeries(string link, int? jobId, string? userId, bool force = false)
        {
            return GetChannel(async (channel, channelName) =>
            {
                string message = JsonSerializer.Serialize(new MessageData()
                {
                    Url = link,
                    Type = "series",
                    JobId = jobId,
                    UserId = userId,
                    AddToUser = false, // Series can not be added to user
                    Force = force
                });
                var body = Encoding.UTF8.GetBytes(message);
                
                channel.BasicPublish(exchange: "",
                    routingKey: channelName,
                    basicProperties: null,
                    body: body);
            });
        }

        public Task SendDownloadBook(string link, int? jobId, string? userId, bool addToUser = false, bool force = false)
        {
            return GetChannel(async (channel, channelName) =>
            {
                string message = JsonSerializer.Serialize(new MessageData()
                {
                    Url = link,
                    Type = "book",
                    JobId = jobId,
                    UserId = String.IsNullOrWhiteSpace(userId) ? null : userId,
                    AddToUser = addToUser,
                    Force = force
                });
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                    routingKey: channelName,
                    basicProperties: null,
                    body: body);
            });
        }

        public async Task GetChannel(Func<IModel, string, Task> func)
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: channelName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                await func(channel, channelName);
            }
        }
    }
}
