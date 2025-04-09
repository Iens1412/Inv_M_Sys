namespace Inv_M_Sys.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string ReportTitle { get; set; }
        public string ReportType { get; set; }
        public string Details { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
    }
}