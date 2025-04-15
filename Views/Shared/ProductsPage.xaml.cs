using Inv_M_Sys.Services;
using Inv_M_Sys.ViewModels;
using Inv_M_Sys.Views.Forms;
using Inv_M_Sys.Views.Main;
using System.Windows;
using System.Windows.Controls;

namespace Inv_M_Sys.Views.Shared
{
    public partial class ProductsPage : Page
    {
        private readonly HomeWindow _homeWindow;

        public ProductsPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
        }

        #region Top Bar Controls

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.Logout();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));
        }

        #endregion
    }
}
