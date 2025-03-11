using Inv_M_Sys.Services;
using Inv_M_Sys.Views.Forms;
using Inv_M_Sys.Views.Main;
using System;
using System.Windows;
using System.Windows.Controls;
using Serilog;

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

        // Navigate to Notifications Page
        private void Note_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.NotificationsPage(_homeWindow));

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
        /// After updating the credentials, an audit record is created.
        /// </summary>
        /// <param name="newUsername">The new username for the owner.</param>
        /// <param name="newPassword">The new password for the owner.</param>
        private void UpdateOwnerCredentials_Click(object sender, RoutedEventArgs e)
        {
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
                // Call the helper method to update the owner's credentials in the database
                DatabaseHelper.UpdateOwnerCredentials(newOwnerUsername, newOwnerPassword);
                MessageBox.Show("Owner credentials updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating owner credentials: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearBtn_Click (object sender, RoutedEventArgs e)
        {
            OwnerUsernameTextBox.Text = "";
            OwnerPasswordBox.Text = "";
        }


        #endregion
    }
}
