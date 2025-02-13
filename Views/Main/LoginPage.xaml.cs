using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Inv_M_Sys.Services;
using Inv_M_Sys.Views.Forms;
using Npgsql;

namespace Inv_M_Sys.Views.Main
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        // Username Placeholder Logic
        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text == "Username" || string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                UsernamePlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                UsernamePlaceholder.Visibility = Visibility.Visible;
            }
        }

        // Password Placeholder Logic
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = PasswordBox.Password.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        // Show/Hide Password Logic
        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordTextBox.Visibility = Visibility.Visible;
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordTextBox.Visibility = Visibility.Collapsed;
        }

        // Clear Input Fields
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            UsernameTextBox.Text = "";
            PasswordBox.Password = "";
            UsernamePlaceholder.Visibility = Visibility.Visible;
            PasswordPlaceholder.Visibility = Visibility.Visible;

            if (PasswordTextBox.Visibility == Visibility.Visible)
            {
                PasswordTextBox.Text = "";
            }
        }

        // Exit Application
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Login Button Click
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check Owner first
            if (AuthenticateOwner(username, password))
            {
                Services.SessionManager.CreateOwnerSession(username);
                OpenHomeWindow();
            }
            else if (AuthenticateUser(username, password))
            {
                Services.SessionManager.CreateUserSession(username);
                OpenHomeWindow();
            }
            else
            {
                MessageBox.Show("Invalid credentials. Please try again.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenHomeWindow()
        {
            Window parentWindow = Window.GetWindow(this);

            HomeWindow homeWindow = new HomeWindow();

            // Check if logged in as Owner
            if (SessionManager.CurrentOwnerId.HasValue)
            {
                homeWindow.NavigateToPage(new Views.Main.DashboardPage(homeWindow));
            }
            // Check if logged in as Admin
            else if (SessionManager.CurrentUserRole == "Admin")
            {
                homeWindow.NavigateToPage(new Views.Main.DashboardPage(homeWindow));
            }
            // Default User Role (Regular User)
            else
            {
                homeWindow.NavigateToPage(new Views.Main.DashboardPage(homeWindow));
            }

            homeWindow.Show();
            parentWindow?.Close();
        }

        // Authentication for Owner
        private bool AuthenticateOwner(string username, string password)
        {
            string query = "SELECT Password FROM Owner WHERE Username = @username";
            return Authenticate(query, username, password);
        }

        // Authentication for User
        private bool AuthenticateUser(string username, string password)
        {
            string query = "SELECT Password FROM Users WHERE Username = @username";
            return Authenticate(query, username, password);
        }

        // Common Authentication Logic
        private bool Authenticate(string query, string username, string password)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        var storedPassword = cmd.ExecuteScalar()?.ToString();

                        if (storedPassword != null && VerifyPassword(password, storedPassword))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        // Hash Password for Verification
        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            return HashPassword(inputPassword) == storedPassword;
        }

        // Secure Password Hashing
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}