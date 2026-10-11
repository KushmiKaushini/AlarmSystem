namespace AlarmSystem.Models.Customer
{
    public class CustomerFaultSummaryDto
    {
        public int Total { get; set; }
        public int Today { get; set; }
        public int Last7Days { get; set; }
        public List<CustomerFaultItemDto> Recent { get; set; } = new();
    }

    public class CustomerFaultItemDto
    {
        public int Id { get; set; }
        public string? Fault { get; set; }
        public DateTime? FaultTime { get; set; }
        public string? Source { get; set; }   // optional: "CX"
    }
}