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
using System.Windows.Shapes;
using System;
using System.Windows.Threading;
using Inv_M_Sys.Services;

namespace Inv_M_Sys.Views.Forms
{
    /// <summary>
    /// Interaction logic for HomeWindow.xaml
    /// </summary>
    public partial class HomeWindow : Window
    {
        private DispatcherTimer _sessionTimer;

        public HomeWindow()
        {
            InitializeComponent();

            _sessionTimer = new DispatcherTimer();
            _sessionTimer.Interval = TimeSpan.FromMinutes(1);
            _sessionTimer.Tick += CheckSessionStatus;
            _sessionTimer.Start();

        }

        // Method to navigate to different pages inside HomeWindow
        public void NavigateToPage(Page page)
        {
            MainFrame.NavigationService.Navigate(page);
        }

        private void CheckSessionStatus(object sender, EventArgs e)
        {
            if (!SessionManager.IsSessionValid())
            {
                MessageBox.Show("Your session has expired. Please log in again.", "Session Expired", MessageBoxButton.OK, MessageBoxImage.Warning);
                SessionManager.Logout();
                _sessionTimer.Stop();
                Window parentWindow = Window.GetWindow(this);

                MainWindow MainWindow = new MainWindow();
                MainWindow.Show();
                MainWindow.NavigateToPage(new Views.Main.LoginPage());
                parentWindow.Close();
            }
        }
    }
}
