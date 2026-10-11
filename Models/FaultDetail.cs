namespace AlarmSystem.Models
{
    public class FaultDetail
    {
        public int ProblemNo { get; set; }

        public string CustomerName { get; set; }

        public string CircuitName { get; set; }

        public string CircuitType { get; set; }

        public string Description { get; set; }

        public string Nature { get; set; }

        public DateTime? ReportedTime { get; set; }

        public string Priority { get; set; }

        public string SLAType { get; set; }

        public string SLATime { get; set; }

        public string WorkGroup { get; set; }

        public string CustomerType { get; set; }

        public string EnterBy { get; set; }

        public string Status { get; set; }

        public string ReportedBy { get; set; }

        public string ReportedContact { get; set; }

        // Escalation
        public string Node { get; set; }

        public string FaultName { get; set; }

        public DateTime? FaultTime { get; set; }
    }
}