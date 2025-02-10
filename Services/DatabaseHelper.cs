using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.IO;

public static class DatabaseHelper
{
    private static string _connectionString;

    static DatabaseHelper()
    {
        LoadConnectionString();
    }

    private static void LoadConnectionString()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Service", "appsettings.json");

        if (!File.Exists(configPath))
        {
            MessageBox.Show("⚠️ Config file not found: " + configPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        else
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(configPath, optional: false, reloadOnChange: true)
                .Build();

            string host = config["DatabaseConfig:Host"];
            string port = config["DatabaseConfig:Port"];
            string username = config["DatabaseConfig:Username"];
            string password = config["DatabaseConfig:Password"];
            string database = config["DatabaseConfig:Database"];

            _connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database}";
        }
    }

    public static NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    public static void TestConnection()
    {
        try
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                Console.WriteLine("✅ Database connection successful!");
                MessageBox.Show("✅ Database connection successful!", "Database Connected", MessageBoxButton.OK, MessageBoxImage.Information);

                if (conn.State == ConnectionState.Open)
                {
                    try
                    {
                        DatabaseInitializer.InitializeDatabase(conn); // Pass the connection
                        MessageBox.Show("✅ Database Initialized Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ Database Initialization Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Database connection failed: " + ex.Message);
            MessageBox.Show("❌ Database connection failed: " + ex.Message, "Database Connection Faild", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}