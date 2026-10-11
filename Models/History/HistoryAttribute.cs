namespace AlarmSystem.Models.History
{
    public class HistoryAttribute
    {
        public long ID { get; set; }

        public long TICKET_ID { get; set; }

        public string KEY_FIELD { get; set; }

        public string VALUE_FIELD { get; set; }
    }
}