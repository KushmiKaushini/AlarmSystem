namespace AlarmSystem.Models
{
    public class ClarityWorkOrder
    {
        public string WO_REF { get; set; } = "";

        public string WO_STATUS { get; set; } = "";

        public DateTime? STARTED_TIME { get; set; }

        public string WORK_GROUP { get; set; } = "";
    }
}