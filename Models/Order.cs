namespace Inv_M_Sys.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public string Notes { get; set; }

        // Optional: For AwaitingPage if you want to display items
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}