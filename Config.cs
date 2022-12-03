using Microsoft.Extensions.Configuration;
using NLog;

namespace AudibleDownloader;

public static class Config {
    private static readonly IConfiguration configuration;

    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    static Config() {
        IConfigurationBuilder builder =
            new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appSettings.json",
                            true,
                            true)
               .AddEnvironmentVariables();
        configuration = builder.Build();
    }

    public static string? Get(string name) {
        string? appSettings = configuration[name];
        if (appSettings == null) log.Warn($"Config {name} is missing");
        return appSettings;
    }

    public static IConfigurationSection GetSection(string name) {
        return configuration.GetSection(name);
    }
}