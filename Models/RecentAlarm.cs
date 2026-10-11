namespace AlarmSystem.Models
{
    public class RecentAlarm
    {
        public string Node { get; set; } = string.Empty;

        public string Fault { get; set; } = string.Empty;

        public string AlarmType { get; set; } = string.Empty;

        public DateTime FaultTime { get; set; }

        public string FaultStatus { get; set; } = string.Empty;
    }
}