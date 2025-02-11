using System;
using System.IO;
using Microsoft.Extensions.Configuration;

public static class ConfigHelper
{
    private static IConfigurationRoot _config;

    static ConfigHelper()
    {
        LoadConfiguration();
    }

    private static void LoadConfiguration()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "appsettings.json");

        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"❌ Config file not found: {configPath}");
        }

        _config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(configPath, optional: false, reloadOnChange: true)
            .Build();
    }

    public static string GetDatabaseConnectionString()
    {
        string host = _config["DatabaseConfig:Host"];
        string port = _config["DatabaseConfig:Port"];
        string username = _config["DatabaseConfig:Username"];
        string password = _config["DatabaseConfig:Password"];
        string database = _config["DatabaseConfig:Database"];

        return $"Host={host};Port={port};Username={username};Password={password};Database={database}";
    }
}