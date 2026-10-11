namespace AlarmSystem.Models
{
    public class EscalationSecondary
    {
        public long ID { get; set; }

        public long ESC_ID { get; set; }

        public string SVC { get; set; }

        public string NAME { get; set; }

        public string MOBILE { get; set; }

        public string MESSAGE { get; set; }

        public string CLI { get; set; }

        public string SEND_STATUS { get; set; }

        public DateTime? SENT_TIME { get; set; }

        public DateTime? SCHEDULE_TIME { get; set; }
    }
}