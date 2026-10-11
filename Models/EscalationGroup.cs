namespace AlarmSystem.Models
{
    public class EscalationGroup
    {
        public long ID { get; set; }

        public string GROUP_NAME { get; set; }

        public int PARENT_ID { get; set; }

        public string PROFILE { get; set; }

        public string ESCALATION_LEVEL { get; set; }
    }
}