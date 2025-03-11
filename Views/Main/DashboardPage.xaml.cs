using Serilog;
using Inv_M_Sys.Services;
using Inv_M_Sys.Views.Forms;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Inv_M_Sys.Views.Main
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page, INotifyPropertyChanged
    {
        private string _userRole;
        private readonly HomeWindow _homeWindow;

        public string UserRole
        {
            get { return _userRole; }
            set
            {
                _userRole = value;
                OnPropertyChanged(nameof(UserRole));
                Log.Information("User role set to: {UserRole}", value); // Log role change
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Constructor and Initialization

        public DashboardPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            DataContext = this;
            _homeWindow = homeWindow ?? throw new ArgumentNullException(nameof(homeWindow));
            UserRole = SessionManager.CurrentUserRole; // Get role from session
            Log.Information("DashboardPage initialized with user role: {UserRole}", UserRole);
            ApplyRoleRestrictions();
        }

        #endregion

        #region Role Management

        private void ApplyRoleRestrictions()
        {
            // Hide all buttons by default
            SharedButtons.Visibility = Visibility.Visible; // Always visible
            SellingButtons.Visibility = Visibility.Collapsed;
            StorageButtons.Visibility = Visibility.Collapsed;
            MangeButtons.Visibility = Visibility.Collapsed;
            OwnerButton.Visibility = Visibility.Collapsed;

            // Show buttons based on role
            switch (UserRole)
            {
                case "SellingStaff":
                    SellingButtons.Visibility = Visibility.Visible;
                    break;

                case "StockStaff":
                    StorageButtons.Visibility = Visibility.Visible;
                    break;

                case "Admin":
                    SellingButtons.Visibility = Visibility.Visible;
                    StorageButtons.Visibility = Visibility.Visible;
                    MangeButtons.Visibility = Visibility.Visible;
                    break;

                case "Owner":
                    SellingButtons.Visibility = Visibility.Visible;
                    StorageButtons.Visibility = Visibility.Visible;
                    MangeButtons.Visibility = Visibility.Visible;
                    OwnerButton.Visibility = Visibility.Visible;
                    break;
                default:
                    Log.Warning("Unknown user role: {UserRole}. No buttons displayed.", UserRole);
                    break;
            }

            Log.Information("Role-based button visibility applied for role: {UserRole}", UserRole);
        }

        #endregion

        #region Button Click Handlers

        // Navigation button handlers
        private void Orders_Click(object sender, RoutedEventArgs e) => NavigateToPage(new Views.Shared.OrdersPage(_homeWindow));

        private void Users_Click(object sender, RoutedEventArgs e) => NavigateToPage(new Views.Admin.UsersPage(_homeWindow));

        private void Note_Click(object sender, RoutedEventArgs e) => NavigateToPage(new Views.Shared.NotificationsPage(_homeWindow));

        private void Customers_Click(object sender, RoutedEventArgs e) => NavigateToPage(new Views.Shared.CustomersPage(_homeWindow));

        private void Restock_Click(object sender, RoutedEventArgs e) => NavigateToPage(new Views.Shared.RestockPage(_homeWindow));

        private void Reports_Click(object sender, RoutedEventArgs e) => NavigateToPage(new Views.Shared.ReportsPage(_homeWindow));

        private void Products_Click(object sender, RoutedEventArgs e) => NavigateToPage(new Views.Shared.ProductsPage(_homeWindow));

        private void Category_Click(object sender, RoutedEventArgs e) => NavigateToPage(new Views.Shared.CategoriesPage(_homeWindow));

        private void Logs_Click(object sender, RoutedEventArgs e) => NavigateToPage(new Views.Admin.LogsPage(_homeWindow));

        private void Owner_Click(object sender, RoutedEventArgs e) => NavigateToPage(new Views.Admin.DatabaseSettingsPage(_homeWindow));

        private void Schedule_Click(object sender, RoutedEventArgs e)
        {
            // Add any necessary code here when the schedule button is clicked
            // For now, it could just log or show a placeholder message
            Log.Information("Schedule button clicked.");
            MessageBox.Show("Schedule feature not implemented yet.");
        }

        // Utility button handlers
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
            Log.Information("Application shutdown initiated by user.");
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.Logout();
            Log.Information("User logged out successfully.");
            // Redirect to the login page or perform other necessary actions after logout
        }
        #endregion

        #region Navigation Helper

        private void NavigateToPage(Page page)
        {
            Log.Information("Navigating to page: {Page}", page.GetType().Name);
            _homeWindow.NavigateToPage(page);
        }

        #endregion
    }
}
