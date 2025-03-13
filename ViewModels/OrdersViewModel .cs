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

        public OrdersViewModel()
        {
            _orderBasket = new ObservableCollection<OrderItem>();
            AddToBasketCommand = new RelayCommand<object>(AddToBasket, CanAddToBasket);
            RemoveFromBasketCommand = new RelayCommand<OrderItem>(RemoveFromBasket, CanRemoveFromBasket);
        }

        private void AddToBasket(object parameter)
        {
            if (SelectedProduct != null && Quantity > 0)
            {
                var orderItem = new OrderItem(SelectedProduct, Quantity)
                {
                    TotalPrice = SelectedProduct.Price * Quantity
                };

                OrderBasket.Add(orderItem);
            }
        }

        private void RemoveFromBasket(OrderItem orderItem)
        {
            if (orderItem != null)
            {
                OrderBasket.Remove(orderItem); // Remove the selected item from the basket
            }
        }

        private bool CanAddToBasket(object parameter)
        {
            bool canExecute = SelectedProduct != null && Quantity > 0;
            Debug.WriteLine($"CanAddToBasket called. SelectedProduct: {SelectedProduct}, Quantity: {Quantity}, CanExecute: {canExecute}");
            return canExecute;
        }

        private bool CanRemoveFromBasket(OrderItem orderItem)
        {
            return orderItem != null;
        }
    }
}