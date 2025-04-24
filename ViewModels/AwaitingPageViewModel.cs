using Inv_M_Sys.Models;
using Inv_M_Sys.Services;
using Inv_M_Sys.Commands;
using Npgsql;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Inv_M_Sys.Helpers;

namespace Inv_M_Sys.ViewModels
{
    public class AwaitingPageViewModel : BaseViewModel
    {
        //control the list of orders
        private ObservableCollection<Order> _ordersList = new();
        public ObservableCollection<Order> OrdersList
        {
            get => _ordersList;
            set
            {
                _ordersList = value;
                OnPropertyChanged(nameof(OrdersList));
            }
        }
        private ObservableCollection<Order> _fullOrderList = new();

        //get teh info of the choosed order
        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (SetProperty(ref _selectedOrder, value))
                {
                    OnPropertyChanged(nameof(SelectedCustomerName));
                    OnPropertyChanged(nameof(SelectedDeliveryDate));
                    OnPropertyChanged(nameof(SelectedTotalPrice));
                    OnPropertyChanged(nameof(SelectedStatus));
                }
            }
        }

        // when order item changes
        private ObservableCollection<OrderItem> _orderItems = new();
        public ObservableCollection<OrderItem> OrderItems
        {
            get => _orderItems;
            set
            {
                _orderItems = value;
                OnPropertyChanged(nameof(OrderItems));
            }
        }

        //needed vars
        public string SelectedCustomerName => SelectedOrder?.CustomerName ?? "";
        public string SelectedDeliveryDate => SelectedOrder?.DeliveryDate.ToString("yyyy-MM-dd") ?? "";
        public string SelectedTotalPrice => SelectedOrder?.TotalPrice.ToString("F2") ?? "";
        public string SelectedStatus => SelectedOrder?.Status.ToString() ?? "";

        //filter for the orders
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }
        private string _searchQuery;

        public string SelectedSearchCriteria
        {
            get => _selectedSearchCriteria;
            set => SetProperty(ref _selectedSearchCriteria, value);
        }
        private string _selectedSearchCriteria = "Order ID";

        //type of the filter
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                    _ = LoadOrdersAsync();
                OnPropertyChanged(nameof(IsDeleteVisible));
                OnPropertyChanged(nameof(IsStatusEditable));
                OnPropertyChanged(nameof(IsUpdateEnabled));
            }
        }
        private string _selectedFilter = "Active Orders";


        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ICommand RestoreCommand { get; }

        //for the combobox and status
        public bool IsStatusEditable => SelectedFilter != "Deleted Orders";
        public bool IsDeleteVisible => SelectedFilter != "Deleted Orders";
        public bool IsUpdateEnabled => SelectedFilter != "Deleted Orders";
        public ObservableCollection<string> SearchCriteriaOptions { get; } = new(new[] { "Order ID", "Customer", "Delivery Date", "Status" });
        public ObservableCollection<string> FilterOptions { get; } = new(new[] { "All Orders", "Active Orders", "Deleted Orders" });
        public ObservableCollection<OrderStatus> StatusOptions { get; } =
            new(Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>());

        //define the commands
        public ICommand RefreshCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand OpenCommand { get; }

        public AwaitingPageViewModel()
        {
            //initilizing the commands
            RefreshCommand = new RelayCommand(async () => await LoadOrdersAsync());
            DeleteCommand = new RelayCommand(async () => await SoftDeleteOrderAsync());
            UpdateStatusCommand = new RelayCommand(async () => await UpdateOrderStatusAsync());
            SearchCommand = new RelayCommand(SearchOrders);
            OpenCommand = new RelayCommand(async () => await OpenSelectedOrderAsync());
            RestoreCommand = new RelayCommand(async () => await RestoreDeletedOrderAsync());

            _ = LoadOrdersAsync();
        }

        //loading the orders from the DB
        public async Task LoadOrdersAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                string query = @"SELECT o.Id, c.FirstName || ' ' || c.LastName AS CustomerName, o.DeliveryDate, o.TotalPrice, o.Status FROM Orders o JOIN Customers c ON o.CustomerId = c.Id";
                if (SelectedFilter == "Active Orders") query += " WHERE o.IsDeleted = FALSE";
                else if (SelectedFilter == "Deleted Orders") query += " WHERE o.IsDeleted = TRUE";

                using var cmd = new NpgsqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var tempList = new ObservableCollection<Order>();
                while (await reader.ReadAsync())
                {
                    tempList.Add(new Order
                    {
                        Id = reader.GetInt32(0),
                        CustomerName = reader.GetString(1),
                        DeliveryDate = reader.GetDateTime(2),
                        TotalPrice = reader.GetDecimal(3),
                        Status = Enum.Parse<OrderStatus>(reader.GetString(4))
                    });
                }

                _fullOrderList = tempList;
                OrdersList = new ObservableCollection<Order>(_fullOrderList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading orders.");
                MessageBox.Show("Failed to load orders.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        //Filter the orders
        private void SearchOrders()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                OrdersList = new ObservableCollection<Order>(_fullOrderList);
                return;
            }

            var query = SearchQuery.ToLower();
            var filtered = _fullOrderList.Where(o =>
                (SelectedSearchCriteria == "Order ID" && o.Id.ToString().Contains(query)) ||
                (SelectedSearchCriteria == "Customer" && o.CustomerName.ToLower().Contains(query)) ||
                (SelectedSearchCriteria == "Delivery Date" && o.DeliveryDate.ToString("yyyy-MM-dd").Contains(query)) ||
                (SelectedSearchCriteria == "Status" && o.Status.ToString().ToLower().Contains(query))
            );

            OrdersList = new ObservableCollection<Order>(filtered);
        }

        //soft delete the orders so the user still can use them after resoting them
        private async Task SoftDeleteOrderAsync()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Please select an order to delete.");
                return;
            }

            if (MessageBox.Show($"Delete Order #{SelectedOrder.Id}?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                using var transaction = await conn.BeginTransactionAsync();

                // 1. Restore product quantities from OrderItems
                var itemsCmd = new NpgsqlCommand(@"
            SELECT ProductId, Quantity 
            FROM OrderItems 
            WHERE OrderId = @OrderId", conn);
                itemsCmd.Parameters.AddWithValue("@OrderId", SelectedOrder.Id);

                var itemList = new List<(int productId, int quantity)>();
                using (var reader = await itemsCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        itemList.Add((reader.GetInt32(0), reader.GetInt32(1)));
                    }
                }

                foreach (var (productId, quantity) in itemList)
                {
                    var updateProductCmd = new NpgsqlCommand(@"
                        UPDATE Products 
                        SET Quantity = Quantity + @Quantity 
                        WHERE Id = @ProductId", conn);
                    updateProductCmd.Parameters.AddWithValue("@Quantity", quantity);
                    updateProductCmd.Parameters.AddWithValue("@ProductId", productId);
                    await updateProductCmd.ExecuteNonQueryAsync();
                }

                // 2. Soft delete the order
                var deleteCmd = new NpgsqlCommand(@"
                    UPDATE Orders 
                    SET IsDeleted = TRUE, DeletedAt = CURRENT_TIMESTAMP, DeletedBy = @User 
                    WHERE Id = @Id", conn);
                deleteCmd.Parameters.AddWithValue("@Id", SelectedOrder.Id);
                deleteCmd.Parameters.AddWithValue("@User", SessionManager.CurrentUser?.Username ?? "Unknown");
                await deleteCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete order and restore product quantities.");
                MessageBox.Show("❌ Error deleting order: " + ex.Message);
            }
        }

        //update the state of the order
        private async Task UpdateOrderStatusAsync()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("No order selected.");
                return;
            }

            using var conn = DatabaseHelper.GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("UPDATE Orders SET Status = @Status WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Status", SelectedOrder.Status.ToString());
            cmd.Parameters.AddWithValue("@Id", SelectedOrder.Id);
            await cmd.ExecuteNonQueryAsync();
            MessageBox.Show("Order status updated successfully.");

            await LoadOrdersAsync();

            Messenger.Default.Send("HideOrderInfo");
        }

        //load the orders items to show details of the orders
        public async Task LoadOrderItemsAsync()
        {
            if (SelectedOrder == null) return;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();
                var items = new ObservableCollection<OrderItem>();
                string query = @"SELECT p.ProductName, oi.Quantity, oi.Price, oi.TotalPrice FROM OrderItems oi JOIN Products p ON oi.ProductId = p.Id WHERE oi.OrderId = @OrderId";

                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrderId", SelectedOrder.Id);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    items.Add(new OrderItem(reader.GetString(0), reader.GetDecimal(2), reader.GetInt32(1)));
                }

                OrderItems = items;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading order items.");
                MessageBox.Show("Failed to load order items.");
            }
        }

        //load the selected order in into the right place
        private async Task OpenSelectedOrderAsync()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Please select an order to open.");
                return;
            }

            await LoadOrderItemsAsync();

            // Visibility is still controlled from the View's code-behind
            Messenger.Default.Send("ShowOrderInfo");
        }

        //restore deleted orders
        private async Task RestoreDeletedOrderAsync()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Please select an order to restore.");
                return;
            }

            var result = MessageBox.Show($"Restore Order #{SelectedOrder.Id}?", "Confirm", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                using var transaction = await conn.BeginTransactionAsync();

                // 1. Get the order status
                OrderStatus orderStatus = OrderStatus.Pending;
                var statusCmd = new NpgsqlCommand("SELECT Status FROM Orders WHERE Id = @Id", conn);
                statusCmd.Parameters.AddWithValue("@Id", SelectedOrder.Id);
                var statusStr = (string?)await statusCmd.ExecuteScalarAsync();
                if (!string.IsNullOrEmpty(statusStr))
                    orderStatus = Enum.Parse<OrderStatus>(statusStr);

                // 2. If the order is NOT cancelled, decrease stock again
                if (orderStatus != OrderStatus.Cancelled)
                {
                    var itemsCmd = new NpgsqlCommand(@"
                        SELECT ProductId, Quantity 
                        FROM OrderItems 
                        WHERE OrderId = @OrderId", conn);
                    itemsCmd.Parameters.AddWithValue("@OrderId", SelectedOrder.Id);

                    var items = new List<(int ProductId, int Quantity)>();
                    using (var reader = await itemsCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            items.Add((reader.GetInt32(0), reader.GetInt32(1)));
                        }
                    }

                    foreach (var (productId, quantity) in items)
                    {
                        var updateCmd = new NpgsqlCommand(@"
                    UPDATE Products 
                    SET Quantity = Quantity - @Quantity 
                    WHERE Id = @ProductId", conn);
                        updateCmd.Parameters.AddWithValue("@Quantity", quantity);
                        updateCmd.Parameters.AddWithValue("@ProductId", productId);
                        await updateCmd.ExecuteNonQueryAsync();
                    }
                }

                // 3. Restore the order
                var restoreCmd = new NpgsqlCommand(@"
                    UPDATE Orders 
                    SET IsDeleted = FALSE, DeletedAt = NULL, DeletedBy = NULL 
                    WHERE Id = @Id", conn);
                restoreCmd.Parameters.AddWithValue("@Id", SelectedOrder.Id);
                await restoreCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                MessageBox.Show($"Order #{SelectedOrder.Id} restored.");
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error restoring order.");
                MessageBox.Show("Failed to restore order.");
            }
        }
    }
}