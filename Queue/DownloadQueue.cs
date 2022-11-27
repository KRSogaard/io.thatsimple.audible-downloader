using System.Text;
using System.Text.Json;
using NLog;
using RabbitMQ.Client;

namespace AudibleDownloader.Queue;

public class DownloadQueue
{
    private readonly string channelName;
    private readonly ConnectionFactory factory;
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public DownloadQueue()
    {
        var host = Config.Get("RABBITMQ_HOST");
        var user = Config.Get("RABBITMQ_USER");
        var pass = Config.Get("RABBITMQ_PASS");
        channelName = Config.Get("RABBITMQ_AUDIBLE_CHANNEL");

        log.Info($"Creating RabbitMQ Factor with \"{host}\" user \"{user}\" channel \"{channelName}\"");

        factory = new ConnectionFactory
        {
            HostName = host, UserName = user, Password = pass
        };
    }

    public Task SendDownloadSeries(string link, int? jobId, string? userId, bool force = false)
    {
        return GetChannel(async (channel, channelName) =>
        {
            var message = JsonSerializer.Serialize(new MessageData
            {
                Url = link,
                Type = "series",
                JobId = jobId,
                UserId = userId,
                AddToUser = false, // Series can not be added to user
                Force = force
            });
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish("",
                channelName,
                null,
                body);
        });
    }

    public Task SendDownloadBook(string link, int? jobId, string? userId, bool addToUser = false, bool force = false)
    {
        return GetChannel(async (channel, channelName) =>
        {
            var message = JsonSerializer.Serialize(new MessageData
            {
                Url = link,
                Type = "book",
                JobId = jobId,
                UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
                AddToUser = addToUser,
                Force = force
            });
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish("",
                channelName,
                null,
                body);
        });
    }

    public async Task GetChannel(Func<IModel, string, Task> func)
    {
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(channelName,
                false,
                false,
                false,
                null);
            await func(channel, channelName);
        }
    }
}