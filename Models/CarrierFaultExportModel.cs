namespace AlarmSystem.Models
{
    public class CarrierFaultExportModel
    {
        public string problem_no { get; set; }

        public string cr { get; set; }

        public string customer_name { get; set; }

        public string circuit_name { get; set; }

        public string circuit_type { get; set; }

        public string description { get; set; }

        public string nature { get; set; }

        public DateTime? reportedtime { get; set; }

        public string priority { get; set; }

        public string SLA_type { get; set; }

        public string SLA_time { get; set; }

        public string workgroup { get; set; }

        public string customer_type { get; set; }

        public string enter_by { get; set; }

        public string problem_status { get; set; }

        public string esc_level { get; set; }

        public string account_number { get; set; }

        public string reported_by { get; set; }

        public string reported_contact { get; set; }

        public string access_medium { get; set; }

        public string CR1 { get; set; }

        public string CCT { get; set; }

        public string LEA { get; set; }

        public string CUSR_NAME { get; set; }

        public string BENDADDRESS { get; set; }

        public string COF { get; set; }
    }
}