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
    private static string _connectionString = ConfigHelper.GetDatabaseConnectionString();

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