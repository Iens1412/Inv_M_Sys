using Inv_M_Sys.Services;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;
using Inv_M_Sys.Models;
using Inv_M_Sys.Views.Forms;
using Inv_M_Sys.Views.Main;

namespace Inv_M_Sys.Views.Admin
{
    /// <summary>
    /// Interaction logic for DatabaseSettingsPage.xaml
    /// </summary>
    public partial class DatabaseSettingsPage : Page
    {
        private readonly HomeWindow _homeWindow;

        public DatabaseSettingsPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
        }

        #region Navigation Buttons

        // Handle Logout
        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        // Handle Close Button (Shut down application)
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        // Handle Minimize Button
        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        // Navigate to Dashboard Page
        private void Home_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));

        #endregion

        #region Database Settings and Owner Settings

        // Test Database Connection
        private void TestDatabaseConnection_Click(object sender, RoutedEventArgs e)
        {
            DatabaseHelper.TestConnection();
        }

        /// <summary>
        /// Updates the owner credentials (username and password).
        /// This operation requires validation to ensure that both fields are provided.
        /// </summary>
        /// <param name="newUsername">The new username for the owner.</param>
        /// <param name="newPassword">The new password for the owner.</param>
        private void UpdateOwnerCredentials_Click(object sender, RoutedEventArgs e)
        {
            // Check if the current user is an owner
            if (SessionManager.CurrentUserRole != "Owner")
            {
                MessageBox.Show("You do not have permission to update the owner credentials.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Stop further processing
            }

            string newOwnerUsername = OwnerUsernameTextBox.Text;
            string newOwnerPassword = OwnerPasswordBox.Text;

            // Validate that both username and password are provided
            if (string.IsNullOrEmpty(newOwnerUsername) || string.IsNullOrEmpty(newOwnerPassword))
            {
                MessageBox.Show("Please fill in both username and password.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Update owner credentials in the database
                DatabaseHelper.UpdateOwnerCredentials(newOwnerUsername, newOwnerPassword);
                MessageBox.Show("Owner credentials updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Log and show error
                Log.Error("Error updating owner credentials: {Error}", ex.Message);
                MessageBox.Show($"Error updating owner credentials: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates the owner credentials (username and password).
        /// This operation add the default username amd default Password to restore the credentials
        /// </summary>
        /// <param name="newUsername">The default username for the owner.</param>
        /// <param name="newPassword">The default password for the owner.</param>
        private void Reset_Owner_Credentials(object sender, RoutedEventArgs e)
        {
            // Check if the current user is an owner
            if (SessionManager.CurrentUserRole != "Owner")
            {
                MessageBox.Show("You do not have permission to reset the owner credentials.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Stop further processing
            }

            try
            {
                // Reset owner credentials to default values
                string defaultUsername = "admin";  // Example default username
                string defaultPassword = "admin123";  // Example default password

                // Call the helper method to reset the owner credentials
                DatabaseHelper.UpdateOwnerCredentials(defaultUsername, defaultPassword);

                // Clear the text boxes after reset
                OwnerUsernameTextBox.Text = defaultUsername;
                OwnerPasswordBox.Text = defaultPassword;

                MessageBox.Show("Owner credentials reset successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Log and show error
                Log.Error("Error resetting owner credentials: {Error}", ex.Message);
                MessageBox.Show($"Error resetting owner credentials: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Clear the text fields for the username and password
        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            OwnerUsernameTextBox.Text = "";
            OwnerPasswordBox.Text = "";
        }

        #endregion
    }
}