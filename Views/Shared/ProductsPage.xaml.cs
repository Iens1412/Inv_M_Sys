using DocumentFormat.OpenXml.Wordprocessing;
using Inv_M_Sys.Helpers;
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

            var vm = new ProductsPageViewModel();
            DataContext = vm;


            Messenger.Default.Register<string>(this, message =>
            {
                if (message == "ShowForm")
                    Info_Container.Visibility = Visibility.Visible;
                else if (message == "HideForm")
                    Info_Container.Visibility = Visibility.Collapsed;
            });
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Unregister<string>(this, null);
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
