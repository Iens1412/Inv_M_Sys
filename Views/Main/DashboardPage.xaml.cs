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
            UserRole = SessionManager.CurrentUserRole; // Get role from session
            _homeWindow = homeWindow ?? throw new ArgumentNullException(nameof(homeWindow));

        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clicked");
            _homeWindow.NavigateToPage(new Views.Shared.OrdersPage(_homeWindow));
        }
    }
}