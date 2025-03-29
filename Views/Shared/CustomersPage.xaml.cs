using Inv_M_Sys.Services;
using Inv_M_Sys.Models;
using Npgsql;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Inv_M_Sys.Views.Forms;
using System.Net.NetworkInformation;
using Inv_M_Sys.Views.Main;
using Serilog;


namespace Inv_M_Sys.Views.Shared
{
    /// <summary>
    /// Interaction logic for CustomersPage.xaml
    /// </summary>
    public partial class CustomersPage : Page
    {
        private readonly HomeWindow _homeWindow;
        private ObservableCollection<Customer> CustomersList = new ObservableCollection<Customer>();
        private Customer SelectedCustomer;
        private bool _isStorageStaff;

        public CustomersPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
            
            _ = LoadCustomersAsync();
            ApplyRoleRestrictions();
        }

        #region User Checked
        private void CheckUser()
        {
            string currentUserRole = SessionManager.CurrentUserRole;
            _isStorageStaff = false; // Default to false

            MessageBox.Show($"Current User Role: {currentUserRole}", "User Info", MessageBoxButton.OK, MessageBoxImage.Information);

            switch (currentUserRole)
            {
                case var role when role == UserRole.Admin.ToString() || role == UserRole.Owner.ToString():
                    MessageBox.Show("Admin Access Granted");
                    break;

                case var role when role == UserRole.StockStaff.ToString():
                    _isStorageStaff = true;
                    ApplyRoleRestrictions();
                    MessageBox.Show("Storage Staff Access - Limited Permissions");
                    break;

                case var role when role == UserRole.SellingStaff.ToString():
                    MessageBox.Show("Selling Staff Access - Granted");
                    break;

                default:
                    MessageBox.Show("Unknown Role - Contact Admin", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }

        private void ApplyRoleRestrictions()
        {
            if (_isStorageStaff)
            {
                newbtn.IsEnabled = false;
                EditBtn.IsEnabled = false;
                DeleteBtn.IsEnabled = false;
                SubmitBtn.IsEnabled = false;
                UpdateBtn.IsEnabled = false;
                Customer_Container.Visibility = Visibility.Collapsed; // Hide the add/edit form

                MessageBox.Show("You do not have permission to modify customers.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

        private async Task LoadCustomersAsync()
        {
            CustomersList.Clear();

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM Customers", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Customer customer = new Customer
                            {
                                Id = reader.GetInt32(0),
                                CompanyName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                FirstName = reader.GetString(2),
                                LastName = reader.GetString(3),
                                Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                PhoneNumber = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                Address = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                Notes = reader.IsDBNull(7) ? "" : reader.GetString(7)
                            };
                            CustomersList.Add(customer);
                        }
                    }
                }
                CustomersListView.ItemsSource = CustomersList;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading customers.");  // Serilog error logging
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        #region Top Menu Actions
        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        private void Home_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));

        #endregion


        #region Search and Refresh
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchTextBox.Text.ToLower();
            CustomersListView.ItemsSource = string.IsNullOrEmpty(query)
                ? CustomersList
                : new ObservableCollection<Customer>(CustomersList.Where(c =>
                    c.FirstName.ToLower().Contains(query) ||
                    c.LastName.ToLower().Contains(query) ||
                    c.CompanyName.ToLower().Contains(query) ||
                    c.Email.ToLower().Contains(query) ||
                    c.PhoneNumber.ToLower().Contains(query)));
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadCustomersAsync();

        #endregion


        #region Buttons actions
        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (_isStorageStaff)
            {
                MessageBox.Show("You do not have permission to add customers.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ShowCustomerForm(isNew: true);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (_isStorageStaff)
            {
                MessageBox.Show("You do not have permission to edit customers.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CustomersListView.SelectedItem is Customer customer)
            {
                ShowCustomerForm(isNew: false, customer);
            }
            else
            {
                MessageBox.Show("Please select a customer to edit.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_isStorageStaff)
            {
                MessageBox.Show("You do not have permission to delete customers.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CustomersListView.SelectedItem is Customer customer)
            {
                var result = MessageBox.Show($"Are you sure you want to delete '{customer.FirstName} {customer.LastName}'?",
                                              "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                try
                {
                    using (var conn = DatabaseHelper.GetConnection())
                    {
                        await conn.OpenAsync();
                        using (var cmd = new NpgsqlCommand("DELETE FROM Customers WHERE Id = @Id", conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", customer.Id);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    MessageBox.Show("Customer deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadCustomersAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error deleting customer.");
                    MessageBox.Show($"Error deleting customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a customer to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(@"INSERT INTO Customers (CompanyName, FirstName, LastName, Email, PhoneNumber, Address, Notes) 
                                                 VALUES (@CompanyName, @FirstName, @LastName, @Email, @PhoneNumber, @Address, @Notes)", conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyName", CompanyTextBox.Text);
                        cmd.Parameters.AddWithValue("@FirstName", FirstNameTextBox.Text);
                        cmd.Parameters.AddWithValue("@LastName", LastNameTextBox.Text);
                        cmd.Parameters.AddWithValue("@Email", EmailTextBox.Text);
                        cmd.Parameters.AddWithValue("@PhoneNumber", PhoneTextBox.Text);
                        cmd.Parameters.AddWithValue("@Address", AddressTextBox.Text);
                        cmd.Parameters.AddWithValue("@Notes", NotesTextBox.Text);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show("Customer added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                Customer_Container.Visibility = Visibility.Collapsed;
                await LoadCustomersAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding customer.");
                MessageBox.Show($"Error adding customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCustomer == null)
            {
                MessageBox.Show("Please select a customer to update.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateForm()) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(@"UPDATE Customers 
                                                 SET CompanyName = @CompanyName, FirstName = @FirstName, LastName = @LastName, 
                                                     Email = @Email, PhoneNumber = @PhoneNumber, Address = @Address, Notes = @Notes 
                                                 WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyName", CompanyTextBox.Text);
                        cmd.Parameters.AddWithValue("@FirstName", FirstNameTextBox.Text);
                        cmd.Parameters.AddWithValue("@LastName", LastNameTextBox.Text);
                        cmd.Parameters.AddWithValue("@Email", EmailTextBox.Text);
                        cmd.Parameters.AddWithValue("@PhoneNumber", PhoneTextBox.Text);
                        cmd.Parameters.AddWithValue("@Address", AddressTextBox.Text);
                        cmd.Parameters.AddWithValue("@Notes", NotesTextBox.Text);
                        cmd.Parameters.AddWithValue("@Id", SelectedCustomer.Id);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show("Customer updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                Customer_Container.Visibility = Visibility.Collapsed;
                await LoadCustomersAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating customer.");
                MessageBox.Show($"Error updating customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Customer_Container.Visibility = Visibility.Collapsed;
            ClearForm();
        }

        #endregion


        #region Form Control
        private void ClearForm()
        {
            CompanyTextBox.Text = "";
            FirstNameTextBox.Text = "";
            LastNameTextBox.Text = "";
            EmailTextBox.Text = "";
            PhoneTextBox.Text = "";
            AddressTextBox.Text = "";
            NotesTextBox.Text = "";
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(CompanyTextBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Fill Company Name, First Name, Last Name, Email.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void ShowCustomerForm(bool isNew, Customer customer = null)
        {
            SelectedCustomer = customer;
            Customer_Container.Visibility = Visibility.Visible;

            if (isNew)
            {
                ClearForm();
                SubmitBtn.Visibility = Visibility.Visible;
                UpdateBtn.Visibility = Visibility.Collapsed;
            }
            else if (customer != null)
            {
                CompanyTextBox.Text = customer.CompanyName;
                FirstNameTextBox.Text = customer.FirstName;
                LastNameTextBox.Text = customer.LastName;
                EmailTextBox.Text = customer.Email;
                PhoneTextBox.Text = customer.PhoneNumber;
                AddressTextBox.Text = customer.Address;
                NotesTextBox.Text = customer.Notes;

                SubmitBtn.Visibility = Visibility.Collapsed;
                UpdateBtn.Visibility = Visibility.Visible;
            }
        }
        #endregion

        private void LogError(Exception ex, string message)
        {
            try
            {
                // Using the logger setup in LoggerSetup.cs
                Log.Error(ex, message); // Logs error using Serilog (ensure that LoggerSetup.SetupLogger() has been called during startup)
            }
            catch (Exception logEx)
            {
                // If logging fails, fallback to default logging method
                string logFilePath = "error_log.txt";
                string logEntry = $"{DateTime.Now}: {message}\n{ex}\n\n";

                try
                {
                    System.IO.File.AppendAllText(logFilePath, logEntry);
                }
                catch (Exception fileEx)
                {
                    MessageBox.Show($"Failed to write to log file: {fileEx.Message}", "Logging Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Show message box for user feedback
            MessageBox.Show($"{message}\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}
