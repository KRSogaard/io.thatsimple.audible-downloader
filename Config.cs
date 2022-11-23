using System.IO;
using Microsoft.Extensions.Configuration;
using NLog;

namespace AudibleDownloader
{
    public static class Config
    {
        private static IConfiguration configuration;

        private static Logger log = LogManager.GetCurrentClassLogger();

        static Config()
        {
            var builder =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appSettings.json",
                    optional: true,
                    reloadOnChange: true)
                    .AddJsonFile("appSettings.Development.json",
                    optional: true,
                    reloadOnChange: true);
            configuration = builder.Build();
        }

        public static string? Get(string name)
        {
            string? appSettings = configuration[name];
            if (appSettings == null)
            {
                log.Warn($"Config {name} is missing");
            }
            return appSettings;
        }

        public static IConfigurationSection GetSection(string name)
        {
            return configuration.GetSection(name);
        }
    }
}
