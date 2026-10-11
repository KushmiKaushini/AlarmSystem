using System.Collections.Generic;

namespace AlarmSystem.Models.ViewModels
{
    public class SummaryViewModel
    {
        public List<Summary> PendingSummary { get; set; }
            = new();

        public List<PendingAlarm> PendingAlarms { get; set; }
            = new();

        public List<Summary> HistorySummary { get; set; }
            = new();

        public List<HistoryAlarm> HistoryAlarms { get; set; }
            = new();
    }
}