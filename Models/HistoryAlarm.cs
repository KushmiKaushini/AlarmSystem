namespace AlarmSystem.Models
{
    public class HistoryAlarm
    {
        public string Node { get; set; }

        public DateTime? FaultTime { get; set; }

        public DateTime? CloseTime { get; set; }

        public string Platform { get; set; }

        public string Province { get; set; }
    }
}