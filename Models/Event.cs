namespace AlarmSystem.Models
{
    public class Event
    {
        public string REF { get; set; } = "";

        public string CIRCUIT { get; set; } = "";

        public string CUSTOMER { get; set; } = "";

        public DateTime DATE_TIME { get; set; }

        public string COMMENT { get; set; } = "";

        public bool IsRecent { get; set; }
    }
}