namespace AlarmSystem.Models
{
    public class PowerAlarmModel
    {
        public string? Id { get; set; }

        public string? Node { get; set; }

        public string? Alarm { get; set; }

        public DateTime FaultTime { get; set; }

        public string? AlarmType { get; set; }

        public string? Ack { get; set; }
    }
}