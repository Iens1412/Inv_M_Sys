namespace Inv_M_Sys.Models
{
    public class OrderItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }

        public OrderItem(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
            TotalPrice = product.Price * quantity;
        }

        public OrderItem(string productName, decimal price, int quantity)
        {
            Product = new Product { Name = productName, Price = price };
            Quantity = quantity;
            TotalPrice = price * quantity;
        }
    }
}