using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Inv_M_Sys.Services;

namespace Inv_M_Sys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Navigate to the LoginPage when the application starts
            NavigateToPage(new Views.Main.LoginPage());
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allows the window to be dragged
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Method to navigate to different pages inside HomeWindow
        public void NavigateToPage(Page page)
        {
            MainFrame.NavigationService.Navigate(page);
        }
    }
}