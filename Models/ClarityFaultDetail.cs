namespace AlarmSystem.Models
{
    public class ClarityFaultDetail
    {
        public int CLARITY_REF { get; set; }

        public string CUSTOMER { get; set; } = "";

        public string CIRCUIT { get; set; } = "";

        public string CIRCUIT_TYPE { get; set; } = "";

        public string DESCRIPTION { get; set; } = "";

        public string NATURE { get; set; } = "";

        public DateTime? REPORTED_TIME { get; set; }

        public string PRIORITY { get; set; } = "";

        public string SLA_TYPE { get; set; } = "";

        public string SLA_TIME { get; set; } = "";

        public string WORKGROUP { get; set; } = "";

        public string CUSTOMER_TYPE { get; set; } = "";

        public string ENTER_BY { get; set; } = "";

        public string STATUS { get; set; } = "";

        public string REPORTED_BY { get; set; } = "";

        public string REPORTED_CONTACT { get; set; } = "";
    }
}