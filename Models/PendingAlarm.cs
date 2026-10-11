namespace AlarmSystem.Models
{
    public class PendingAlarm
    {
        public string Node { get; set; }

        public DateTime? FaultTime { get; set; }

        public string Platform { get; set; }

        public string Province { get; set; }
    }
}