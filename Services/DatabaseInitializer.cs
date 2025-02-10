using System;
using Npgsql;
using System.Security.Cryptography;
using System.Text;

public static class DatabaseInitializer
{
    public static void InitializeDatabase(NpgsqlConnection conn)
    {
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;

            // Categories Table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Categories (
                Id SERIAL PRIMARY KEY,
                Name TEXT NOT NULL
            );";
            cmd.ExecuteNonQuery();

            // Customers Table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Customers (
                Id SERIAL PRIMARY KEY,
                CompanyName TEXT,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                Email TEXT,
                PhoneNumber TEXT,
                Address TEXT,
                Notes TEXT
            );";
            cmd.ExecuteNonQuery();

            // Notifications Table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Notifications (
                Id SERIAL PRIMARY KEY,
                Title TEXT NOT NULL,
                Content TEXT NOT NULL,
                Date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );";
            cmd.ExecuteNonQuery();

            // Orders Table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Orders (
                Id SERIAL PRIMARY KEY,
                Products TEXT NOT NULL,
                Quantity INTEGER NOT NULL,
                Price NUMERIC NOT NULL,
                TotalPrice NUMERIC NOT NULL
            );";
            cmd.ExecuteNonQuery();

            // Products Table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Products (
                Id SERIAL PRIMARY KEY,
                ProductName TEXT NOT NULL,
                Category TEXT NOT NULL,
                Quantity INTEGER NOT NULL,
                Price NUMERIC NOT NULL,
                MinQuantity INTEGER NOT NULL,
                Supplier TEXT NOT NULL,
                Description TEXT
            );";
            cmd.ExecuteNonQuery();

            // Reports Table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Reports (
                Id SERIAL PRIMARY KEY,
                ReportTitle TEXT NOT NULL,
                ReportType TEXT NOT NULL,
                Details TEXT,
                StartDate DATE,
                EndDate DATE,
                Status TEXT,
                Date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );";
            cmd.ExecuteNonQuery();

            // Restock Table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Restock (
                Id SERIAL PRIMARY KEY,
                ProductName TEXT NOT NULL,
                Quantity INTEGER NOT NULL,
                DateOf DATE,
                Supplier TEXT NOT NULL,
                Status TEXT NOT NULL,
                Notes TEXT
            );";
            cmd.ExecuteNonQuery();

            // Users Table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Users (
                Id SERIAL PRIMARY KEY,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                Email TEXT NOT NULL,
                PhoneNumber TEXT NOT NULL,
                Username TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                Address TEXT,
                Role TEXT NOT NULL
            );";
            cmd.ExecuteNonQuery();

            // Logs Tables
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS SalesLogs (
                Id SERIAL PRIMARY KEY,
                Title TEXT NOT NULL,
                Content TEXT NOT NULL
            );";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS UserLogs (
                Id SERIAL PRIMARY KEY,
                Title TEXT NOT NULL
            );";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS StockLogs (
                Id SERIAL PRIMARY KEY,
                Title TEXT NOT NULL,
                Content TEXT NOT NULL
            );";
            cmd.ExecuteNonQuery();

            // ✅ Create Owner Table
            cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Owner (
                Id SERIAL PRIMARY KEY,
                Username TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                Role TEXT NOT NULL
            );";
            cmd.ExecuteNonQuery();

            // 🔹 **Check if an Owner Exists**
            cmd.CommandText = "SELECT COUNT(*) FROM Owner";
            int ownerCount = Convert.ToInt32(cmd.ExecuteScalar());

            // 🟢 **Insert Default Owner If No Owner Exists**
            if (ownerCount == 0)
            {
                string defaultUsername = "admin";
                string defaultPassword = HashPassword("admin123"); // 🔒 Hash the password
                string defaultRole = "SuperAdmin";

                cmd.CommandText = $@"
                INSERT INTO Owner (Username, Password, Role) 
                VALUES ('{defaultUsername}', '{defaultPassword}', '{defaultRole}');
                ";
                cmd.ExecuteNonQuery();
                Console.WriteLine("✅ Owner account was missing. Default owner added!");
            }
            else
            {
                Console.WriteLine("✅ Owner account exists. No changes made.");
            }

            // Session Table (for Authentication)
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS UserSessions (
                SessionId SERIAL PRIMARY KEY,
                UserId INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
                Token TEXT NOT NULL UNIQUE,
                ExpiryDate TIMESTAMP NOT NULL
            );";
            cmd.ExecuteNonQuery();

            // Session Table (for Owner Authentication)
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS OwnerSessions (
                SessionId SERIAL PRIMARY KEY,
                UserId INTEGER NOT NULL REFERENCES Owner(Id) ON DELETE CASCADE,
                Token TEXT NOT NULL UNIQUE,
                ExpiryDate TIMESTAMP NOT NULL
            );";
            cmd.ExecuteNonQuery();
        }

        Console.WriteLine("✅ Database tables initialized successfully!");
    }

    // 🔹 **Securely Hash Passwords**
    private static string HashPassword(string password)
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