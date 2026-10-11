namespace AlarmSystem.Models
{
    public class WorkGroupOrder
    {
        public string CLARITY_REF { get; set; } = "";

        public string CIRCUIT { get; set; } = "";

        public string CUSTOMER { get; set; } = "";

        public string REGION { get; set; } = "";

        public string PROVINCE { get; set; } = "";

        public string OPMC_NW { get; set; } = "";

        public string WO_STATUS { get; set; } = "";

        public int PENDING_HRS { get; set; }

        public string WORKGROUP { get; set; } = "";
    }
}