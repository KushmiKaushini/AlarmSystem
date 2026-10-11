namespace AlarmSystem.Models
{
    public class MsanBatteryBackup
    {
        public string NODE { get; set; }
        public string Region { get; set; }
        public string Province { get; set; }
        public string LEA { get; set; }
        public string NwEng { get; set; }
        public string NwEngName { get; set; }
        public int HOURS { get; set; }
        public int MINUTES { get; set; }
        public int CustomerCount { get; set; }
    }
}