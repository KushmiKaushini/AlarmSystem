namespace AlarmSystem.Models
{
    public class AlarmDelayModel
    {
        public string PLATFORM { get; set; }

        public string ALARM { get; set; }

        public int DELAY_MINUTES { get; set; }

        public string ALARM_TYPE { get; set; }
    }
}