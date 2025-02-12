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
                    if (!IsDatabaseInitialized()) // Only initialize if not already initialized
                    {
                        try
                        {
                            MessageBox.Show("🔹 First-time setup: Initializing database...");
                            DatabaseInitializer.InitializeDatabase(conn);
                            MessageBox.Show("✅ Database Initialized Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"❌ Database Initialization Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        Console.WriteLine("✅ Database already initialized. Skipping setup.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Database connection failed: " + ex.Message);
            MessageBox.Show("❌ Database connection failed: " + ex.Message, "Database Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static bool IsDatabaseInitialized()
    {
        try
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'", conn))
                {
                    int tableCount = Convert.ToInt32(cmd.ExecuteScalar());
                    return tableCount > 0; // If tables exist, the database is initialized
                }
            }
        }
        catch (Exception)
        {
            return false; // If connection fails, assume DB is not initialized
        }
    }
}