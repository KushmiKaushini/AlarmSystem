namespace AlarmSystem.Models
{
    public class Ticket
    {
        public long ID { get; set; }

        public string NODE { get; set; } = string.Empty;

        public string SEVERITY { get; set; } = string.Empty;

        public string TICKET_TYPE { get; set; } = string.Empty;

        public string TAG { get; set; } = string.Empty;

        public string PLATFORM { get; set; } = string.Empty;

        public string WORKGROUP { get; set; } = string.Empty;

        public string DESCRIPTION { get; set; } = string.Empty;

        public string CREATION_METHOD { get; set; } = string.Empty;

        public DateTime? ISSUE_TIME { get; set; }

        public DateTime? END_TIME { get; set; }

        public DateTime? REPORTING_TIME { get; set; }

        public string? REPORTING_BY { get; set; }

        public string? REPORTING_PLATFORM { get; set; }

        public string STATUS { get; set; } = string.Empty;

        public string? RESPONSIBLE_OFFICER { get; set; }

        public string? MESSAGE_TEXT { get; set; }

        public DateTime? CLEAR_TIME { get; set; }

        public DateTime? ACK_TIME { get; set; }

        public string? ACK_BY { get; set; }

        public DateTime? CLOSE_TIME { get; set; }

        public string? CLOSE_BY { get; set; }
    }
}