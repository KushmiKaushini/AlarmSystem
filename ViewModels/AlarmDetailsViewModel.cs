using AlarmSystem.Models.Alarm;

namespace AlarmSystem.ViewModels
{
    public class AlarmDetailsViewModel
    {
        public AlarmModel Alarm { get; set; }

        public int TicketId { get; set; }

        public List<AutoEscalationModel> AutoEscalations { get; set; } = new();

        public List<SmsEscalationModel> SmsEscalations { get; set; } = new();
    }
}