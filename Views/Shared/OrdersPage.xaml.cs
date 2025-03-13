using Inv_M_Sys.Models;
using Inv_M_Sys.Services;
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

        #region Top Menu
        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        private void Note_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.NotificationsPage(_homeWindow));

        private void Home_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));
        #endregion

        #region Customer
        private async Task LoadCustomersAsync()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "SELECT Id, FirstName, LastName, CompanyName FROM Customers"; // Adjust query if necessary
                    using (var cmd = new NpgsqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var customers = new List<Customer>();
                        while (await reader.ReadAsync())
                        {
                            customers.Add(new Customer
                            {
                                Id = reader.GetInt32(0), // Now using 'Id' instead of 'CustomerID'
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                CompanyName = reader.GetString(3)
                            });
                        }

                        CustomersListView.ItemsSource = new ObservableCollection<Customer>(customers);
                    }
                }
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
        private async Task LoadProductsAsync(int categoryId)
        {
            ProductsList.Clear(); // Clear any previously loaded products

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "SELECT Id, ProductName, Price FROM Products WHERE CategoryId = @CategoryId"; // Query to filter products by category
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryId", categoryId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            var products = new List<Product>();
                            while (await reader.ReadAsync())
                            {
                                products.Add(new Product
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Price = reader.GetDecimal(2)
                                });
                            }

                            // Bind filtered products to the ListView
                            ProductsList = new ObservableCollection<Product>(products);
                            ProductsListView.ItemsSource = ProductsList;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading products based on category.");
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
        private async void PlaceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCustomer == null)
            {
                MessageBox.Show("Please select a customer.");
                return;
            }

            // Use the OrderBasket from the ViewModel (assuming you have DataContext set to OrdersViewModel)
            if (OrderBasket.Count == 0)
            {
                MessageBox.Show("Your basket is empty.");
                return;
            }

            var deliveryDate = DeliveryDatePicker.SelectedDate;

            if (!deliveryDate.HasValue)
            {
                MessageBox.Show("Please select a delivery date.");
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Create Order record
                            var orderCmd = new NpgsqlCommand("INSERT INTO Orders (CustomerID, DeliveryDate) VALUES (@CustomerID, @DeliveryDate) RETURNING OrderID", conn);
                            orderCmd.Parameters.AddWithValue("@CustomerID", SelectedCustomer.Id);
                            orderCmd.Parameters.AddWithValue("@DeliveryDate", deliveryDate.Value);
                            var orderId = (int)await orderCmd.ExecuteScalarAsync();

                            // Add OrderItems
                            foreach (var item in OrderBasket) // Loop through the OrderBasket from ViewModel
                            {
                                var orderItemCmd = new NpgsqlCommand("INSERT INTO OrderItems (OrderID, ProductID, Quantity, TotalPrice) VALUES (@OrderID, @ProductID, @Quantity, @TotalPrice)", conn);
                                orderItemCmd.Parameters.AddWithValue("@OrderID", orderId);
                                orderItemCmd.Parameters.AddWithValue("@ProductID", item.Product.Id);
                                orderItemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                orderItemCmd.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                                await orderItemCmd.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();
                            MessageBox.Show("Order placed successfully.");
                            ClearOrderForm();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Log.Error(ex, "Error placing order.");
                            MessageBox.Show($"Error placing order: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error placing order: {ex.Message}");
            }
        }

        private void ClearOrderForm()
        {
            OrderBasket.Clear();
            OrderBasketListView.ItemsSource = null;
            DeliveryDatePicker.SelectedDate = null;
            SelectedCustomer = null;
            QuantityTextBox.Clear();
        }

        private void CancelOrderButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear all form data and reset the basket
            ClearOrderForm();
        }
        #endregion

        #region CategoryBox
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
                                CatID = reader.GetInt32(0), // Correct field name
                                CategoryName = reader.GetString(1)
                            });
                        }

                        // Ensure this is populated correctly
                        CategoryComboBox.ItemsSource = categories;
                        CategoryComboBox.DisplayMemberPath = "CategoryName"; // Display the category name
                        CategoryComboBox.SelectedValuePath = "CatID"; // Bind the category ID to the selected value
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading categories into the combo box.");
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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
