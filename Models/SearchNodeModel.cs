namespace AlarmSystem.Models
{
    public class SearchNodeModel
    {
        public string Node { get; set; }

        public string Reason { get; set; }

        public string AlarmStatus { get; set; }

        public string Platform { get; set; }

        public DateTime AlarmTime { get; set; }
    }
}