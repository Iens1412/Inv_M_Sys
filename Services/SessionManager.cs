using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Npgsql;

namespace Inv_M_Sys.Services
{
    public static class SessionManager
    {
        public static int? CurrentUserId { get; private set; }
        public static int? CurrentOwnerId { get; private set; }
        public static string CurrentUserRole { get; private set; }
        public static string CurrentSessionToken { get; private set; }
        public static DateTime? SessionExpiry { get; private set; } // Stores session expiry time

        private const int SESSION_DURATION_MINUTES = 1; // 5 minutes for testing (change to 720 for 12 hours)

        public static void CreateUserSession(string username)
        {
            string query = "SELECT Id, Role FROM Users WHERE Username = @username";
            CreateSession(query, username, isOwner: false);
        }

        public static void CreateOwnerSession(string username)
        {
            string query = "SELECT Id, Role FROM Owner WHERE Username = @username";
            CreateSession(query, username, isOwner: true);
        }

        private static void CreateSession(string query, string username, bool isOwner)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string token = GenerateSessionToken(); // Generate a unique token
                                DateTime expiryDate = DateTime.UtcNow.AddMinutes(SESSION_DURATION_MINUTES); // Set expiry time
                                int userId = reader.GetInt32(0); // Store user ID

                                // 🛠 Close the reader before executing a new command
                                reader.Close();

                                if (isOwner)
                                {
                                    CurrentOwnerId = userId;
                                    CurrentUserRole = "Owner"; // Since Owner table has no Role column
                                    CurrentSessionToken = token;
                                    SessionExpiry = expiryDate;

                                    MessageBox.Show($"Logged in as Owner: {username}\nSession Expires: {expiryDate}", "Session Info", MessageBoxButton.OK, MessageBoxImage.Information);
                                    SaveSessionToDB(conn, "OwnerSessions", CurrentOwnerId.Value, token, expiryDate);
                                }
                                else
                                {
                                    CurrentUserId = userId;
                                    CurrentUserRole = reader.GetString(1); // Fetch User Role
                                    CurrentSessionToken = token;
                                    SessionExpiry = expiryDate;

                                    MessageBox.Show($"Logged in as: {username}\nRole: {CurrentUserRole}\nSession Expires: {expiryDate}", "Session Info", MessageBoxButton.OK, MessageBoxImage.Information);
                                    SaveSessionToDB(conn, "UserSessions", CurrentUserId.Value, token, expiryDate);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating session: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GenerateSessionToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenBytes = new byte[32];
                rng.GetBytes(tokenBytes);
                return Convert.ToBase64String(tokenBytes);
            }
        }

        private static void SaveSessionToDB(NpgsqlConnection conn, string tableName, int userId, string token, DateTime expiry)
        {
            string query = $@"
                INSERT INTO {tableName} (UserId, Token, ExpiryDate) 
                VALUES (@userId, @token, @expiryDate)
                ON CONFLICT (UserId) 
                DO UPDATE SET Token = @token, ExpiryDate = @expiryDate;";

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@expiryDate", expiry);
                cmd.ExecuteNonQuery();
            }
        }

        public static bool IsSessionValid()
        {
            return SessionExpiry.HasValue && DateTime.UtcNow < SessionExpiry.Value;
        }

        public static void Logout()
        {
            CurrentUserId = null;
            CurrentOwnerId = null;
            CurrentSessionToken = null;
            SessionExpiry = null;
            MessageBox.Show("Logged out successfully.", "Logout", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}