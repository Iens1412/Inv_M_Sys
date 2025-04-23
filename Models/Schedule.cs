namespace Inv_M_Sys.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public double TotalHours { get; set; }
    }
}