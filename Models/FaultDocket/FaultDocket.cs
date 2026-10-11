namespace AlarmSystem.Models
{
    public class FaultDocket
    {
        public string CLARITY_REF { get; set; } = "";

        public string CUS_TYPE { get; set; } = "";

        public string CUS_NAME { get; set; } = "";

        public string CIR_NAME { get; set; } = "";

        public string CIR_TYPE { get; set; } = "";

        public DateTime? REPORTED { get; set; }

        public int HOURS { get; set; }

        public string SLA { get; set; } = "";

        public string ACCESS_MEDIUM { get; set; } = "";

        public int WO_COUNT { get; set; }

        public string WORK_GROUP { get; set; } = "";

        // Used for row highlighting (same as old TMS)
        public bool SLAExceeded { get; set; }
    }
}