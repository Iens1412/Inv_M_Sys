using Inv_M_Sys.Models;
using Inv_M_Sys.Services;
using Inv_M_Sys.Services.Pages_Services;
using Inv_M_Sys.ViewModels;
using Inv_M_Sys.Views.Forms;
using Inv_M_Sys.Views.Main;
using Npgsql;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Inv_M_Sys.Views.Shared
{
    /// <summary>
    /// Interaction logic for OrdersPage.xaml
    /// </summary>
    public partial class OrdersPage : Page
    {
        private readonly HomeWindow _homeWindow;
        private Customer SelectedCustomer;
        private ObservableCollection<OrderItem> OrderBasket = new ObservableCollection<OrderItem>();
        private ObservableCollection<Product> ProductsList = new ObservableCollection<Product>();

        public OrdersPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            DataContext = new OrdersViewModel();

            _homeWindow = homeWindow;
            _ = LoadCustomersAsync();
            _ = LoadCategoriesIntoComboBoxAsync();
        }

        /// <summary>
        /// Handles top menu actions like logout, close, minimize, and navigation.
        /// </summary>
        #region Top Menu
        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        private void Home_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));
        #endregion

        #region Customer
        /// <summary>
        /// Loads customers from the database into the ListView.
        /// </summary>
        private async Task LoadCustomersAsync()
        {
            try
            {
                var customers = await CustomerService.GetAllAsync();
                CustomersListView.ItemsSource = new ObservableCollection<Customer>(customers);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading customers.");
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Handle selection change in the CustomersListView
        private void CustomersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedCustomer = (Customer)CustomersListView.SelectedItem;
            if (SelectedCustomer != null)
            {
                // Show the customer's name, or handle selection as needed
                MessageBox.Show($"Selected Customer: {SelectedCustomer.FullName}");
            }
        }
        #endregion

        #region Products
        /// <summary>
        /// Loads products from the database filtered by selected category.
        /// </summary>
        private async Task LoadProductsAsync(int categoryId)
        {
            try
            {
                var products = await ProductService.GetByCategoryIdAsync(categoryId);
                ProductsList = new ObservableCollection<Product>(products);
                ProductsListView.ItemsSource = ProductsList;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading products.");
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Handle selection of product
        private void ProductsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Set the selected product to the SelectedProduct in the ViewModel
            if (ProductsListView.SelectedItem is Product selectedProduct)
            {
                // Update the SelectedProduct in the ViewModel
                var viewModel = DataContext as OrdersViewModel;
                viewModel.SelectedProduct = selectedProduct;
            }
        }
        #endregion

        #region Order
        /// <summary>
        /// Validates input and places the order by inserting data into Orders and OrderItems tables.
        /// </summary>
        private async void PlaceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as OrdersViewModel;

            if (viewModel?.OrderBasket.Count == 0)
            {
                MessageBox.Show("Your basket is empty.");
                return;
            }

            if (viewModel.SelectedCustomer == null)
            {
                MessageBox.Show("Please select a customer.");
                return;
            }

            if (!viewModel.DeliveryDate.HasValue)
            {
                MessageBox.Show("Please select a delivery date.");
                return;
            }

            var order = new Order
            {
                Id = viewModel.SelectedCustomer.Id,
                DeliveryDate = viewModel.DeliveryDate.Value,
                TotalPrice = viewModel.OrderBasket.Sum(x => x.TotalPrice),
                Status = OrderStatus.Pending
            };

            try
            {
                var orderId = await OrderService.PlaceOrderAsync(order, viewModel.OrderBasket.ToList());
                MessageBox.Show($"Order #{orderId} placed successfully.");
                viewModel.ClearBasketCommand.Execute(null); // Clear
                DeliveryDatePicker.SelectedDate = null;

                if (CategoryComboBox.SelectedItem is Category selectedCategory)
                {
                    await LoadProductsAsync(selectedCategory.CatID);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error placing order.");
                MessageBox.Show($"Error placing order: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears the order form UI fields and basket after placing or canceling an order.
        /// </summary>
        private void ClearOrderForm()
        {
            OrderBasket.Clear();
            OrderBasketListView.ItemsSource = null;
            DeliveryDatePicker.SelectedDate = null;
            SelectedCustomer = null;
            QuantityTextBox.Clear();
        }

        /// <summary>
        /// Cancels the order by clearing the form and basket.
        /// </summary>
        private void CancelOrderButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear all form data and reset the basket
            ClearOrderForm();
        }
        #endregion

        #region CategoryBox
        /// <summary>
        /// Loads all categories into the category ComboBox and auto-loads products for the first category.
        /// </summary>
        private async Task LoadCategoriesIntoComboBoxAsync()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "SELECT Id, Name FROM Categories";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var categories = new List<Category>();
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new Category
                            {
                                CatID = reader.GetInt32(0),
                                CategoryName = reader.GetString(1)
                            });
                        }

                        CategoryComboBox.ItemsSource = categories;
                        CategoryComboBox.DisplayMemberPath = "CategoryName";
                        CategoryComboBox.SelectedValuePath = "CatID";

                        // ✅ Set default selection
                        if (categories.Any())
                        {
                            CategoryComboBox.SelectedIndex = 0;

                            // Optional: Immediately load products for default category
                            await LoadProductsAsync(categories[0].CatID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading categories into the combo box.");
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates the product list when a new category is selected from the ComboBox.
        /// </summary>
        private async void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is Category selectedCategory)
            {
                // Load products based on the selected category ID
                await LoadProductsAsync(selectedCategory.CatID);
            }
        }

        #endregion

    }
}
