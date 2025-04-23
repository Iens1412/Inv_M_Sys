using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog; // Add Serilog if logging is being used globally

/// <summary>
/// A helper class for loading and accessing configuration settings from appsettings.json.
/// </summary>
public static class ConfigHelper
{
    private static IConfigurationRoot _config;

    /// <summary>
    /// Static constructor to initialize configuration loading at application startup.
    /// </summary>
    static ConfigHelper()
    {
        LoadConfiguration();
    }

    /// <summary>
    /// Loads the configuration from Services/appsettings.json file.
    /// Throws FileNotFoundException if the file does not exist.
    /// </summary>
    private static void LoadConfiguration()
    {
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "appsettings.json");

        if (!File.Exists(configPath))
        {
            string error = $"Configuration file not found: {configPath}";
            Log.Error(error); // Optional: if Serilog is used in your project
            throw new FileNotFoundException(error);
        }

        _config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(configPath, optional: false, reloadOnChange: true)
            .Build();
    }

    /// <summary>
    /// Retrieves the PostgreSQL database connection string from the configuration.
    /// </summary>
    /// <returns>A PostgreSQL connection string constructed from appsettings.json values.</returns>
    public static string GetDatabaseConnectionString()
    {
        try
        {
            string host = _config["DatabaseConfig:Host"];
            string port = _config["DatabaseConfig:Port"];
            string username = _config["DatabaseConfig:Username"];
            string password = _config["DatabaseConfig:Password"];
            string database = _config["DatabaseConfig:Database"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(port) ||
                string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(database))
            {
                string msg = "One or more database configuration values are missing in appsettings.json.";
                Log.Error(msg); // Optional
                throw new InvalidOperationException(msg);
            }

            return $"Host={host};Port={port};Username={username};Password={password};Database={database}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to build the database connection string.");
            throw;
        }
    }
}
