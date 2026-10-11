namespace AlarmSystem.Models
{
    public class TicketAttribute
    {
        public long ID { get; set; }

        public long TICKET_ID { get; set; }

        public string KEY_FIELD { get; set; }

        public string VALUE_FIELD { get; set; }

        public DateTime? ENTER_DATE { get; set; }
    }
}