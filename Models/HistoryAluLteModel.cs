namespace AlarmSystem.Models
{
    public class HistoryAluLteModel
    {
        public string Node { get; set; }
        public string Fault { get; set; }
        public string AlarmType { get; set; }
        public DateTime FaultTime { get; set; }
        public DateTime? CloseTime { get; set; }
        public string NwEng { get; set; }
        public string Province { get; set; }
        public string Region { get; set; }
    }
}