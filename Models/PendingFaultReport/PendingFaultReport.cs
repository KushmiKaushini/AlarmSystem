namespace AlarmSystem.Models
{
    public class PendingFaultReport
    {
        public long No { get; set; }
        public string DataCircuitId { get; set; } = string.Empty;
        public string DetailsOfCustomer { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string WorkOrdersHeldUpAt { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DurationOfComplaint { get; set; } = string.Empty;
        public string ReportedDate { get; set; } = string.Empty;
        public string CircuitName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string OutageTime { get; set; } = string.Empty;
        public string Lea { get; set; } = string.Empty;
        public string RelevantOpmc { get; set; } = string.Empty;
        public string Dgm { get; set; } = string.Empty;
        public string Gm { get; set; } = string.Empty;
        public string RecentUpdate { get; set; } = string.Empty;
    }
}