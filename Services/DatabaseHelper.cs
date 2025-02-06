using System;
using System.Data;
using System.Windows;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;

public static class DatabaseHelper
{
    private static string _connectionString;
    static DatabaseHelper()
    {
        LoadConnectionString();
    }

    private static void LoadConnectionString()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "appsettings.json"); 

        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(configPath, optional: false, reloadOnChange: true)
            .Build();

        _connectionString = config.GetConnectionString("PostgresDb");

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
                MessageBox.Show("✅ Database connection successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Database connection failed: " + ex.Message);
            MessageBox.Show("❌ Database connection failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}