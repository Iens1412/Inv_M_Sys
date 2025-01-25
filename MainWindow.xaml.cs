using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }

        private void NavigateToProducts(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Views.ProductsPage());
        }

        private void NavigateToCategories(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Views.CategoriesPage());
        }

        private void NavigateToReports(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Views.ReportsPage());
        }

        private void NavigateToUsers(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Views.UsersPage());
        }
    }
}