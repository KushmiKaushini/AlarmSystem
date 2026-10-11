namespace AlarmSystem.Models
{
    public class EscalationPrimary
    {
        public long ID { get; set; }

        public long TICKET_ID { get; set; }

        public string GROUP_NAME { get; set; }

        public DateTime ESCALATION_TIME { get; set; }

        public string ESCALATE_BY_SVC { get; set; }

        public string ESCALATE_BY_NAME { get; set; }

        public string ESCALATION_LEVEL { get; set; }

        public string TICKET_TYPE { get; set; }

        public string PLATFORM { get; set; }
    }
}