using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NLog;
using System.Text;
using System.Text.Json.Nodes;
using AudibleDownloader.Exceptions;
using AudibleDownloader.Services.dal;

namespace AudibleDownloader {
    class Listener {
        public static void Main(string[] args) {
            new Listener().Run();
        }

        Logger log = LogManager.GetCurrentClassLogger();
        bool shutDown = false;
        AudibleDownloadManager audibleDownloader;
        UserService userService;

        public Listener()
        {
            Precondition(Config.Get("LISTENER_THREADS"), "Config LISTENER_THREADS is missing");
            if(!int.TryParse(Config.Get("LISTENER_THREADS"), out int threads))
            {
                Precondition(false, "Config LISTENER_THREADS is not a number");
            }
            Precondition(Config.Get("RABBITMQ_HOST"), "Config RABBITMQ_HOST is missing");
            Precondition(Config.Get("RABBITMQ_USER"), "Config RABBITMQ_USER is missing");
            Precondition(Config.Get("RABBITMQ_PASS"), "Config RABBITMQ_PASS is missing");
            Precondition(Config.Get("RABBITMQ_AUDIBLE_CHANNEL"), "Config RABBITMQ_AUDIBLE_CHANNEL is missing");


            Precondition(Config.Get("DB_HOST"), "Config DB_HOST is missing");
            Precondition(Config.Get("DB_PORT"), "Config DB_PORT is missing");
            Precondition(Config.Get("DB_USER"), "Config DB_USER is missing");
            Precondition(Config.Get("DB_PASSWORD"), "Config DB_PASSWORD is missing");
            Precondition(Config.Get("DB_NAME"), "Config DB_NAME is missing");

            //audibleDownloader = new AudibleDownloadManager();
            userService = new UserService();
        }

        public void Run() {
            int threads = int.Parse(Config.Get("LISTENER_THREADS"));
            log.Info($"Starting {threads} listeners");
            List<Task> tasks = new List<Task>();
            for(int i = 0; i < threads; i++) {
                tasks.Add(Task.Run(() => {
                    CreateListener();
                }));
            }
            Task.WaitAll(tasks.ToArray());
            // log.Info("Starting up");

            // var config = Configuration.Default.WithDefaultLoader();
            // var address = "https://en.wikipedia.org/wiki/List_of_The_Big_Bang_Theory_episodes";
            // var context = BrowsingContext.New(config);
            // var document = await context.OpenAsync(address);
            // var cellSelector = "tr.vevent td:nth-child(3)";
            // var cells = document.QuerySelectorAll(cellSelector);
            // var titles = cells.Select(m => m.TextContent);

            // log.Fatal("Done");
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

        void CreateListener() {
            var host = Config.Get("RABBITMQ_HOST");
            var user = Config.Get("RABBITMQ_USER");
            var pass = Config.Get("RABBITMQ_PASS");
            var channelName = Config.Get("RABBITMQ_AUDIBLE_CHANNEL");

            log.Info($"Creating RabbitMQ listerner with \"{host}\" user {user} channel {channelName}");

            var factory = new ConnectionFactory() { HostName = host, UserName = user, Password = pass };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: channelName,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    try {
                        log.Debug($"Received {message}, Redelivery? {ea.Redelivered}");

                        await OnMessage(message);

                        channel.BasicAck(ea.DeliveryTag, false);
                    } catch (Exception e) {
                        log.Info($"Redeliver: {ea.Redelivered} and {e is FatalException}, {e.GetType().Name}");
                        bool redeliver = true;
                        if (ea.Redelivered || e is FatalException) {
                            redeliver = false;
                        }

                        if (e is FatalException) {
                            log.Fatal("Failed to process message \"{0}\" will redeliver? {1}", message, redeliver);
                        } else {
                            log.Fatal(e, "Failed to process message \"{0}\" will redeliver? {1}:", message, redeliver);
                        }
                        channel.BasicNack(ea.DeliveryTag, false, redeliver);
                    }
                };
                channel.BasicConsume(queue: channelName,
                                    autoAck: false,
                                    consumer: consumer);
                while(!shutDown) {
                    Task.Delay(1000).Wait();
                }
            }
        }

        async Task OnMessage(String message) {
            JsonNode? jsonParse;
            try {
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
            } catch (Exception e) {
                log.Error(e, "Failed to parse message \"{0}\"", message);
                throw new FatalException("Failed to parse message");
            }
        }
    }
}
