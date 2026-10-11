using System;

namespace AlarmSystem.Models.History
{
    public class HistoryEscalationPrimary
    {
        public long ID { get; set; }

        public long TICKET_ID { get; set; }

        public string GROUP_NAME { get; set; }

        public DateTime ESCALATION_TIME { get; set; }

        public string ESCALATE_BY_SVC { get; set; }

        public string ESCALATE_BY_NAME { get; set; }

        public string ESCALATION_LEVEL { get; set; }
    }
}