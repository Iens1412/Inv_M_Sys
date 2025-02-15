using Inv_M_Sys.Services;
using Inv_M_Sys.Views.Forms;
using Inv_M_Sys.Views.Main;
using System;
using System.Collections.Generic;
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
using Inv_M_Sys.Models;
using Npgsql;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Inv_M_Sys.Views.Shared
{
    /// <summary>
    /// Interaction logic for CategoriesPage.xaml
    /// </summary>
    public partial class CategoriesPage : Page
    {
        private readonly HomeWindow _homeWindow;
        private ObservableCollection<Category> CategoriesList = new ObservableCollection<Category>();
        private Category SelectedCategory;

        public CategoriesPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
            _ = LoadCategoriesAsync();
            ApplyRoleRestrictions();
        }

        private void ApplyRoleRestrictions()
        {
            // 🔹 Restrict Selling Staff from modifying categories
            if (SessionManager.CurrentUserRole == UserRole.SellingStaff.ToString())
            {
                NewBtn.IsEnabled = false;
                UpdateBtn.IsEnabled = false;
                DeleteBtn.IsEnabled = false;
                MessageBox.Show("You do not have permission to modify categories.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            CategoriesList.Clear();

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM Categories", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Category category = new Category
                            {
                                CatID = reader.GetInt32(0),
                                CategoryName = reader.GetString(1)
                            };
                            CategoriesList.Add(category);
                        }
                    }
                }
                CategorysListView.ItemsSource = CategoriesList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddCategory()
        {
            string categoryName = CategoryTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                MessageBox.Show("Category name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand("INSERT INTO Categories (Name) VALUES (@CategoryName)", conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryName", categoryName);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show("Category added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CategoryTextBox.Clear();
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateCategory()
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("Please select a category to update.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newCategoryName = CategoryTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(newCategoryName))
            {
                MessageBox.Show("Category name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand("UPDATE Categories SET Name = @CategoryName WHERE Id = @CatID", conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryName", newCategoryName);
                        cmd.Parameters.AddWithValue("@CatID", SelectedCategory.CatID);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show("Category updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CategoryTextBox.Clear();
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteCategory()
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("Please select a category to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete '{SelectedCategory.CategoryName}'?",
                                                      "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand("DELETE FROM Categories WHERE Id = @CatID", conn))
                    {
                        cmd.Parameters.AddWithValue("@CatID", SelectedCategory.CatID);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show("Category deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CategoryTextBox.Clear();
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void New_Click(object sender, RoutedEventArgs e) => await AddCategory();

        private async void Update_Click(object sender, RoutedEventArgs e) => await UpdateCategory();

        private async void Delete_Click(object sender, RoutedEventArgs e) => await DeleteCategory();

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string query = RoundedTextBox.Text.ToLower();
            CategorysListView.ItemsSource = string.IsNullOrEmpty(query)
                ? CategoriesList
                : new ObservableCollection<Category>(CategoriesList.Where(c =>
                    c.CategoryName.ToLower().Contains(query)));
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadCategoriesAsync();
        }

        private void CategorysListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategorysListView.SelectedItem is Category category)
            {
                SelectedCategory = category;
                CategoryTextBox.Text = category.CategoryName;
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        private void Note_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.NotificationsPage(_homeWindow));

        private void Home_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));

        private void CategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CategoryPlaceholderText.Visibility = string.IsNullOrEmpty(CategoryTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RoundedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholderText.Visibility = string.IsNullOrWhiteSpace(RoundedTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

    }
}
