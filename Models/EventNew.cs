namespace AlarmSystem.Models
{
    public class EventNew
    {
        public string REF { get; set; }

        public DateTime EVENT_TIME { get; set; }

        public string CIRCUIT { get; set; }

        public string CUSTOMER { get; set; }

        public string DESCRIPTION { get; set; }
    }
}