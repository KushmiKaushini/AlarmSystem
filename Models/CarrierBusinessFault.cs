namespace AlarmSystem.Models
{
    public class CarrierBusinessFault
    {
        public string CustomerRef { get; set; }

        public string CustomerName { get; set; }

        public string CircuitId { get; set; }

        public DateTime? ReportedTime { get; set; }

        public string ReportedBy { get; set; }

        public string ReportedContact { get; set; }

        public string AccessMedium { get; set; }

        public string LEA { get; set; }

        public string BendAddress { get; set; }

        public string COF { get; set; }

        public string AllRemarks { get; set; }

        public string AddRemark { get; set; }
    }
}