namespace AlarmSystem.Models
{
    public class FaultComment
    {
        public int DOCKET_ID { get; set; }

        public string DESCRIPTION { get; set; }

        public DateTime? EVENT_TIME { get; set; }
    }
}