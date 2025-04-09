using Inv_M_Sys.Models;
using Inv_M_Sys.Services;
using Inv_M_Sys.Views.Forms;
using Inv_M_Sys.Views.Main;
using Npgsql;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Inv_M_Sys.Views.Shared
{
    public partial class AwaitingPage : Page
    {
        private readonly HomeWindow _homeWindow;
        private ObservableCollection<Order> OrdersList = new ObservableCollection<Order>();
        private Order SelectedOrder;

        public AwaitingPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
            FilterComboBox.SelectedIndex = 0;
        }

        #region Top Menu
        // Logs out the current user and ends their session.
        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        // Safely shuts down the application after expiring the session.
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        // Minimizes the current application window.
        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        // Navigates back to the home dashboard window.
        private void Home_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));
        #endregion

        #region ListView Selection & Open
        // Updates the selected order when a new item is selected in the ListView.
        private void OrdersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedOrder = (Order)OrdersListView.SelectedItem;
        }

        // Loads and displays detailed information of the selected order if available.
        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedOrder != null)
            {
                await LoadOrderItemsAsync(SelectedOrder.Id);
                ShowOrderInfo();
            }
            else
            {
                MessageBox.Show("Please select an order to view.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

        #region Delete

        // Handles the soft deletion (logical deletion) of the selected order after confirmation.
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedOrder != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete Order #{SelectedOrder.Id}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                try
                {
                    await SoftDeleteOrderAsync(SelectedOrder.Id);

                    Log.Information("Order #{OrderID} soft-deleted by user: {User}.", SelectedOrder.Id, SessionManager.CurrentUser?.Username ?? "Unknown");

                    MessageBox.Show("Order deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadOrdersAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error soft-deleting order.");
                    MessageBox.Show($"Error deleting order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select an order to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Executes the soft delete in the database by updating IsDeleted, DeletedAt, and DeletedBy fields.
        private async Task SoftDeleteOrderAsync(int orderId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(@"
                    UPDATE Orders 
                    SET IsDeleted = TRUE, 
                    DeletedAt = CURRENT_TIMESTAMP, 
                    DeletedBy = @User 
                    WHERE Id = @OrderID", conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    cmd.Parameters.AddWithValue("@User", SessionManager.CurrentUser?.Username ?? "Unknown");
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        #endregion

        #region Update Status
        // Updates the status of the selected order based on the dropdown selection.
        private async void UpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("No order selected.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var selectedStatus = selectedItem.Content.ToString();

                try
                {
                    using (var conn = DatabaseHelper.GetConnection())
                    {
                        await conn.OpenAsync();
                        using (var cmd = new NpgsqlCommand("UPDATE Orders SET Status = @Status WHERE Id = @Id", conn))
                        {
                            cmd.Parameters.AddWithValue("@Status", selectedStatus);
                            cmd.Parameters.AddWithValue("@Id", SelectedOrder.Id);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    MessageBox.Show("Order status updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    OrderInfoContainer.Visibility = Visibility.Collapsed;
                    await LoadOrdersAsync(); // ✅ Refresh ListView
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error updating order status.");
                }
            }
        }
        #endregion

        #region Search & Refresh
        // Searches the orders list based on selected search criteria and query input.
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var query = RoundedTextBox.Text.ToLower();
            var criteria = (SearchComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            IEnumerable<Order> filtered = OrdersList;
            switch (criteria)
            {
                case "Order ID":
                    filtered = OrdersList.Where(o => o.Id.ToString().Contains(query));
                    break;
                case "Customer":
                    filtered = OrdersList.Where(o => o.CustomerName.ToLower().Contains(query));
                    break;
                case "Delivery Date":
                    filtered = OrdersList.Where(o => o.DeliveryDate.ToString("yyyy-MM-dd").Contains(query));
                    break;
                case "Status":
                    filtered = OrdersList.Where(o => o.Status.ToString().ToLower().Contains(query));
                    break;
            }

            OrdersListView.ItemsSource = new ObservableCollection<Order>(filtered);
        }

        // Reloads all orders from the database and updates the ListView.
        private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadOrdersAsync();

        // Reloads filtered orders when the filter ComboBox (Active/Deleted/All) changes.
        private async void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadOrdersAsync();
        }
        #endregion

        #region Helpers
        // Loads orders from the database based on the selected filter (Active, Deleted, or All).
        private async Task LoadOrdersAsync()
        {
            OrdersList.Clear();

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    // Determine the filter
                    string filter = (FilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    string query = @"
                SELECT o.Id, c.FirstName || ' ' || c.LastName AS CustomerName,
                       o.DeliveryDate, o.TotalPrice, o.Status
                FROM Orders o
                JOIN Customers c ON o.CustomerId = c.Id
            ";

                    if (filter == "Active Orders")
                        query += " WHERE o.IsDeleted = FALSE";
                    else if (filter == "Deleted Orders")
                        query += " WHERE o.IsDeleted = TRUE";
                    // else show all (no WHERE clause)

                    using (var cmd = new NpgsqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            OrdersList.Add(new Order
                            {
                                Id = reader.GetInt32(0),
                                CustomerName = reader.GetString(1),
                                DeliveryDate = reader.GetDateTime(2),
                                TotalPrice = reader.GetDecimal(3),
                                Status = Enum.Parse<OrderStatus>(reader.GetString(4))
                            });
                        }
                    }
                }

                OrdersListView.ItemsSource = OrdersList;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading orders.");
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Loads the items of a specific order from the database and displays them in the UI.
        private async Task LoadOrderItemsAsync(int orderId)
        {
            var items = new List<OrderItem>();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"SELECT p.ProductName, oi.Quantity, oi.Price, oi.TotalPrice 
                                     FROM OrderItems oi 
                                     JOIN Products p ON oi.ProductId = p.Id 
                                     WHERE oi.OrderId = @OrderId";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                items.Add(new OrderItem(reader.GetString(0), reader.GetDecimal(2), reader.GetInt32(1))
                                {
                                    TotalPrice = reader.GetDecimal(3) // optional, you can also calculate price * quantity
                                });
                            }
                        }
                    }
                }

                // Populate info fields
                OrderIdTextBox.Text = SelectedOrder.Id.ToString();
                CustomerNameTextBox.Text = SelectedOrder.CustomerName;
                DeliveryDateTextBox.Text = SelectedOrder.DeliveryDate.ToString("yyyy-MM-dd");
                TotalPriceTextBox.Text = SelectedOrder.TotalPrice.ToString("F2");
                StatusComboBox.Text = SelectedOrder.Status.ToString();    
                OrderItemsListView.ItemsSource = items;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading order items.");
                MessageBox.Show($"Error loading order items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Makes the order information container visible to the user.
        private void ShowOrderInfo()
        {
            OrderInfoContainer.Visibility = Visibility.Visible;
        }

        // Hides the order information container and resets its visibility.
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            OrderInfoContainer.Visibility = Visibility.Collapsed;
        }

        // Updates the order object when a new status is selected from the ComboBox.
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedOrder != null && StatusComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var selectedStatus = selectedItem.Content.ToString();
                SelectedOrder.Status = Enum.Parse<OrderStatus>(selectedStatus);
            }
        }
        #endregion
    }
}
