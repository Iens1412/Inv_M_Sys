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
        public ObservableCollection<Order> OrdersList { get; set; } = new();
        private ObservableCollection<Order> _fullOrderList = new();

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

        private ObservableCollection<OrderItem> _orderItems = new();
        public ObservableCollection<OrderItem> OrderItems
        {
            get => _orderItems;
            set => SetProperty(ref _orderItems, value);
        }

        public string SelectedCustomerName => SelectedOrder?.CustomerName ?? "";
        public string SelectedDeliveryDate => SelectedOrder?.DeliveryDate.ToString("yyyy-MM-dd") ?? "";
        public string SelectedTotalPrice => SelectedOrder?.TotalPrice.ToString("F2") ?? "";
        public string SelectedStatus => SelectedOrder?.Status.ToString() ?? "";

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

        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                    _ = LoadOrdersAsync();
            }
        }
        private string _selectedFilter = "Active Orders";

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ObservableCollection<string> SearchCriteriaOptions { get; } = new(new[] { "Order ID", "Customer", "Delivery Date", "Status" });
        public ObservableCollection<string> FilterOptions { get; } = new(new[] { "All Orders", "Active Orders", "Deleted Orders" });
        public ObservableCollection<string> StatusOptions { get; } = new(new[] { "Pending", "Shipped", "Delivered", "Cancelled" });

        public ICommand RefreshCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand OpenCommand { get; }

        public AwaitingPageViewModel()
        {
            RefreshCommand = new RelayCommand(async () => await LoadOrdersAsync());
            DeleteCommand = new RelayCommand(async () => await SoftDeleteOrderAsync());
            UpdateStatusCommand = new RelayCommand(async () => await UpdateOrderStatusAsync());
            SearchCommand = new RelayCommand(SearchOrders);
            OpenCommand = new RelayCommand(async () => await OpenSelectedOrderAsync());

            _ = LoadOrdersAsync();
        }

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

        private async Task SoftDeleteOrderAsync()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Please select an order to delete.");
                return;
            }

            if (MessageBox.Show($"Delete Order #{SelectedOrder.Id}?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            using var conn = DatabaseHelper.GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(@"UPDATE Orders SET IsDeleted = TRUE, DeletedAt = CURRENT_TIMESTAMP, DeletedBy = @User WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", SelectedOrder.Id);
            cmd.Parameters.AddWithValue("@User", SessionManager.CurrentUser?.Username ?? "Unknown");
            await cmd.ExecuteNonQueryAsync();

            await LoadOrdersAsync();
        }

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

            await LoadOrdersAsync();
        }

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
                    items.Add(new OrderItem(reader.GetString(0), reader.GetDecimal(2), reader.GetInt32(1))
                    {
                        TotalPrice = reader.GetDecimal(3)
                    });
                }

                OrderItems = items;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading order items.");
                MessageBox.Show("Failed to load order items.");
            }
        }

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
    }
}