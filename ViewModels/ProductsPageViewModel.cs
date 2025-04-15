using Inv_M_Sys.Commands;
using Inv_M_Sys.Helpers;
using Inv_M_Sys.Models;
using Inv_M_Sys.Services.Pages_Services;
using Serilog;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Inv_M_Sys.ViewModels
{
    public class ProductsPageViewModel : BaseViewModel
    {
        public ObservableCollection<Product> ProductsList { get; set; } = new();
        private List<Product> _allProductsCache = new(); // Cache for search refresh

        public ObservableCollection<Category> Categories { get; set; } = new();

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        private bool _isFormVisible = false;
        public bool IsFormVisible
        {
            get => _isFormVisible;
            set => SetProperty(ref _isFormVisible, value);
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        private bool _isNewMode;
        public bool IsNewMode
        {
            get => _isNewMode;
            set => SetProperty(ref _isNewMode, value);
        }


        public ICommand AddProductCommand { get; }
        public ICommand UpdateProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }

        public ICommand NewProductFormCommand { get; }
        public ICommand EditProductFormCommand { get; }
        public ICommand BackCommand { get; }

        public ProductsPageViewModel()
        {
            AddProductCommand = new RelayCommand(async () => await AddProductAsync());
            UpdateProductCommand = new RelayCommand(async () => await UpdateProductAsync());
            DeleteProductCommand = new RelayCommand(async () => await DeleteProductAsync());
            RefreshCommand = new RelayCommand(async () => await LoadProductsAsync());
            SearchCommand = new RelayCommand(SearchProducts);

            NewProductFormCommand = new RelayCommand(ShowNewProductForm);
            EditProductFormCommand = new RelayCommand(ShowEditProductForm);
            BackCommand = new RelayCommand(HideProductForm);

            _ = LoadProductsAsync();
            _ = LoadCategoriesAsync();
        }

        public async Task LoadProductsAsync()
        {
            try
            {
                var products = await ProductService.GetAllProductsAsync();
                _allProductsCache = products; // Update cache
                ProductsList = new ObservableCollection<Product>(products);
                OnPropertyChanged(nameof(ProductsList));
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "Failed to load products.");
                MessageBox.Show("Failed to load products.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await ProductService.GetAllCategoriesAsync();
                Categories = new ObservableCollection<Category>(categories);
                OnPropertyChanged(nameof(Categories));

                if (Categories.Count == 0)
                {
                    MessageBox.Show("No categories found. Please add categories before managing products.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "Failed to load categories.");
                MessageBox.Show("Failed to load categories.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowNewProductForm()
        {
            SelectedProduct = new Product();
            IsNewMode = true;
            IsEditMode = false;
            Messenger.Default.Send("ShowForm");
        }

        private void ShowEditProductForm()
        {
            if (SelectedProduct != null)
            {
                IsNewMode = false;
                IsEditMode = true;
                Messenger.Default.Send("ShowForm");
            }
            else
            {
                MessageBox.Show("Please select a product to edit.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void HideProductForm()
        {
            SelectedProduct = null;
            IsNewMode = false;
            IsEditMode = false;
            Messenger.Default.Send("HideForm");
        }

        private async Task AddProductAsync()
        {
            SelectedProduct ??= new Product();

            if (!ValidateProduct(SelectedProduct)) return;

            try
            {
                await ProductService.AddProductAsync(SelectedProduct);
                await LoadProductsAsync();
                MessageBox.Show("Product added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                HideProductForm();
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "Error adding product.");
                MessageBox.Show("Error adding product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateProductAsync()
        {
            if (!ValidateProduct(SelectedProduct)) return;

            try
            {
                await ProductService.UpdateProductAsync(SelectedProduct);
                await LoadProductsAsync();
                MessageBox.Show("Product updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                HideProductForm();
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "Error updating product.");
                MessageBox.Show("Error updating product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Please select a product to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Delete product '{SelectedProduct.Name}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                await ProductService.DeleteProductAsync(SelectedProduct.Id);
                await LoadProductsAsync();
                MessageBox.Show("Product deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "Error deleting product.");
                MessageBox.Show("Error deleting product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchProducts()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                ProductsList = new ObservableCollection<Product>(_allProductsCache);
                OnPropertyChanged(nameof(ProductsList));
                return;
            }

            var filtered = _allProductsCache
                .Where(p =>
                    p.Name.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase) ||
                    p.CategoryName.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase) ||
                    p.Supplier.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase) ||
                    p.Id.ToString().Contains(SearchQuery))
                .ToList();

            ProductsList = new ObservableCollection<Product>(filtered);
            OnPropertyChanged(nameof(ProductsList));
        }

        private bool ValidateProduct(Product product)
        {
            if (product == null ||
                string.IsNullOrWhiteSpace(product.Name) ||
                product.CategoryId <= 0 ||
                product.Quantity == null || product.Quantity < 0 ||
                product.Price == null || product.Price <= 0 ||
                product.MinQuantity == null || product.MinQuantity < 0)
            {
                MessageBox.Show("Please fill all required fields correctly.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
