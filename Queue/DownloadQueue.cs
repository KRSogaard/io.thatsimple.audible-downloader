using System.Text;
using System.Text.Json;
using NLog;
using RabbitMQ.Client;

namespace AudibleDownloader.Queue;

public class DownloadQueue {
    private readonly string channelName;
    private readonly ConnectionFactory factory;
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public DownloadQueue() {
        string? host = Config.Get("RABBITMQ_HOST");
        string? user = Config.Get("RABBITMQ_USER");
        string? pass = Config.Get("RABBITMQ_PASS");
        channelName = Config.Get("RABBITMQ_AUDIBLE_CHANNEL");

        log.Info($"Creating RabbitMQ Factor with \"{host}\" user \"{user}\" channel \"{channelName}\"");

        factory = new ConnectionFactory {
                                            HostName = host, UserName = user, Password = pass
                                        };
    }

    public Task SendDownloadSeries(string asin, int? jobId, int? userId, bool force = false) {
        return GetChannel(async (channel, channelName) => {
            string message = JsonSerializer.Serialize(new MessageData {
                                                                          Asin = asin,
                                                                          Type = "series",
                                                                          JobId = jobId,
                                                                          UserId = userId,
                                                                          AddToUser = false, // Series can not be added to user
                                                                          Force = force
                                                                      });
            byte[] body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish("",
                                 channelName,
                                 null,
                                 body);
        });
    }

    public Task SendDownloadBook(string asin, int? jobId, int? userId, bool addToUser = false, bool force = false) {
        return GetChannel(async (channel, channelName) => {
            string message = JsonSerializer.Serialize(new MessageData {
                                                                          Asin = asin,
                                                                          Type = "book",
                                                                          JobId = jobId,
                                                                          UserId = userId,
                                                                          AddToUser = addToUser,
                                                                          Force = force
                                                                      });
            byte[] body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish("",
                                 channelName,
                                 null,
                                 body);
        });
    }

    public async Task GetChannel(Func<IModel, string, Task> func) {
        using (IConnection? connection = factory.CreateConnection()) {
            using (IModel? channel = connection.CreateModel()) {
                channel.QueueDeclare(channelName,
                                     false,
                                     false,
                                     false,
                                     null);
                await func(channel, channelName);
            }
        }
    }
}