namespace AlarmSystem.Models
{
    public class AlarmHistoryRegion
    {
        public string Node { get; set; }

        public string Alarm { get; set; }

        public DateTime? AlarmTime { get; set; }

        public string Platform { get; set; }

        public string Vendor { get; set; }

        public DateTime? CloseTime { get; set; }

        public string Region { get; set; }

        public string Province { get; set; }

        public string NetworkEngineer { get; set; }

        public string AlarmType { get; set; }
    }
}