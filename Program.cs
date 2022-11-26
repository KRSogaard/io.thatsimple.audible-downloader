using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NLog;
using System.Text;
using System.Text.Json.Nodes;
using AudibleDownloader.Exceptions;
using AudibleDownloader.Parser;
using AudibleDownloader.Queue;
using AudibleDownloader.Services;
using AudibleDownloader.Services.dal;

namespace AudibleDownloader {
    class Listener {
        public static void Main(string[] args) {
            new Listener().Run().Wait();
        }

        Logger log = LogManager.GetCurrentClassLogger();
        bool shutDown = false;
        private AudibleDownloadManager audibleDownloader;
        private BookService bookService;
        private AuthorService authorService;
        private NarratorService narratorService;
        private CategoryService categoryService;
        private TagService tagService;
        private SeriesService seriesService;
        private UserService userService;
        private StorageService storageService;
        private DownloadService downloadService;
        private DownloadQueue downloadQueue;

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
            if(!int.TryParse(Config.Get("LISTENER_THREADS"), out int threads))
            {
                Precondition(false, "Config LISTENER_THREADS is not a number");
            }
           

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
            
            audibleDownloader = new AudibleDownloadManager(
                bookService, authorService, narratorService,
                categoryService, tagService, seriesService, userService,
                storageService, downloadService, downloadQueue);
        }

        public async Task Run()
        {
            CreateListener();

            // int threads = int.Parse(Config.Get("LISTENER_THREADS"));
            // log.Info($"Starting {threads} listeners");
            // List<Task> tasks = new List<Task>();
            // for(int i = 0; i < threads; i++) {
            //     tasks.Add(Task.Run(() => {
            //         CreateListener();
            //     }));
            // }
            // Task.WaitAll(tasks.ToArray());
        }

        private void Precondition(Object? obj, string message) {
            if (obj == null) {
                log.Fatal(message);
                throw new ArgumentNullException(message);
            }
            if (obj is string && string.IsNullOrWhiteSpace((string)obj)) {
                log.Fatal(message);
                throw new ArgumentException(message);
            }
        }

        void CreateListener()
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
                        bool redeliver = !(ea.Redelivered || e is FatalException);
                        log.Fatal(e, "Got unknown exception while processing message. Will redeliver? {0}", redeliver);
                        channel.BasicNack(ea.DeliveryTag, false, redeliver);
                    }
                };
                // This kicks off the reading from the queue
                channel.BasicConsume(queue: channelName,
                    autoAck: false,
                    consumer: consumer);
                while (!shutDown)
                {
                    Task.Delay(1000).Wait();
                }
            }).Wait();
        }

        async Task OnMessage(String message) {
            JsonNode? jsonParse;
            
            jsonParse = JsonNode.Parse(message);
            if (jsonParse == null){
                log.Error("Failed to parse message \"{0}\"", message);
                throw new FatalException("Failed to parse message");
            }
            var jsonObject = jsonParse.AsObject();


            var userIdObj = jsonObject["userId"];
            var addToUserObj = jsonObject["addToUser"];
            var forceObj = jsonObject["force"];

            string? jobId = jsonObject["jobId"]?.ToString();
            string? userId = jsonObject["userId"]?.ToString();
            bool addTouser = false;
            bool force = false;
            if (jsonObject["addToUser"] != null) {
                addTouser = jsonObject["addToUser"]?.ToString().ToLower() == "true";
            }
            if (jsonObject["force"] != null) {
                force = jsonObject["force"]?.ToString().ToLower() == "true";
            }

            var type = jsonObject["type"]?.ToString();
            if (type == null) {
                log.Error("Failed to parse message \"{0}\" missing url", message);
                throw new FatalException("Failed to parse message");
            }

            var url = jsonObject["url"]?.ToString().Split('?')[0];
            if (url == null) {
                log.Error("Failed to parse message \"{0}\" missing url", message);
                throw new FatalException("Failed to parse message");
            }

            switch(type.Trim()) {
                case "book":
                    await audibleDownloader.DownloadBook(url, userId, addTouser, force);
                    break;
                case "series":
                    await audibleDownloader.DownloadSeries(url, userId, force);
                    break;
                default:
                    log.Error("Unknown message type \"{0}\"", type);
                    throw new FatalException("Unknown message type");
            }
            if (jobId != null) {
                await userService.FinishJob(jobId);
            }
        }
    }
}
