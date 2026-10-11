using System;

namespace AlarmSystem.Models.History
{
    public class HistoryComment
    {
        public long ID { get; set; }

        public long TICKET_REF { get; set; }

        public string COMMENT_TEXT { get; set; }

        public DateTime? COMMENT_TIME { get; set; }

        public string COMMENT_BY { get; set; }
    }
}