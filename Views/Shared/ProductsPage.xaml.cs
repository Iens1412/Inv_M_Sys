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
    /// <summary>
    /// Interaction logic for the ProductsPage.
    /// This page allows users to manage products (add, edit, delete, and search).
    /// The page interacts with the database to fetch, update, and remove products.
    /// </summary>
    public partial class ProductsPage : Page
    {
        private readonly HomeWindow _homeWindow;
        private ObservableCollection<Product> ProductsList = new ObservableCollection<Product>();
        private Product SelectedProduct;

        public ProductsPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
            _ = LoadProductsAsync();
            _ = LoadCategoriesIntoComboBoxAsync();
        }

        #region Top Menu Actions
        /// <summary>
        /// Logs out the user by calling the SessionManager to expire the session.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the button).</param>
        /// <param name="e">The event arguments.</param>
        private void Logout_Click(object sender, RoutedEventArgs e) => SessionManager.Logout();

        /// <summary>
        /// Closes the application and expires the session in the database.
        /// This method ensures the session is terminated before the application shuts down.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the close button).</param>
        /// <param name="e">The event arguments.</param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.ExpireSessionInDB();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Minimizes the application window.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the minimize button).</param>
        /// <param name="e">The event arguments.</param>
        private void Minimize_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;

        /// <summary>
        /// Navigates to the NotificationsPage.
        /// This page displays notifications for the user.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the notifications button).</param>
        /// <param name="e">The event arguments.</param>
        private void Note_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new Views.Shared.NotificationsPage(_homeWindow));

        /// <summary>
        /// Navigates to the DashboardPage.
        /// This page serves as the home page for the application, showing key information.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the home button).</param>
        /// <param name="e">The event arguments.</param>
        private void Home_Click(object sender, RoutedEventArgs e) => _homeWindow.NavigateToPage(new DashboardPage(_homeWindow));
        #endregion

        #region Buttons control to add, edit, or delete products
        /// <summary>
        /// Clears the form and sets it for adding a new product.
        /// This method prepares the form to add a new product by clearing existing data 
        /// and showing the necessary buttons (Submit) for the user to fill in the form.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the "New" button).</param>
        /// <param name="e">The event arguments.</param>
        private async void New_Btn(object sender, RoutedEventArgs e)
        {
            Clear_Form();
            Submit_Btn.Visibility = Visibility.Visible;
            Update_Btn.Visibility = Visibility.Collapsed;
            Info_Container.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Edits the selected product by populating the form with its data.
        /// This method enables the user to edit the selected product by filling the form fields
        /// with the product's existing details, allowing for updates.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the "Edit" button).</param>
        /// <param name="e">The event arguments.</param>
        private async void Edit_Btn(object sender, RoutedEventArgs e)
        {
            if (SelectedProduct != null)
            {
                // Ensure categories are loaded first
                await LoadCategoriesIntoComboBoxAsync();

                Info_Container.Visibility = Visibility.Visible;
                Submit_Btn.Visibility = Visibility.Collapsed;
                Update_Btn.Visibility = Visibility.Visible;

                // Set the product's information into the form fields
                ProductNameTextBox.Text = SelectedProduct.Name;
                QuantityTextBox.Text = SelectedProduct.Quantity.ToString();
                PriceTextBox.Text = SelectedProduct.Price.ToString();
                MinQntTextBox.Text = SelectedProduct.MinQuantity.ToString();
                SupplierTextBox.Text = SelectedProduct.Supplier;
                DescriptionTextBox.Text = SelectedProduct.Description;

                // Set the selected category in the ComboBox
                CategoryComboBox.SelectedItem = CategoryComboBox.Items
                    .Cast<Category>()
                    .FirstOrDefault(c => c.CatID == SelectedProduct.CategoryId);  // Matching based on CatID
            }
            else
            {
                MessageBox.Show("Please select a product to edit.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Deletes the selected product after confirming with the user.
        /// This method prompts the user for confirmation before deleting the selected product.
        /// If confirmed, the product is removed from the database and the UI is updated.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the "Delete" button).</param>
        /// <param name="e">The event arguments.</param>
        private async void Delete_btn(object sender, RoutedEventArgs e)
        {
            if (SelectedProduct != null)
            {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete '{SelectedProduct.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await DeleteProductAsync(SelectedProduct.Id);
                        MessageBox.Show("Product deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadProductsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a product to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

        #region Form submission buttons
        /// <summary>
        /// Handles the submission of the new product form.
        /// This method validates the form inputs, creates a new product object with the entered data,
        /// and adds the product to the database. After successful submission, it clears the form and reloads the product list.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the submit button).</param>
        /// <param name="e">The event arguments.</param>
        private async void Submit_btn(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                try
                {
                    var selectedCategory = (Category)CategoryComboBox.SelectedItem;
                    var newProduct = new Product
                    {
                        Name = ProductNameTextBox.Text,
                        CategoryId = selectedCategory.CatID,
                        CategoryName = selectedCategory.CategoryName,
                        Quantity = int.Parse(QuantityTextBox.Text),
                        Price = decimal.Parse(PriceTextBox.Text),
                        MinQuantity = int.Parse(MinQntTextBox.Text),
                        Supplier = SupplierTextBox.Text,
                        Description = DescriptionTextBox.Text
                    };

                    await AddProductAsync(newProduct);
                    MessageBox.Show("Product added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Clear_Form();
                    Info_Container.Visibility = Visibility.Collapsed;
                    await LoadProductsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Handles the update of the selected product.
        /// This method validates the form inputs, updates the selected product's details with the entered data,
        /// and saves the updated product in the database. After successful update, it clears the form and reloads the product list.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the update button).</param>
        /// <param name="e">The event arguments.</param>
        private async void Update_btn(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                if (SelectedProduct != null)
                {
                    try
                    {
                        var selectedCategory = (Category)CategoryComboBox.SelectedItem;
                        SelectedProduct.Name = ProductNameTextBox.Text;
                        SelectedProduct.CategoryId = selectedCategory.CatID;
                        SelectedProduct.Quantity = int.Parse(QuantityTextBox.Text);
                        SelectedProduct.Price = decimal.Parse(PriceTextBox.Text);
                        SelectedProduct.MinQuantity = int.Parse(MinQntTextBox.Text);
                        SelectedProduct.Supplier = SupplierTextBox.Text;
                        SelectedProduct.Description = DescriptionTextBox.Text;

                        await UpdateProductAsync(SelectedProduct);
                        MessageBox.Show("Product updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Clear_Form();
                        Info_Container.Visibility = Visibility.Collapsed;
                        await LoadProductsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a product to update.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        /// <summary>
        /// Resets the form to the initial state when the Back button is clicked.
        /// This method hides the form container and clears all form fields, allowing the user to go back to the previous state.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the back button).</param>
        /// <param name="e">The event arguments.</param>
        private void Back_btn(object sender, RoutedEventArgs e)
        {
            Info_Container.Visibility = Visibility.Collapsed;
            Clear_Form();
        }
        #endregion

        #region Search and Refresh

        /// <summary>
        /// Searches for products based on the text entered in the search box and the selected search criterion.
        /// This method filters the product list according to the search query and selected criterion (e.g., Name, Category, Supplier).
        /// </summary>
        /// <param name="sender">The object that triggered the event (the search button).</param>
        /// <param name="e">The event arguments.</param>
        private void Search_btn(object sender, RoutedEventArgs e)
        {
            var query = RoundedTextBox.Text.ToLower();

            // Check the selected search criterion from the ComboBox
            var selectedCriterion = (SearchComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(selectedCriterion))
            {
                MessageBox.Show("Please select a search criterion.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<Product> filteredProducts;

            switch (selectedCriterion)
            {
                case "Name":
                    filteredProducts = ProductsList.Where(p => p.Name.ToLower().Contains(query)).ToList();
                    break;

                case "Category":
                    filteredProducts = ProductsList.Where(p => p.CategoryName.ToLower().Contains(query)).ToList();
                    break;

                case "Supplier":
                    filteredProducts = ProductsList.Where(p => p.Supplier.ToLower().Contains(query)).ToList();
                    break;

                default:
                    filteredProducts = new List<Product>();
                    break;
            }

            ProductListView.ItemsSource = new ObservableCollection<Product>(filteredProducts);
        }

        /// <summary>
        /// Refreshes the product list by reloading it from the database.
        /// This method reloads the product data from the database to ensure that the list is up-to-date.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the refresh button).</param>
        /// <param name="e">The event arguments.</param>
        private async void Refresh_btn(object sender, RoutedEventArgs e)
        {
            await LoadProductsAsync();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Clears the product form fields, resetting them to their default values.
        /// </summary>
        private void Clear_Form()
        {
            ProductNameTextBox.Text = "";
            CategoryComboBox.SelectedIndex = -1;
            QuantityTextBox.Text = "";
            PriceTextBox.Text = "";
            MinQntTextBox.Text = "";
            SupplierTextBox.Text = "";
            DescriptionTextBox.Text = "";
        }

        /// <summary>
        /// Loads the products from the database and binds them to the UI, populating the product list.
        /// </summary>
        private async Task LoadProductsAsync()
        {
            ProductsList.Clear();

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"SELECT p.Id, p.ProductName, p.CategoryId, c.Name as CategoryName, 
                             p.Quantity, p.Price, p.MinQuantity, p.Supplier, p.Description 
                             FROM Products p
                             JOIN Categories c ON p.CategoryId = c.Id";  // Corrected column name from 'CatId' to 'Id'
                    using (var cmd = new NpgsqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ProductsList.Add(new Product
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                CategoryId = reader.GetInt32(2),
                                CategoryName = reader.GetString(3),
                                Category = new Category
                                {
                                    CatID = reader.GetInt32(2),
                                    CategoryName = reader.GetString(3)
                                },
                                Quantity = reader.GetInt32(4),
                                Price = reader.GetDecimal(5),
                                MinQuantity = reader.GetInt32(6),
                                Supplier = reader.GetString(7),
                                Description = reader.GetString(8)
                            });
                        }
                    }
                }

                ProductListView.ItemsSource = ProductsList;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading products from the database.");
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Adds a new product to the database.
        /// </summary>
        /// <param name="product">The product to be added to the database.</param>
        private async Task AddProductAsync(Product product)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand("INSERT INTO Products (ProductName, CategoryId, Quantity, Price, MinQuantity, Supplier, Description) VALUES (@Name, @CategoryId, @Quantity, @Price, @MinQuantity, @Supplier, @Description)", conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", product.Name);
                        cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                        cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
                        cmd.Parameters.AddWithValue("@Price", product.Price);
                        cmd.Parameters.AddWithValue("@MinQuantity", product.MinQuantity);
                        cmd.Parameters.AddWithValue("@Supplier", product.Supplier);
                        cmd.Parameters.AddWithValue("@Description", product.Description);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates an existing product in the database.
        /// </summary>
        /// <param name="product">The product to be updated.</param>
        private async Task UpdateProductAsync(Product product)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand("UPDATE Products SET ProductName = @Name, CategoryId = @CategoryId, Quantity = @Quantity, Price = @Price, MinQuantity = @MinQuantity, Supplier = @Supplier, Description = @Description WHERE Id = @ProductID", conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductID", product.Id);
                        cmd.Parameters.AddWithValue("@Name", product.Name);
                        cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                        cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
                        cmd.Parameters.AddWithValue("@Price", product.Price);
                        cmd.Parameters.AddWithValue("@MinQuantity", product.MinQuantity);
                        cmd.Parameters.AddWithValue("@Supplier", product.Supplier);
                        cmd.Parameters.AddWithValue("@Description", product.Description);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Deletes a product from the database.
        /// </summary>
        /// <param name="productId">The ID of the product to be deleted.</param>
        private async Task DeleteProductAsync(int productId)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand("DELETE FROM Products WHERE ProductID = @ProductID", conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductID", productId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Validates the form fields before submitting the product form.
        /// Ensures all required fields are filled in and that numerical fields contain valid numbers.
        /// </summary>
        /// <returns>True if the form is valid; otherwise, false.</returns>
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text) || CategoryComboBox.SelectedIndex == -1 || string.IsNullOrWhiteSpace(QuantityTextBox.Text) || string.IsNullOrWhiteSpace(PriceTextBox.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (!int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                MessageBox.Show("Please enter a valid number for Quantity.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityTextBox.Focus();
            }
            else if (!decimal.TryParse(PriceTextBox.Text, out decimal price))
            {
                MessageBox.Show("Please enter a valid number for Price.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                PriceTextBox.Focus();
            }
            else if (!int.TryParse(MinQntTextBox.Text, out int minQuantity))
            {
                MessageBox.Show("Please enter a valid number for Min Quantity.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                MinQntTextBox.Focus();
            }
            return true;
        }

        /// <summary>
        /// Fills the category combo box with categories from the database.
        /// Ensures that the ComboBox displays all available categories for product selection.
        /// </summary>
        private async Task LoadCategoriesIntoComboBoxAsync()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand("SELECT Id, Name FROM Categories", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var categories = new List<Category>();
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new Category
                            {
                                CatID = reader.GetInt32(0), // Id
                                CategoryName = reader.GetString(1) // Name
                            });
                        }

                        // Setting the ComboBox's ItemsSource
                        CategoryComboBox.ItemsSource = categories;
                        CategoryComboBox.DisplayMemberPath = "CategoryName"; // Display the CategoryName in the ComboBox
                        CategoryComboBox.SelectedValuePath = "CatID"; // Store the CatID as the selected value
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading categories into the combo box.");
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the selection of a product from the ListView.
        /// Populates the form with the details of the selected product.
        /// </summary>
        /// <param name="sender">The object that triggered the event (the ListView).</param>
        /// <param name="e">The event arguments.</param>
        private void ProductListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected product from the ListView
            SelectedProduct = (Product)ProductListView.SelectedItem;

            // If a product is selected, populate the form fields
            if (SelectedProduct != null)
            {
                ProductNameTextBox.Text = SelectedProduct.Name;
                CategoryComboBox.SelectedItem = SelectedProduct.Category;
                QuantityTextBox.Text = SelectedProduct.Quantity.ToString();
                PriceTextBox.Text = SelectedProduct.Price.ToString();
                MinQntTextBox.Text = SelectedProduct.MinQuantity.ToString();
                SupplierTextBox.Text = SelectedProduct.Supplier;
                DescriptionTextBox.Text = SelectedProduct.Description;
            }
        }

        #endregion
    }
}
