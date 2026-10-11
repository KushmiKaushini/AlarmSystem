using System;

namespace AlarmSystem.Models
{
    public class TroubleTicket
    {
        public long ID { get; set; }

        public string NODE { get; set; }

        public string SEVERITY { get; set; }

        public string TICKET_TYPE { get; set; }

        public string TAG { get; set; }

        public string PLATFORM { get; set; }

        public string WORKGROUP { get; set; }

        public string DESCRIPTION { get; set; }

        public string CREATION_METHOD { get; set; }

        public DateTime? ISSUE_TIME { get; set; }

        public DateTime? REPORTING_TIME { get; set; }

        public string REPORTING_BY { get; set; }

        public DateTime? REPORTED_TIME { get; set; }

        public string ASSIGNED_TO { get; set; }

        public DateTime? ASSIGNED_TIME { get; set; }

        public DateTime? CLOSED_TIME { get; set; }

        public string STATUS { get; set; }
    }
}