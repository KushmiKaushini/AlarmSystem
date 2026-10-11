namespace AlarmSystem.Models
{
    public class SmsInbox
    {
        public DateTime Time { get; set; }

        public string Sender { get; set; } = "";

        public string Message { get; set; } = "";

        public string Destination { get; set; } = "";

        public bool IsRecent { get; set; }
    }
}