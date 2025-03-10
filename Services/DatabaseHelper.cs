using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.IO;
using Inv_M_Sys.Services;


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

    // Update database credentials (Username and Password)
    public static void UpdateDatabaseSettings(string newUsername, string newPassword)
    {
        try
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = @"UPDATE DatabaseConfig SET Username = @newUsername, Password = @newPassword";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@newUsername", newUsername);
                    cmd.Parameters.AddWithValue("@newPassword", newPassword);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error updating database settings: " + ex.Message);
        }
    }

    // Update owner credentials (Username and Password)
    public static void UpdateOwnerCredentials(string newUsername, string newPassword)
    {
        try
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string hashedPassword = HashPassword(newPassword);

                string query = @"UPDATE Owner SET Username = @newUsername, Password = @newPassword WHERE Id = @ownerId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@newUsername", newUsername);
                    cmd.Parameters.AddWithValue("@newPassword", hashedPassword);
                    cmd.Parameters.AddWithValue("@ownerId", SessionManager.CurrentOwnerId); // Assuming owner is logged in
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error updating owner credentials: " + ex.Message);
        }
    }

    // This method will hash the password using SHA256
    public static string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }

}