using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Inv_M_Sys.Services;
using Inv_M_Sys.Views.Main;

namespace Inv_M_Sys.Views.Forms
{
    /// <summary>
    /// Interaction logic for HomeWindow.xaml
    /// Manages page navigation and session expiration handling.
    /// </summary>
    public partial class HomeWindow : Window
    {
        private DispatcherTimer _sessionTimer;

        #region Constructor & Initialization

        /// <summary>
        /// Initializes the HomeWindow, starts session timer and navigates to the dashboard.
        /// </summary>
        public HomeWindow()
        {
            InitializeComponent();

            // Add event listener for key press
            this.PreviewKeyDown += new KeyEventHandler(HomeWindow_PreviewKeyDown);

            // Navigate to the DashboardPage when HomeWindow is loaded
            NavigateToPage(new Views.Main.DashboardPage(this));

            // Set up session timer that checks every minute
            _sessionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _sessionTimer.Tick += CheckSessionStatus;
            _sessionTimer.Start();
        }

        #endregion

        #region Key Handling

        /// <summary>
        /// Handles key down events to allow Backspace inside text fields, and prevent navigation.
        /// </summary>
        private void HomeWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Allow Backspace inside TextBox, PasswordBox, and ComboBox
            if (Keyboard.FocusedElement is TextBox || Keyboard.FocusedElement is PasswordBox || Keyboard.FocusedElement is ComboBox)
            {
                return; // Do nothing, allow backspace to work normally
            }

            // Prevent page navigation when pressing Backspace elsewhere
            if (e.Key == Key.Back)
            {
                e.Handled = true;
            }
        }

        #endregion

        #region Navigation Methods

        /// <summary>
        /// Navigates to the provided page within the HomeWindow.
        /// </summary>
        /// <param name="page">The page to navigate to.</param>
        public void NavigateToPage(Page page)
        {
            // Check if the page is already being displayed
            if (MainFrame.Content != page)
            {
                MainFrame.NavigationService.Navigate(page);
            }
        }

        #endregion

        #region Session Management

        /// <summary>
        /// Periodically checks the session validity and handles session expiration.
        /// </summary>
        private void CheckSessionStatus(object sender, EventArgs e)
        {
            if (!SessionManager.IsSessionValid())
            {
                // Notify user about session expiration
                MessageBox.Show("Your session has expired. Please log in again.", "Session Expired", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Log out the user and stop the session timer
                SessionManager.Logout();
                _sessionTimer.Stop();

                // Navigate to the login screen
                Window parentWindow = Window.GetWindow(this);

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                mainWindow.NavigateToPage(new Views.Main.LoginPage());

                // Close the current window
                parentWindow.Close();
            }
        }

        #endregion

        #region UI Controls

        /// <summary>
        /// Closes the application window.
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Minimizes the application window.
        /// </summary>
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        #endregion
    }
}