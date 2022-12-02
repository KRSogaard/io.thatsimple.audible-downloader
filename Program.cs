using System.Text;
using System.Text.Json.Nodes;
using AudibleDownloader.Exceptions;
using AudibleDownloader.Parser;
using AudibleDownloader.Queue;
using AudibleDownloader.Services;
using AudibleDownloader.Services.dal;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AudibleDownloader;

internal class Listener
{
    private static readonly TimeSpan RefreshTimerSeconds = TimeSpan.FromMinutes(60);
    private readonly AudibleDownloadManager audibleDownloader;
    private readonly AuthorService authorService;
    private readonly BookService bookService;
    private readonly CategoryService categoryService;
    private readonly DownloadQueue downloadQueue;
    private readonly DownloadService downloadService;
    private readonly Logger log = LogManager.GetCurrentClassLogger();
    private readonly NarratorService narratorService;
    private readonly SeriesService seriesService;
    private readonly bool shutDown = false;
    private readonly StorageService storageService;
    private readonly TagService tagService;
    private readonly UserService userService;
    private readonly AudibleDataGetter audibleDataGetter;

    public Listener()
    {
        Precondition(Config.Get("RABBITMQ_HOST"), "Config RABBITMQ_HOST is missing");
        Precondition(Config.Get("RABBITMQ_USER"), "Config RABBITMQ_USER is missing");
        Precondition(Config.Get("RABBITMQ_PASS"), "Config RABBITMQ_PASS is missing");
        Precondition(Config.Get("RABBITMQ_AUDIBLE_CHANNEL"), "Config RABBITMQ_AUDIBLE_CHANNEL is missing");

        Precondition(Config.Get("MINIO_END_POINT"), "Config MINIO_END_POINT is missing");
        Precondition(Config.Get("MINIO_ACCESS_KEY"), "Config MINIO_ACCESS_KEY is missing");
        Precondition(Config.Get("MINIO_SECRET_KEY"), "Config MINIO_SECRET_KEY is missing");

        Precondition(Config.Get("DB_HOST"), "Config DB_HOST is missing");
        Precondition(Config.Get("DB_PORT"), "Config DB_PORT is missing");
        Precondition(Config.Get("DB_USER"), "Config DB_USER is missing");
        Precondition(Config.Get("DB_PASSWORD"), "Config DB_PASSWORD is missing");
        Precondition(Config.Get("DB_NAME"), "Config DB_NAME is missing");

        Precondition(Config.Get("PROXY_LIST"), "Config PROXY_LIST is missing");

        Precondition(Config.Get("LISTENER_THREADS"), "Config LISTENER_THREADS is missing");
        if (!int.TryParse(Config.Get("LISTENER_THREADS"), out var threads))
            Precondition(false, "Config LISTENER_THREADS is not a number");


        authorService = new AuthorService();
        narratorService = new NarratorService();
        categoryService = new CategoryService();
        tagService = new TagService();
        seriesService = new SeriesService();
        userService = new UserService();
        storageService = new StorageService();
        downloadQueue = new DownloadQueue();
        bookService = new BookService(authorService, narratorService, categoryService, tagService);
        downloadService = new DownloadService(storageService);
        audibleDataGetter = new AudibleAPIDataGetter(downloadService);

        audibleDownloader = new AudibleDownloadManager(
            bookService, authorService, narratorService,
            categoryService, tagService, seriesService, userService,
            storageService, downloadService, downloadQueue, audibleDataGetter);
    }

    public static void Main(string[] args)
    {
        new Listener().Run().Wait();
    }

    public async Task Run()
    {
        //CreateListener();

        var threads = int.Parse(Config.Get("LISTENER_THREADS"));
        log.Info($"Starting {threads} listeners");
        var tasks = new List<Task>();
        for (var i = 0; i < threads; i++)
            tasks.Add(Task.Run(() => { CreateListener(); }));

        var refreshTimer = new Timer(state => OnRefreshSeries().Wait(),
            null, TimeSpan.FromSeconds(5), RefreshTimerSeconds);
        Task.WaitAll(tasks.ToArray());
    }

