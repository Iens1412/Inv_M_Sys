using Inv_M_Sys.Services;
using Inv_M_Sys.Views.Forms;
using Inv_M_Sys.Views.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Windows;
using Npgsql;
using Inv_M_Sys.Services;

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


        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        private void Note_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.NotificationsPage(_homeWindow));

        private void Home_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));

        // Handle Save Settings Button for Database
        private void SaveDatabaseSettings_Click(object sender, RoutedEventArgs e)
        {
            string newDbUsername = DbUsernameTextBox.Text;
            string newDbPassword = DbPasswordBox.Text;

            if (string.IsNullOrEmpty(newDbUsername) || string.IsNullOrEmpty(newDbPassword))
            {
                MessageBox.Show("Please fill in both username and password.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Update the database settings in the config or directly in DB if needed
                DatabaseHelper.UpdateDatabaseSettings(newDbUsername, newDbPassword);
                MessageBox.Show("Database settings updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating database settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Handle Test Connection Button for Database
        private void TestDatabaseConnection_Click(object sender, RoutedEventArgs e)
        {
            DatabaseHelper.TestConnection();
        }

        // Handle Update Owner Credentials Button
        private void UpdateOwnerCredentials_Click(object sender, RoutedEventArgs e)
        {
            string newOwnerUsername = OwnerUsernameTextBox.Text;
            string newOwnerPassword = OwnerPasswordBox.Text;

            if (string.IsNullOrEmpty(newOwnerUsername) || string.IsNullOrEmpty(newOwnerPassword))
            {
                MessageBox.Show("Please fill in both username and password.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                DatabaseHelper.UpdateOwnerCredentials(newOwnerUsername, newOwnerPassword);
                MessageBox.Show("Owner credentials updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating owner credentials: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
 
    }
}
