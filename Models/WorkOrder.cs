namespace AlarmSystem.Models
{
    public class WorkOrder
    {
        public string CLARITY_REF { get; set; } = "";

        public string CIRCUIT { get; set; } = "";

        public string CUSTOMER { get; set; } = "";

        public string WORK_ORDER { get; set; } = "";

        public string WO_STATUS { get; set; } = "";

        public int PENDING_HRS { get; set; }

        public string WORKGROUP { get; set; } = "";
    }
}