namespace AlarmSystem.Models
{
    public class AlarmHistoryEngineer
    {
        public int Id { get; set; }

        public string Node { get; set; }

        public string Alarm { get; set; }

        public string AlarmType { get; set; }

        public string Platform { get; set; }

        public string Vendor { get; set; }

        public string Status { get; set; }

        public DateTime? AlarmTime { get; set; }

        public DateTime? CloseTime { get; set; }

        public string NetworkEngineer { get; set; }

        public string Region { get; set; }

        public string Province { get; set; }

        public string RTOM { get; set; }

        public string LeaCode { get; set; }

        public string LeaName { get; set; }

        public string Position { get; set; }
    }
}