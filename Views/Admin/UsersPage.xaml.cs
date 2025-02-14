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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System;
using System.Security.Cryptography;

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

        public UsersPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
            LoadRoles();
            LoadUsers();
        }

        private void LoadRoles()
        {
            RoleComboBox.Items.Clear(); // Remove all previous items

            if (SessionManager.CurrentUserRole == "Owner")
            {
                // ✅ Owners can see and assign all roles
                RoleComboBox.Items.Add(new ComboBoxItem { Content = "Admin" });
            }

            // ✅ Both Owners and Admins can add Selling and Stock Staff
            RoleComboBox.Items.Add(new ComboBoxItem { Content = "Selling Staff" });
            RoleComboBox.Items.Add(new ComboBoxItem { Content = "Stock Staff" });

            RoleComboBox.SelectedIndex = 0; // Set default selection
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordBox.Password))
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }

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
                if (SessionManager.CurrentUserRole == "Admin" && user.Role == "Admin")
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
                RoleComboBox.SelectedItem = user.Role;

                Submitbtn.Visibility = Visibility.Collapsed;
                Updatebtn.Visibility = Visibility.Visible;
                Conatiner_User.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Please select a user to edit.");
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SalesListView.SelectedItem is User user)
            {
                // ✅ Prevent Admins from Deleting Other Admins
                if (SessionManager.CurrentUserRole == "Admin" && user.Role == "Admin")
                {
                    MessageBox.Show("You cannot delete another admin.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    using (var conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();
                        using (var cmd = new NpgsqlCommand("DELETE FROM Users WHERE id = @UserID", conn))
                        {
                            cmd.Parameters.AddWithValue("@UserID", user.UserID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    UsersList.Remove(user);
                    MessageBox.Show("User deleted successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a user to delete.");
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        private void Note_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.NotificationsPage(_homeWindow));

        private void Home_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string query = RoundedTextBox.Text.ToLower();
            SalesListView.ItemsSource = string.IsNullOrEmpty(query)
                ? UsersList
                : new ObservableCollection<User>(UsersList.Where(u =>
                    u.FirstName.ToLower().Contains(query) ||
                    u.LastName.ToLower().Contains(query) ||
                    u.Email.ToLower().Contains(query)));
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => LoadUsers();

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedRole = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                // ✅ Prevent Admin from adding another Admin
                if (SessionManager.CurrentUserRole == "Admin" && selectedRole == "Admin")
                {
                    MessageBox.Show("You do not have permission to add an Admin.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Stop execution
                }

                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, Address, Role, Username, Password) 
                VALUES (@FirstName, @LastName, @Email, @Phone, @Address, @Role, @Username, @Password)", conn))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", FirstNameTextBox.Text);
                        cmd.Parameters.AddWithValue("@LastName", LastNameTextBox.Text);
                        cmd.Parameters.AddWithValue("@Email", EmailTextBox.Text);
                        cmd.Parameters.AddWithValue("@Phone", PhoneTextBox.Text);
                        cmd.Parameters.AddWithValue("@Address", AddressTextBox.Text);
                        cmd.Parameters.AddWithValue("@Role", selectedRole);
                        cmd.Parameters.AddWithValue("@Username", UsernameTextBox.Text);
                        cmd.Parameters.AddWithValue("@Password", HashPassword(PasswordBox.Password));

                        cmd.ExecuteNonQuery();
                    }
                }

                LoadUsers();
                MessageBox.Show("User added successfully.");
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedUser != null)
            {
                try
                {
                    using (var conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();
                        using (var cmd = new NpgsqlCommand(@"
                            UPDATE Users SET 
                            FirstName = @FirstName, LastName = @LastName, Email = @Email, 
                            PhoneNumber = @Phone, Address = @Address, Role = @Role, Username = @Username, Password = @Password 
                            WHERE id = @UserID", conn))
                        {
                            cmd.Parameters.AddWithValue("@FirstName", FirstNameTextBox.Text);
                            cmd.Parameters.AddWithValue("@LastName", LastNameTextBox.Text);
                            cmd.Parameters.AddWithValue("@Email", EmailTextBox.Text);
                            cmd.Parameters.AddWithValue("@Phone", PhoneTextBox.Text);
                            cmd.Parameters.AddWithValue("@Address", AddressTextBox.Text);
                            cmd.Parameters.AddWithValue("@Role", (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString());
                            cmd.Parameters.AddWithValue("@Username", UsernameTextBox.Text);
                            cmd.Parameters.AddWithValue("@Password", HashPassword(PasswordBox.Password));
                            cmd.Parameters.AddWithValue("@UserID", SelectedUser.UserID);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    LoadUsers();
                    MessageBox.Show("User updated successfully.");
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void LoadUsers()
        {
            UsersList.Clear();

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM Users", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User
                            {
                                UserID = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Email = reader.GetString(3),
                                Phone = reader.GetString(4),
                                Username = reader.GetString(5),
                                Password = reader.GetString(6),
                                Address = reader.GetString(7),
                                Role = reader.GetString(8)
                            };

                            // ✅ Restrict Admins from seeing other Admins
                            if (SessionManager.CurrentUserRole == "Admin" && user.Role == "Admin")
                            {
                                continue; // Skip adding other admins
                            }

                            UsersList.Add(user);
                        }
                    }
                }
                SalesListView.ItemsSource = UsersList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            FirstNameTextBox.Text = "";
            LastNameTextBox.Text = "";
            EmailTextBox.Text = "";
            PhoneTextBox.Text = "";
            UsernameTextBox.Text = "";
            PasswordBox.Password = "";
            AddressTextBox.Text = "";
            RoleComboBox.SelectedIndex = 0;

            Conatiner_User.Visibility = Visibility.Collapsed;
        }

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
    }

}

