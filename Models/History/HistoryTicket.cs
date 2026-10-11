using System;

namespace AlarmSystem.Models.History
{
    public class HistoryTicket
    {
        public long ID { get; set; }

        public string NODE { get; set; }

        public string SEVERITY { get; set; }

        public string TAG { get; set; }

        public string PLATFORM { get; set; }

        public string DESCRIPTION { get; set; }

        public DateTime? ISSUE_TIME { get; set; }

        public DateTime? REPORTING_TIME { get; set; }

        public string STATUS { get; set; }

        public string RESPONSIBLE_OFFICER { get; set; }

        public string TICKET_TYPE { get; set; }

        public string WORKGROUP { get; set; }

        public string CREATION_METHOD { get; set; }

        public string REPORTING_PLATFORM { get; set; }

        public string REPORTING_BY { get; set; }

        public DateTime? ACK_TIME { get; set; }

        public string ACK_BY { get; set; }

        public DateTime? CLEAR_TIME { get; set; }

        public string CLEAR_BY { get; set; }

        public DateTime? CLOSE_TIME { get; set; }

        public string CLOSE_BY { get; set; }
    }
}