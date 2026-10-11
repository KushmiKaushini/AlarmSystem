namespace AlarmSystem.Models
{
    public class CurrentMsanOutage
    {
        public string PLATFORM { get; set; }

        public DateTime FAULT_TIME { get; set; }

        public string NW_ENG { get; set; }

        public string PROVINCE { get; set; }

        public string REGION { get; set; }

        public string DATE { get; set; }

        public string NODE { get; set; }

        public string FAULT_OCCURED_AT { get; set; }

        public string PENDING_HOURS { get; set; }
    }
}