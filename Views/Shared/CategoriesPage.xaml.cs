using Inv_M_Sys.Services;
using Inv_M_Sys.Models;
using Inv_M_Sys.Views.Forms;
using Inv_M_Sys.Views.Main;
using Npgsql;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Inv_M_Sys.Views.Shared
{
    /// <summary>
    /// Interaction logic for CategoriesPage.xaml
    /// </summary>
    public partial class CategoriesPage : Page
    {
        #region Private Fields

        private readonly HomeWindow _homeWindow;
        private ObservableCollection<Category> CategoriesList = new ObservableCollection<Category>();
        private Category SelectedCategory;

        #endregion

        #region Constructor

        public CategoriesPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;

            // Load categories and apply role restrictions
            _ = LoadCategoriesAsync();
            ApplyRoleRestrictions();
        }

        #endregion

        #region Role-based Restrictions

        /// <summary>
        /// Restrict actions based on user role.
        /// Selling staff are not allowed to modify categories.
        /// </summary>
        private void ApplyRoleRestrictions()
        {
            if (SessionManager.CurrentUserRole == UserRole.SellingStaff.ToString())
            {
                DisableCategoryEditing();
            }
        }

        /// <summary>
        /// Disable buttons for editing categories (New, Edit, Delete) for restricted roles.
        /// </summary>
        private void DisableCategoryEditing()
        {
            NewBtn.IsEnabled = false;
            UpdateBtn.IsEnabled = false;
            DeleteBtn.IsEnabled = false;
            MessageBox.Show("You do not have permission to modify categories.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        #endregion

        #region Database Operations

        /// <summary>
        /// Load categories from the database asynchronously.
        /// </summary>
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
                            CategoriesList.Add(new Category
                            {
                                CatID = reader.GetInt32(0),
                                CategoryName = reader.GetString(1)
                            });
                        }
                    }
                }
                CategorysListView.ItemsSource = CategoriesList;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading categories from the database.");
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Add a new category to the database.
        /// </summary>
        private async Task AddCategory()
        {
            var categoryName = CategoryTextBox.Text.Trim();

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
                Log.Error(ex, "Error adding category.");
                MessageBox.Show($"Error adding category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Update an existing category in the database.
        /// </summary>
        private async Task UpdateCategory()
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("Please select a category to update.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newCategoryName = CategoryTextBox.Text.Trim();
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
                Log.Error(ex, "Error updating category.");
                MessageBox.Show($"Error updating category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Delete a category from the database.
        /// </summary>
        private async Task DeleteCategory()
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("Please select a category to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete '{SelectedCategory.CategoryName}'?",
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
                Log.Error(ex, "Error deleting category.");
                MessageBox.Show($"Error deleting category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Button Click Events

        /// <summary>
        /// Handles the New Category button click.
        /// </summary>
        private async void New_Click(object sender, RoutedEventArgs e) => await AddCategory();

        /// <summary>
        /// Handles the Update Category button click.
        /// </summary>
        private async void Update_Click(object sender, RoutedEventArgs e) => await UpdateCategory();

        /// <summary>
        /// Handles the Delete Category button click.
        /// </summary>
        private async void Delete_Click(object sender, RoutedEventArgs e) => await DeleteCategory();

        #endregion

        #region UI Interaction

        /// <summary>
        /// Handles the Search functionality for categories.
        /// </summary>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var query = RoundedTextBox.Text.ToLower();
            CategorysListView.ItemsSource = string.IsNullOrEmpty(query)
                ? CategoriesList
                : new ObservableCollection<Category>(CategoriesList.Where(c =>
                    c.CategoryName.ToLower().Contains(query)));
        }

        /// <summary>
        /// Refresh the categories list.
        /// </summary>
        private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadCategoriesAsync();

        /// <summary>
        /// Handles the selection of a category from the list.
        /// </summary>
        private void CategorysListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategorysListView.SelectedItem is Category category)
            {
                SelectedCategory = category;
                CategoryTextBox.Text = category.CategoryName;
            }
        }

        #endregion

        #region Navigation & Utility

        /// <summary>
        /// Handles the Logout functionality.
        /// </summary>
        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        /// <summary>
        /// Handles the Close functionality.
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the Minimize functionality.
        /// </summary>
        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        /// <summary>
        /// Navigate to Notifications Page.
        /// </summary>
        private void Note_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.NotificationsPage(_homeWindow));

        /// <summary>
        /// Navigate to Home Page.
        /// </summary>
        private void Home_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));

        #endregion

        private void RoundedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Handle visibility of search placeholder based on the text in the search box
            SearchPlaceholderText.Visibility = string.IsNullOrWhiteSpace(RoundedTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Handle visibility of category name placeholder based on the text in the category textbox
            CategoryPlaceholderText.Visibility = string.IsNullOrEmpty(CategoryTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

