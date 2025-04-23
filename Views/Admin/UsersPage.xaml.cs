using Inv_M_Sys.Models;
using Inv_M_Sys.Services;
using Inv_M_Sys.Views.Forms;
using Inv_M_Sys.Views.Main;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Security.Cryptography;
using Serilog;
using Inv_M_Sys.Services.Pages_Services;

namespace Inv_M_Sys.Views.Admin
{
    /// <summary>
    /// Interaction logic for UsersPage.xaml
    /// </summary>
    public partial class UsersPage : Page
    {
        private readonly HomeWindow _homeWindow;
        private ObservableCollection<User> UsersList = new ObservableCollection<User>();
        private User SelectedUser;
        private bool isLoading = false;

        public UsersPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
            LoadRoles();
            _ = LoadUsersAsync();
        }

        //Checking roles and load it to the combobox
        private void LoadRoles()
        {
            RoleComboBox.Items.Clear();

            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                // ✅ Owners can see and assign all roles
                if (SessionManager.CurrentUserRole == "Owner" || role != UserRole.Admin)
                {
                    if (role.ToString() == "Owner")
                    {
                        continue;                      
                    }else
                    {
                        RoleComboBox.Items.Add(new ComboBoxItem { Content = role.ToString() });
                    }
                }
            }

            RoleComboBox.SelectedIndex = 0; // Set default selection
        }

        //Make sure everything is right and filled all needed info
        private bool ValidateNewUserInputs()
        {
            var user = new User
            {
                FirstName = FirstNameTextBox.Text,
                LastName = LastNameTextBox.Text,
                Email = EmailTextBox.Text,
                Phone = PhoneTextBox.Text,
                Address = AddressTextBox.Text,
                Username = UsernameTextBox.Text,
                HashedPassword = PasswordTextBox.Text // Only temporary for validation
            };

            var errors = UserValidator.Validate(user, validatePassword: true, confirmPassword: ConfirmPasswordTextBox.Text);

            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join("\n", errors), "Validation Errors", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        //Make sure everything is right and filled all needed info for editing
        private bool ValidateEditUserInputs()
        {
            var user = new User
            {
                FirstName = FirstNameTextBox.Text,
                LastName = LastNameTextBox.Text,
                Email = EmailTextBox.Text,
                Phone = PhoneTextBox.Text,
                Address = AddressTextBox.Text,
                Username = UsernameTextBox.Text,
                HashedPassword = PasswordTextBox.Text // For comparing new password
            };

            bool hasPassword = !string.IsNullOrWhiteSpace(PasswordTextBox.Text) || !string.IsNullOrWhiteSpace(ConfirmPasswordTextBox.Text);

            var errors = UserValidator.Validate(user, validatePassword: hasPassword, confirmPassword: ConfirmPasswordTextBox.Text);

            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join("\n", errors), "Validation Errors", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        //Make sure everything is right and filled all needed info for password
        private bool ValidatePasswords(bool required = false)
        {
            bool passEmpty = string.IsNullOrWhiteSpace(PasswordTextBox.Text);
            bool confirmEmpty = string.IsNullOrWhiteSpace(ConfirmPasswordTextBox.Text);

            if (required && (passEmpty || confirmEmpty))
            {
                MessageBox.Show("Password fields cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Only check match if both are filled
            if (!passEmpty && !confirmEmpty && PasswordTextBox.Text != ConfirmPasswordTextBox.Text)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        //Control passwrod fields
        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility(PasswordTextBox, PasswordPlaceholder);
        }

        private void ConfirmPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility(ConfirmPasswordTextBox, ConfirmPasswordPlaceholder);
        }

        private void UpdatePlaceholderVisibility(TextBox textBox, TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrWhiteSpace(textBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HandleFocus(TextBox textBox, TextBlock placeholder)
        {
            placeholder.Visibility = Visibility.Collapsed;
        }

        private void HandleLostFocus(TextBox textBox, TextBlock placeholder)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
                placeholder.Visibility = Visibility.Visible;
        }

        //control password fileds end here

        //Check if pass word field is choosed or not
        private void PasswordTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender == PasswordTextBox)
                HandleFocus(PasswordTextBox, PasswordPlaceholder);
            else if (sender == ConfirmPasswordTextBox)
                HandleFocus(ConfirmPasswordTextBox, ConfirmPasswordPlaceholder);
        }

        private void PasswordTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender == PasswordTextBox)
                HandleLostFocus(PasswordTextBox, PasswordPlaceholder);
            else if (sender == ConfirmPasswordTextBox)
                HandleLostFocus(ConfirmPasswordTextBox, ConfirmPasswordPlaceholder);
        }
        //end check password

        //Control buttons to open container or allowing editing by preading the info or deleting user
        private void New_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            Conatiner_User.Visibility = Visibility.Visible;
            Submitbtn.Visibility = Visibility.Visible;
            Updatebtn.Visibility = Visibility.Collapsed;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (SalesListView.SelectedItem is User user)
            {
                // ✅ Prevent Admins from Editing Other Admins
                if (SessionManager.CurrentUserRole == "Admin" && user.Role == UserRole.Admin)
                {
                    MessageBox.Show("You cannot edit another admin.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SelectedUser = user;
                FirstNameTextBox.Text = user.FirstName;
                LastNameTextBox.Text = user.LastName;
                EmailTextBox.Text = user.Email;
                PhoneTextBox.Text = user.Phone;
                AddressTextBox.Text = user.Address;
                UsernameTextBox.Text = user.Username;
                foreach (var item in RoleComboBox.Items)
                {
                    if (item is ComboBoxItem comboBoxItem && comboBoxItem.Content.ToString() == user.Role.ToString())
                    {
                        RoleComboBox.SelectedItem = comboBoxItem;
                        break;
                    }
                }

                Submitbtn.Visibility = Visibility.Collapsed;
                Updatebtn.Visibility = Visibility.Visible;
                Conatiner_User.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Please select a user to edit.");
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SalesListView.SelectedItem is User user)
            {
                if (SessionManager.CurrentUserRole == "Admin" && user.Role == UserRole.Admin)
                {
                    MessageBox.Show("You cannot delete another admin.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    await UserService.DeleteAsync(user.UserID);
                    UsersList.Remove(user);
                    MessageBox.Show("User deleted successfully.");
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a user to delete.");
            }
        }
        //end control buttons

        //top minue control for navigation and loing out
        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        private void Home_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));
        //end top menu

        //functions to search specifc item or refresh the page
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string query = RoundedTextBox.Text.ToLower();
            SalesListView.ItemsSource = string.IsNullOrEmpty(query)
                ? UsersList
                : new ObservableCollection<User>(UsersList.Where(u =>
                    u.UserID.ToString().Contains(query) ||
                    u.FirstName.ToLower().Contains(query) ||
                    u.LastName.ToLower().Contains(query) ||
                    u.Email.ToLower().Contains(query) ||
                    u.Role.ToString().ToLower().Contains(query)));
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadUsersAsync();
        //end search and refresh

        //control adding or updating data to the database
        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            await AddUserAsync(); // Call the async method
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            await UpdateUserAsync();
        }
        //end control adding or updating

        //close the Container and clear the form
        private void Back_Click(object sender, RoutedEventArgs e) => ClearForm();

        //async to load the users to the userlist
        private async Task LoadUsersAsync()
        {
            if (isLoading) return;
            isLoading = true;
            LoadingIndicator.Visibility = Visibility.Visible;

            try
            {
                var users = await UserService.GetAllAsync();
                UsersList = new ObservableCollection<User>(users);
                SalesListView.ItemsSource = UsersList;
            }
            catch (Exception ex)
            {
                LogError(ex);
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isLoading = false;
                LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        //clearing the container and closing it
        private void ClearForm()
        {
            FirstNameTextBox.Text = "";
            LastNameTextBox.Text = "";
            EmailTextBox.Text = "";
            PhoneTextBox.Text = "";
            UsernameTextBox.Text = "";
            PasswordTextBox.Text = "";
            ConfirmPasswordTextBox.Text = "";
            AddressTextBox.Text = "";
            RoleComboBox.SelectedIndex = 0;

            Conatiner_User.Visibility = Visibility.Collapsed;

            UpdatePlaceholderVisibility(PasswordTextBox, PasswordPlaceholder);
            UpdatePlaceholderVisibility(ConfirmPasswordTextBox, ConfirmPasswordPlaceholder);
        }

        //secure the password
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2")); // Convert to hexadecimal string
                }
                return builder.ToString();
            }
        }

        //check if username is used by someone else
        private async Task<bool> UsernameExistsAsync(string username, int? userIdToExclude = null)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";

                    if (userIdToExclude.HasValue)
                    {
                        query += " AND Id != @UserId";
                    }

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        if (userIdToExclude.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@UserId", userIdToExclude.Value);
                        }

                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }

        //mkae the logging 
        private void LogError(Exception ex)
        {
            // Log the error using Serilog setup from the LoggerSetup class
            Log.Logger.Error(ex, "An error occurred at {Time}", DateTime.Now);
        }

        //adding new user to the database (used direct databse code for the examn only)
        private async Task AddUserAsync()
        {
            try
            {
                Submitbtn.IsEnabled = false;
                Updatebtn.IsEnabled = false;

                UserRole selectedRoleEnum = Enum.Parse<UserRole>((RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString());

                if (SessionManager.CurrentUserRole == "Admin" && selectedRoleEnum == UserRole.Admin)
                {
                    MessageBox.Show("You do not have permission to add an Admin.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidatePasswords()) return;
                if (!ValidateNewUserInputs()) return;

                if (await UserService.UsernameExistsAsync(UsernameTextBox.Text))
                {
                    MessageBox.Show("This username is already taken.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newUser = new User
                {
                    FirstName = FirstNameTextBox.Text,
                    LastName = LastNameTextBox.Text,
                    Email = EmailTextBox.Text,
                    Phone = PhoneTextBox.Text,
                    Address = AddressTextBox.Text,
                    Role = selectedRoleEnum,
                    Username = UsernameTextBox.Text,
                    HashedPassword = HashPassword(PasswordTextBox.Text)
                };

                await UserService.AddAsync(newUser);

                await LoadUsersAsync();
                MessageBox.Show("User added successfully.");
                ClearForm();
            }
            catch (Exception ex)
            {
                LogError(ex);
                MessageBox.Show($"Error adding user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Submitbtn.IsEnabled = true;
                Updatebtn.IsEnabled = true;
            }
        }

        //updating exsisting user info in the databse. 
        private async Task UpdateUserAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                Submitbtn.IsEnabled = false;
                Updatebtn.IsEnabled = false;

                if (!ValidateEditUserInputs()) return;

                if (!string.IsNullOrWhiteSpace(PasswordTextBox.Text))
                {
                    if (!ValidatePasswords()) return;
                }

                if (await UserService.UsernameExistsAsync(UsernameTextBox.Text, SelectedUser.UserID))
                {
                    MessageBox.Show("This username is already taken.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var updatedUser = new User
                {
                    UserID = SelectedUser.UserID,
                    FirstName = FirstNameTextBox.Text,
                    LastName = LastNameTextBox.Text,
                    Email = EmailTextBox.Text,
                    Phone = PhoneTextBox.Text,
                    Address = AddressTextBox.Text,
                    Role = Enum.TryParse<UserRole>((RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(), out var role) ? role : UserRole.SellingStaff,
                    Username = UsernameTextBox.Text,
                    HashedPassword = string.IsNullOrWhiteSpace(PasswordTextBox.Text) ? SelectedUser.HashedPassword : HashPassword(PasswordTextBox.Text)
                };

                await UserService.UpdateAsync(updatedUser);
                await LoadUsersAsync();
                MessageBox.Show("✅ User updated successfully.");
                ClearForm();
            }
            catch (Exception ex)
            {
                LogError(ex);
                MessageBox.Show($"❌ Error updating user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Submitbtn.IsEnabled = true;
                Updatebtn.IsEnabled = true;
            }
        }


    }
}