    private async Task OnRefreshSeries()
    {
        try
        {
            log.Debug("Refreshing series");
            var series = await seriesService.GetSeriesToRefresh();
            if (series.Count == 0)
                return;

            log.Info("Found {0} series to refresh", series.Count);
            foreach (var s in series)
            {
                log.Debug("Sending series {0} to be refreshed", s.Name);
                await downloadQueue.SendDownloadSeries(s.Link, null, null);
            }
        } catch (Exception e)
        {
            log.Error(e, "Error while refreshing series");
        }
    }

    private void Precondition(object? obj, string message)
    {
        if (obj == null)
        {
            log.Fatal(message);
            throw new ArgumentNullException(message);
        }

        if (obj is string && string.IsNullOrWhiteSpace((string)obj))
        {
            log.Fatal(message);
            throw new ArgumentException(message);
        }
    }

    private void CreateListener()
    {
        downloadQueue.GetChannel(async (channel, channelName) =>
        {
            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                try
                {
                    log.Debug($"Received {message.Replace("\n", " ")}, Redelivery? {ea.Redelivered}");

                    await OnMessage(message);

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (RetryableException e)
                {
                    log.Warn(e, "Got a retryable exception, requeueing message");
                    channel.BasicNack(ea.DeliveryTag, false, true);
                }
                catch (FatalException e)
                {
                    log.Fatal(e, "Got a fatal exception, not requeueing message");
                    channel.BasicNack(ea.DeliveryTag, false, false);
                }
                catch (Exception e)
                {
                    var redeliver = !(ea.Redelivered || e is FatalException);
                    log.Fatal(e, "Got unknown exception while processing message. Will redeliver? {0}",
                        redeliver);
                    channel.BasicNack(ea.DeliveryTag, false, redeliver);
                }
            };
            // This kicks off the reading from the queue
            channel.BasicConsume(channelName,
                false,
                consumer);
            while (!shutDown) Task.Delay(1000).Wait();
        }).Wait();
    }

    private async Task OnMessage(string message)
    {
        JsonNode? jsonParse;

        jsonParse = JsonNode.Parse(message);
        if (jsonParse == null)
        {
            log.Error("Failed to parse message \"{0}\"", message);
            throw new FatalException("Failed to parse message");
        }

        var jsonObject = jsonParse.AsObject();


        var userIdObj = jsonObject["userId"];
        var addToUserObj = jsonObject["addToUser"];
        var forceObj = jsonObject["force"];

        var jobId = jsonObject["jobId"]?.ToString();
        var userId = jsonObject["userId"]?.ToString();
        var addTouser = false;
        var force = false;
        if (jsonObject["addToUser"] != null) addTouser = jsonObject["addToUser"]?.ToString().ToLower() == "true";
        if (jsonObject["force"] != null) force = jsonObject["force"]?.ToString().ToLower() == "true";

        var type = jsonObject["type"]?.ToString();
        if (type == null)
        {
            log.Error("Failed to parse message \"{0}\" missing url", message);
            throw new FatalException("Failed to parse message");
        }

        var asin = jsonObject["asin"].ToString().Trim();
        if (asin == null)
        {
            log.Error("Failed to parse message \"{0}\" missing asin", message);
            throw new FatalException("Failed to parse message");
        }

        try
        {
            switch (type.Trim())
            {
                case "book":
                    await audibleDownloader.DownloadBook(asin, userId, addTouser, force);
                    break;
                case "series":
                    await audibleDownloader.DownloadSeries(asin, userId, force);
                    break;
                default:
                    log.Error("Unknown message type \"{0}\"", type);
                    throw new FatalException("Unknown message type");
            }

            if (jobId != null) await userService.FinishJob(jobId);
        }
        catch (FatalException e)
        {
            log.Info("Delete job {0} as it cause a fatal exception", jobId);
            await userService.FinishJob(jobId);
        }
    }
}