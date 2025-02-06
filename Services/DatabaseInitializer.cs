using System;
using Npgsql;

public static class DatabaseInitializer
{
    public static void InitializeDatabase()
    {
        string connectionString = "Host=host.docker.internal;Port=5433;Username=admin;Password=admin123;Database=inventory";

        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
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

                // Session Table (for Authentication)
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS UserSessions (
                    SessionId SERIAL PRIMARY KEY,
                    UserId INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
                    Token TEXT NOT NULL UNIQUE,
                    ExpiryDate TIMESTAMP NOT NULL
                );";
                cmd.ExecuteNonQuery();
            }
        }

        Console.WriteLine("✅ Database tables initialized successfully!");
    }
}
