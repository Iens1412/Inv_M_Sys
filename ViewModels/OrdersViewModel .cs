using Inv_M_Sys.Models;
using Inv_M_Sys.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics;

namespace Inv_M_Sys.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private ObservableCollection<OrderItem> _orderBasket;
        private Product _selectedProduct;
        private int _quantity;

        private Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        private DateTime? _deliveryDate;
        public DateTime? DeliveryDate
        {
            get => _deliveryDate;
            set => SetProperty(ref _deliveryDate, value);
        }

        public ObservableCollection<OrderItem> OrderBasket
        {
            get => _orderBasket;
            set => SetProperty(ref _orderBasket, value);
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public ICommand AddToBasketCommand { get; }
        public ICommand RemoveFromBasketCommand { get; }
        public ICommand ClearBasketCommand { get; }

        public OrdersViewModel()
        {
            OrderBasket = new ObservableCollection<OrderItem>();
            AddToBasketCommand = new RelayCommand<object>(AddToBasket, CanAddToBasket);
            RemoveFromBasketCommand = new RelayCommand<OrderItem>(RemoveFromBasket);
            ClearBasketCommand = new RelayCommand(ClearBasket);
        }

        //clear basket
        private void ClearBasket()
        {
            OrderBasket.Clear();
        }

        /// <summary>
        /// Adds the selected product and quantity to the order basket if valid.
        /// </summary>
        private void AddToBasket(object parameter)
        {
            if (SelectedProduct != null && Quantity > 0)
            {
                int alreadyInBasket = OrderBasket
                    .Where(x => x.Product.Id == SelectedProduct.Id)
                    .Sum(x => x.Quantity);

                int totalRequested = alreadyInBasket + Quantity;

                if (totalRequested > SelectedProduct.Quantity)
                {
                    System.Windows.MessageBox.Show(
                        $"Not enough stock for {SelectedProduct.Name}. Available: {SelectedProduct.Quantity}, Already in basket: {alreadyInBasket}, Requested: {Quantity}.",
                        "Insufficient Stock",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning
                    );
                    return;
                }

                var existingItem = OrderBasket.FirstOrDefault(x => x.Product.Id == SelectedProduct.Id);

                if (existingItem != null)
                {
                    // ✅ Update existing item
                    existingItem.Quantity += Quantity;
                    existingItem.TotalPrice = existingItem.Quantity * (SelectedProduct.Price ?? 0);

                    // Notify UI that item has changed
                    OnPropertyChanged(nameof(OrderBasket));
                }
                else
                {
                    // ✅ Add new item
                    var orderItem = new OrderItem(SelectedProduct, Quantity)
                    {
                        TotalPrice = (SelectedProduct.Price ?? 0) * Quantity,
                    };

                    OrderBasket.Add(orderItem);
                }
            }
        }

        /// <summary>
        /// Removes the selected item from the basket.
        /// </summary>
        private void RemoveFromBasket(OrderItem orderItem)
        {
            if (orderItem != null)
            {
                OrderBasket.Remove(orderItem); // Remove the selected item from the basket
            }
        }

        /// <summary>
        /// Determines if a product can be added to the basket.
        /// </summary>
        private bool CanAddToBasket(object parameter)
        {
            bool canExecute = SelectedProduct != null && Quantity > 0;
            Debug.WriteLine($"CanAddToBasket called. SelectedProduct: {SelectedProduct}, Quantity: {Quantity}, CanExecute: {canExecute}");
            return canExecute;
        }

        /// <summary>
        /// Determines if a basket item can be removed.
        /// </summary>
        private bool CanRemoveFromBasket(OrderItem orderItem)
        {
            return orderItem != null;
        }
    }
}