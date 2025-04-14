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
using Inv_M_Sys.Services.Pages_Services;


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

        private Customer _formCustomer;
        public Customer FormCustomer
        {
            get => _formCustomer;
            set
            {
                _formCustomer = value;
                DataContext = _formCustomer;
            }
        }

        public CustomersPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
            
            _ = LoadCustomersAsync();
            ApplyRoleRestrictions();
        }

        #region User Checked
        // Checks the current logged-in user’s role and adjusts access permissions accordingly.
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

        // Applies UI restrictions if the user is a StockStaff (no access to modify customers).
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

        // Loads all customers from the database and populates the ObservableCollection.
        private async Task LoadCustomersAsync()
        {
            CustomersList.Clear();
            try
            {
                var customers = await CustomerService.GetAllAsync();
                CustomersList = new ObservableCollection<Customer>(customers);
                CustomersListView.ItemsSource = CustomersList;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error loading customers.");
            }
        }

        #region Top Menu Actions
        // Logs the user out and ends the session.
        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        // Closes the application and expires the current session.
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        // Minimizes the application window.
        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        // Navigates back to the Dashboard/Home page.
        private void Home_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));

        #endregion

        #region Search and Refresh
        // Searches customers based on selected field and query text.
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchTextBox.Text.ToLower();
            CustomersListView.ItemsSource = string.IsNullOrEmpty(query)
                ? CustomersList
                : new ObservableCollection<Customer>(CustomersList.Where(c =>
                    c.Id.ToString().ToLower().Contains(query) ||
                    c.FirstName.ToLower().Contains(query) ||
                    c.LastName.ToLower().Contains(query) ||
                    c.CompanyName.ToLower().Contains(query) ||
                    c.Email.ToLower().Contains(query) ||
                    c.PhoneNumber.ToLower().Contains(query)));
        }

        // Reloads customer data from the database and refreshes the list.
        private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadCustomersAsync();

        #endregion

        #region Buttons actions
        // Opens the form to add a new customer. Restricted for StockStaff.
        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (_isStorageStaff)
            {
                MessageBox.Show("You do not have permission to add customers.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ShowCustomerForm(isNew: true);
        }

        // Opens the form to edit the selected customer. Restricted for StockStaff.
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

        // Deletes the selected customer after confirmation. Restricted for StockStaff.
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersListView.SelectedItem is Customer customer)
            {
                var result = MessageBox.Show($"Are you sure you want to delete '{customer.FirstName} {customer.LastName}'?",
                                              "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                try
                {
                    await CustomerService.DeleteAsync(customer.Id);
                    MessageBox.Show("Customer deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadCustomersAsync();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error deleting customer.");
                }
            }
            else
            {
                MessageBox.Show("Please select a customer to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Submits a new customer entry to the database after validation.
        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                await CustomerService.AddAsync(FormCustomer);
                MessageBox.Show("Customer added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                Customer_Container.Visibility = Visibility.Collapsed;
                await LoadCustomersAsync();
            }
            catch (Exception ex)
            {
                LogError(ex, "Error adding customer.");
            }
        }

        // Updates the selected customer's information in the database after validation.
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
                FormCustomer.Id = SelectedCustomer.Id;
                await CustomerService.UpdateAsync(FormCustomer);
                MessageBox.Show("Customer updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                Customer_Container.Visibility = Visibility.Collapsed;
                await LoadCustomersAsync();
            }
            catch (Exception ex)
            {
                LogError(ex, "Error updating customer.");
            }
        }

        // Cancels the current form view and clears all input fields.
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Customer_Container.Visibility = Visibility.Collapsed;
            ClearForm();
        }

        #endregion

        #region Form Control
        // Clears all input fields in the customer form.
        private void ClearForm()
        {
            FormCustomer = new Customer();
        }

        // Validates required customer fields before saving or updating.
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

        // Displays the customer form and binds data depending on whether it's for a new or existing customer.
        private void ShowCustomerForm(bool isNew, Customer customer = null)
        {
            FormCustomer = isNew ? new Customer() : new Customer
            {
                Id = customer.Id,
                CompanyName = customer.CompanyName,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                Notes = customer.Notes
            };

            SelectedCustomer = customer;

            Customer_Container.Visibility = Visibility.Visible;
            SubmitBtn.Visibility = isNew ? Visibility.Visible : Visibility.Collapsed;
            UpdateBtn.Visibility = isNew ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion

        // Handles error logging using Serilog with fallback to local file logging if needed.
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
