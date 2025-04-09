namespace Inv_M_Sys.Models
{
    public class ReportDetail
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }
}