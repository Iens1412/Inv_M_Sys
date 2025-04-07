using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.IO;
using Inv_M_Sys.Services;
using Inv_M_Sys.ViewModels;
using Serilog;


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

    public static async Task<int> GetStaffIdByNameAsync(string fullName)
    {
        int id = 0;
        try
        {
            using (var conn = GetConnection())
            {
                await conn.OpenAsync();
                string query = "SELECT Id FROM Users WHERE LOWER(TRIM(FirstName) || ' ' || TRIM(LastName)) = LOWER(@FullName)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", fullName.Trim());
                    object result = await cmd.ExecuteScalarAsync();
                    if (result != null)
                        id = Convert.ToInt32(result);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting staff ID by name.");
        }
        return id;
    }

    public static async Task<WeeklyScheduleViewModel> LoadWeeklyScheduleForStaffAsync(int staffId)
    {
        var weeklySchedule = new WeeklyScheduleViewModel();
        weeklySchedule.DailySchedules.Clear();
        try
        {
            using (var conn = GetConnection())
            {
                await conn.OpenAsync();
                // Adjust column indices: DayName(0), StartTime(1), EndTime(2), RestStartTime(3), RestEndTime(4), TotalHours(5), IsRestDay(6)
                string query = "SELECT DayName, StartTime, EndTime, RestStartTime, RestEndTime, TotalHours, IsRestDay FROM Schedule WHERE EmployeeId = @EmployeeId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeId", staffId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var day = new DailyScheduleViewModel
                            {
                                DayName = reader.GetString(0),
                                WorkStartHour = reader.GetTimeSpan(1).Hours,
                                WorkStartMinute = reader.GetTimeSpan(1).Minutes,
                                WorkEndHour = reader.GetTimeSpan(2).Hours,
                                WorkEndMinute = reader.GetTimeSpan(2).Minutes,
                                // Load rest times if they exist
                                HasRestTime = !reader.IsDBNull(3) && !reader.IsDBNull(4),
                                IsRestDay = reader.GetBoolean(6)
                            };
                            if (day.HasRestTime)
                            {
                                var restStart = reader.GetTimeSpan(3);
                                var restEnd = reader.GetTimeSpan(4);
                                day.RestStartHour = restStart.Hours;
                                day.RestStartMinute = restStart.Minutes;
                                day.RestEndHour = restEnd.Hours;
                                day.RestEndMinute = restEnd.Minutes;
                            }
                            weeklySchedule.DailySchedules.Add(day);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading weekly schedule for staff.");
        }
        return weeklySchedule;
    }

    public static async Task SaveDailyScheduleAsync(DailyScheduleViewModel day, int staffId)
    {
        try
        {
            using (var conn = GetConnection())
            {
                await conn.OpenAsync();
                string checkQuery = "SELECT COUNT(*) FROM Schedule WHERE EmployeeId = @EmployeeId AND DayName = @DayName";
                using (var checkCmd = new NpgsqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@EmployeeId", staffId);
                    checkCmd.Parameters.AddWithValue("@DayName", day.DayName);
                    int count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                    if (count > 0)
                    {
                        string updateQuery = @"UPDATE Schedule SET 
                                           StartTime = @StartTime, 
                                           EndTime = @EndTime, 
                                           RestStartTime = @RestStartTime, 
                                           RestEndTime = @RestEndTime, 
                                           TotalHours = @TotalHours,
                                           IsRestDay = @IsRestDay
                                           WHERE EmployeeId = @EmployeeId AND DayName = @DayName";
                        using (var updateCmd = new NpgsqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@StartTime", new TimeSpan(day.WorkStartHour, day.WorkStartMinute, 0));
                            updateCmd.Parameters.AddWithValue("@EndTime", new TimeSpan(day.WorkEndHour, day.WorkEndMinute, 0));
                            if (day.HasRestTime)
                            {
                                updateCmd.Parameters.AddWithValue("@RestStartTime", new TimeSpan(day.RestStartHour, day.RestStartMinute, 0));
                                updateCmd.Parameters.AddWithValue("@RestEndTime", new TimeSpan(day.RestEndHour, day.RestEndMinute, 0));
                            }
                            else
                            {
                                updateCmd.Parameters.AddWithValue("@RestStartTime", DBNull.Value);
                                updateCmd.Parameters.AddWithValue("@RestEndTime", DBNull.Value);
                            }
                            updateCmd.Parameters.AddWithValue("@TotalHours", day.TotalWorkHours);
                            updateCmd.Parameters.AddWithValue("@IsRestDay", day.IsRestDay);
                            updateCmd.Parameters.AddWithValue("@EmployeeId", staffId);
                            updateCmd.Parameters.AddWithValue("@DayName", day.DayName);
                            await updateCmd.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        string insertQuery = @"INSERT INTO Schedule (EmployeeId, DayName, StartTime, EndTime, RestStartTime, RestEndTime, TotalHours, IsRestDay)
                                           VALUES (@EmployeeId, @DayName, @StartTime, @EndTime, @RestStartTime, @RestEndTime, @TotalHours, @IsRestDay)";
                        using (var insertCmd = new NpgsqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@EmployeeId", staffId);
                            insertCmd.Parameters.AddWithValue("@DayName", day.DayName);
                            insertCmd.Parameters.AddWithValue("@StartTime", new TimeSpan(day.WorkStartHour, day.WorkStartMinute, 0));
                            insertCmd.Parameters.AddWithValue("@EndTime", new TimeSpan(day.WorkEndHour, day.WorkEndMinute, 0));
                            if (day.HasRestTime)
                            {
                                insertCmd.Parameters.AddWithValue("@RestStartTime", new TimeSpan(day.RestStartHour, day.RestStartMinute, 0));
                                insertCmd.Parameters.AddWithValue("@RestEndTime", new TimeSpan(day.RestEndHour, day.RestEndMinute, 0));
                            }
                            else
                            {
                                insertCmd.Parameters.AddWithValue("@RestStartTime", DBNull.Value);
                                insertCmd.Parameters.AddWithValue("@RestEndTime", DBNull.Value);
                            }
                            insertCmd.Parameters.AddWithValue("@TotalHours", day.TotalWorkHours);
                            insertCmd.Parameters.AddWithValue("@IsRestDay", day.IsRestDay);
                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving daily schedule.");
            throw;
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