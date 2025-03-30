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

            // Orders Table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Orders (
                Id SERIAL PRIMARY KEY,
                CustomerId INTEGER REFERENCES Customers(Id) ON DELETE CASCADE,
                DeliveryDate DATE NOT NULL,
                TotalPrice NUMERIC NOT NULL,
                Status TEXT NOT NULL DEFAULT 'Pending'
            );";
            cmd.ExecuteNonQuery();

            // Create OrderItems table to store product-level information for each order
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS OrderItems (
                Id SERIAL PRIMARY KEY,
                OrderId INTEGER REFERENCES Orders(Id) ON DELETE CASCADE,  -- Foreign key to Orders
                ProductId INTEGER REFERENCES Products(Id) ON DELETE CASCADE,  -- Foreign key to Products
                Quantity INTEGER NOT NULL,
                Price NUMERIC NOT NULL,  -- Price at the time of order
                TotalPrice NUMERIC NOT NULL -- Price * Quantity
            );";
            cmd.ExecuteNonQuery();

            // Products Table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Products (
                Id SERIAL PRIMARY KEY,
                ProductName TEXT NOT NULL,
                CategoryId INT NOT NULL,  -- Foreign Key
                Quantity INTEGER NOT NULL,
                Price NUMERIC NOT NULL,
                MinQuantity INTEGER NOT NULL,
                Supplier TEXT NOT NULL,
                Description TEXT,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
            );";
            cmd.ExecuteNonQuery();

            // table for the Schedule
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Schedule (
                Id SERIAL PRIMARY KEY,
                EmployeeName TEXT NOT NULL,
                StartTime TIME NOT NULL,
                EndTime TIME NOT NULL,
                TotalHours DOUBLE PRECISION NOT NULL
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

            // ✅ Fix: Make UserId UNIQUE in UserSessions
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS UserSessions (
            SessionId SERIAL PRIMARY KEY,
            UserId INTEGER NOT NULL UNIQUE REFERENCES Users(Id) ON DELETE CASCADE,
            Token TEXT NOT NULL UNIQUE,
            ExpiryDate TIMESTAMP NOT NULL
            );";
            cmd.ExecuteNonQuery();

            // ✅ Fix: Make UserId UNIQUE in OwnerSessions
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS OwnerSessions (
            SessionId SERIAL PRIMARY KEY,
            UserId INTEGER NOT NULL UNIQUE REFERENCES Owner(Id) ON DELETE CASCADE,
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