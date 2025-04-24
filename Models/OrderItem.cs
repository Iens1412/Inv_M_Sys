using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Inv_M_Sys.Models
{
    public class OrderItem : INotifyPropertyChanged
    {
        public Product Product { get; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice)); // Trigger UI to update TotalPrice too
                }
            }
        }

        public decimal TotalPrice => (Product?.Price ?? 0) * Quantity;

        public OrderStatus Status { get; set; }

        public OrderItem(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }

        public OrderItem(string productName, decimal price, int quantity)
        {
            Product = new Product { Name = productName, Price = price };
            Quantity = quantity;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
