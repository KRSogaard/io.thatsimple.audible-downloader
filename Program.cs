using RabbitMQ.Client;

namespace AudibleDownloader {
    class Listener {
        public static void Main(string[] args) {
            Console.WriteLine("Hello, World! \"" + Config.Get("KeyOne") + "\"");
        }
    }
}

