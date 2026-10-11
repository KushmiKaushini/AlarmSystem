namespace AlarmSystem.Models
{
    public class OutageReasonModel
    {
        public int ID { get; set; }

        public string NODE { get; set; }

        public string PLATFORM { get; set; }

        public DateTime FAULT_TIME { get; set; }

        public int HOURS { get; set; }

        public string FAULT { get; set; }

        public string NW_ENG { get; set; }

        public string TAG { get; set; }
    }
}