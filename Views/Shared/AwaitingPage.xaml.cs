using Inv_M_Sys.Services;
using Inv_M_Sys.ViewModels;
using Inv_M_Sys.Views.Main;
using Inv_M_Sys.Helpers;
using System.Windows;
using System.Windows.Controls;
using Inv_M_Sys.Views.Forms;

namespace Inv_M_Sys.Views.Shared
{
    public partial class AwaitingPage : Page
    {
        private readonly HomeWindow _homeWindow;

        public AwaitingPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
            DataContext = new AwaitingPageViewModel();

            Messenger.Default.Register<string>(this, (string msg) =>
            {
                if (msg == "ShowOrderInfo")
                    OrderInfoContainer.Visibility = Visibility.Visible;
                else if (msg == "HideOrderInfo")
                    OrderInfoContainer.Visibility = Visibility.Collapsed;
            });
        }

        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        private void Home_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            OrderInfoContainer.Visibility = Visibility.Collapsed;
        }


    }
}
