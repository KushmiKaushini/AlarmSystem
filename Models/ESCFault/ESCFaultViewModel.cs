using System;
using System.Collections.Generic;

namespace AlarmSystem.Models.EscFault
{
    public class EscFaultViewModel
    {
        // Ticket Details
        public int ID { get; set; }

        public string TICKET_TYPE { get; set; }

        public string NODE { get; set; }

        public string DESCRIPTION { get; set; }

        public DateTime ISSUE_TIME { get; set; }

        public DateTime? CLEAR_TIME { get; set; }

        public string SEVERITY { get; set; }

        public string TAG { get; set; }

        public string PLATFORM { get; set; }

        public string WORKGROUP { get; set; }

        public string STATUS { get; set; }

        // Reporting
        public string RESPONSIBLE_OFFICER { get; set; }

        public string CREATION_METHOD { get; set; }

        public string REPORTING_PLATFORM { get; set; }

        public string REPORTING_BY { get; set; }

        public DateTime REPORTING_TIME { get; set; }

        public DateTime? ACK_TIME { get; set; }

        public string ACK_BY { get; set; }

        public string CLEAR_BY { get; set; }

        public DateTime? CLOSE_TIME { get; set; }

        public string CLOSE_BY { get; set; }

        // Child Collections

        public List<TicketAttributeModel> Attributes { get; set; }
            = new();

        public List<CommentModel> Comments { get; set; }
            = new();

        public List<EscalationModel> Escalations { get; set; }
            = new();
    }

    public class TicketAttributeModel
    {
        public int ID { get; set; }

        public string KEY_FIELD { get; set; }

        public string VALUE_FIELD { get; set; }
    }

    public class CommentModel
    {
        public string COMMENT_TEXT { get; set; }

        public DateTime COMMENT_TIME { get; set; }

        public string COMMENT_BY { get; set; }
    }

    public class EscalationModel
    {
        public int ID { get; set; }

        public string GROUP_NAME { get; set; }

        public DateTime ESCALATION_TIME { get; set; }

        public string ESCALATION_LEVEL { get; set; }
    }
}