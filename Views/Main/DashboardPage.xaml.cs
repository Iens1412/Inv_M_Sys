using Inv_M_Sys.Services;
using Inv_M_Sys.Views.Forms;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Inv_M_Sys.Views.Main
{
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
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DashboardPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            DataContext = this;
            _homeWindow = homeWindow ?? throw new ArgumentNullException(nameof(homeWindow));
            UserRole = SessionManager.CurrentUserRole; // Get role from session

            ApplyRoleRestrictions();
        }

        private void ApplyRoleRestrictions()
        {
            // Hide all groups by default
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
            }
        }

        private void Orders_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.OrdersPage(_homeWindow));

        private void Users_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Admin.UsersPage(_homeWindow));

        private void Note_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.NotificationsPage(_homeWindow));

        private void LogOut_CLick(object sender, RoutedEventArgs e) => SessionManager.Logout();

        private void Customers_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.CustomersPage(_homeWindow));

        private void Restock_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.RestockPage(_homeWindow));

        private void Reports_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.ReportsPage(_homeWindow));

        private void Schedule_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Products_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.ProductsPage(_homeWindow));

        private void Category_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.CategoriesPage(_homeWindow));

        private void Logs_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Admin.LogsPage(_homeWindow));

        private void Owner_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Admin.DatabaseSettingsPage(_homeWindow));

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;
    }
}