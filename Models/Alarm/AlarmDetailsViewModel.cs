using AlarmSystem.Models.Alarm;

namespace AlarmSystem.Models.Alarm
{
    public class AlarmDetailsViewModel
    {
        public AlarmModel Alarm { get; set; } = new();

         public int TicketId { get; set; }

        public List<AutoEscalationModel> AutoEscalations { get; set; } = new();

        public List<SmsEscalationModel> SmsEscalations { get; set; } = new();
    }

    public class AutoEscalationModel
    {
        public int ESCALATION_LEVEL { get; set; }

        public string CLI { get; set; } = "";

        public string MESSAGE { get; set; } = "";
    }

    public class SmsEscalationModel
    {
        public int ESCALATION_LEVEL { get; set; }

        public string MOBILE { get; set; } = "";

        public string NAME { get; set; } = "";

        public string DESIGNATION { get; set; } = "";

        public DateTime? SCHEDULE_TIME { get; set; }

        public string SEND_STATUS { get; set; } = "";

        public DateTime? SENT_TIME { get; set; }

        public string MESSAGE { get; set; } = "";

        public int TicketId { get; set; }
    }
}