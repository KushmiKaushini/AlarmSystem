namespace AlarmSystem.Models
{
    public class ACMainFailureModel
    {
        public string NODE { get; set; } = "";

        public string ALARM_TYPE { get; set; } = "";

        public string PLATFORM { get; set; } = "";

        public DateTime? FAULT_TIME { get; set; }

        public DateTime? CLOSE_TIME { get; set; }

        public string REGION { get; set; } = "";

        public string PROVINCE { get; set; } = "";

        public string NW_ENG { get; set; } = "";

        public string RTOM { get; set; } = "";

        public double DURATION { get; set; }
    }
}