using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Npgsql;
using System;
using System.Timers;
using Inv_M_Sys.Views.Forms;
using Inv_M_Sys.Views.Main;

namespace Inv_M_Sys.Services
{
    public static class SessionManager
    {
        public static int? CurrentUserId { get; private set; }
        public static int? CurrentOwnerId { get; private set; }
        public static string CurrentUserRole { get; private set; }
        public static string CurrentSessionToken { get; private set; }
        public static DateTime? SessionExpiry { get; private set; }
        private static System.Timers.Timer sessionTimer;

        private const int SESSION_DURATION_MINUTES = 360;

        static SessionManager()
        {
            StartSessionTimer();
        }

        private static void StartSessionTimer()
        {
            sessionTimer = new System.Timers.Timer(30000);
            sessionTimer.Elapsed += (sender, e) => CheckSessionValidity();
            sessionTimer.AutoReset = true;
            sessionTimer.Start();
        }

        public static bool IsSessionValid()
        {
            return SessionExpiry.HasValue && DateTime.Now < SessionExpiry.Value;
        }

        private static void CheckSessionValidity()
        {
            if (!IsSessionValid())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Logout();
                });
            }
        }

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
                                string token = GenerateSessionToken();
                                DateTime expiryDate = DateTime.Now.AddMinutes(SESSION_DURATION_MINUTES);
                                int userId = reader.GetInt32(0);
                                string role = isOwner ? "Owner" : reader.GetString(1); // Fetch role for users

                                reader.Close(); // ✅ Close before executing next command

                                if (isOwner)
                                {
                                    CurrentOwnerId = userId;
                                    CurrentUserRole = "Owner";
                                    CurrentSessionToken = token;
                                    SessionExpiry = expiryDate;
                                    SaveSessionToDB(conn, "OwnerSessions", CurrentOwnerId.Value, token, expiryDate);
                                }
                                else
                                {
                                    CurrentUserId = userId;
                                    CurrentUserRole = role;
                                    CurrentSessionToken = token;
                                    SessionExpiry = expiryDate;
                                    SaveSessionToDB(conn, "UserSessions", CurrentUserId.Value, token, expiryDate);
                                }

                                // ✅ Show MessageBox AFTER session is stored
                                MessageBox.Show($"Logged in as: {username}\nRole: {CurrentUserRole}\nSession Expires: {expiryDate}",
                                    "Session Info", MessageBoxButton.OK, MessageBoxImage.Information);
                            }

                            if (sessionTimer == null)
                            {
                                StartSessionTimer(); // ✅ Restart session timer for a new session
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

        public static void Logout()
        {
            try
            {
                // ✅ Update session expiry in the database to expire immediately
                ExpireSessionInDB();

                // ✅ Clear session variables
                CurrentUserId = null;
                CurrentOwnerId = null;
                CurrentUserRole = null;
                CurrentSessionToken = null;
                SessionExpiry = null;

                if (sessionTimer != null)
                {
                    sessionTimer.Stop();
                    sessionTimer.Dispose();
                    sessionTimer = null; // Reset to null for a fresh start
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Logged out successfully.", "Logout", MessageBoxButton.OK, MessageBoxImage.Information);

                    // ✅ Close HomeWindow if it's open
                    Window homeWindow = Application.Current.Windows.OfType<HomeWindow>().FirstOrDefault();

                    // ✅ Find or create MainWindow
                    MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

                    if (mainWindow != null)
                    {
                        // ✅ If minimized, restore it
                        if (mainWindow.WindowState == WindowState.Minimized)
                        {
                            mainWindow.WindowState = WindowState.Normal;
                        }
                        mainWindow.Activate(); // Bring to front
                    }
                    else
                    {
                        // ✅ Create new MainWindow if not found
                        mainWindow = new MainWindow();
                        mainWindow.Show();
                    }

                    // ✅ Ensure LoginPage is displayed after logout
                    mainWindow.NavigateToPage(new LoginPage());

                    if (homeWindow != null)
                    {
                        homeWindow?.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Logout Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void ExpireSessionInDB()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "";

                    if (CurrentUserId.HasValue)
                    {
                        query = "UPDATE UserSessions SET ExpiryDate = '1970-01-01 00:00:00' WHERE UserId = @UserId";
                    }
                    else if (CurrentOwnerId.HasValue)
                    {
                        query = "UPDATE OwnerSessions SET ExpiryDate = '1970-01-01 00:00:00' WHERE UserId = @UserId";
                    }

                    if (!string.IsNullOrEmpty(query))
                    {
                        using (var cmd = new NpgsqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", CurrentUserId ?? CurrentOwnerId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error expiring session in DB: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}