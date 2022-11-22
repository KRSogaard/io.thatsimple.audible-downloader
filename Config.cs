using System.IO;
using Microsoft.Extensions.Configuration;

namespace AudibleDownloader
{
    public static class Config
    {
        private static IConfiguration configuration;

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

        public static string Get(string name)
        {
            string appSettings = configuration[name];
            return appSettings;
        }

        public static IConfigurationSection GetSection(string name)
        {
            return configuration.GetSection(name);
        }
    }
}
