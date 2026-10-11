using AlarmSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarmSystem.ViewModels
{
    public class CeaHistoryViewModel
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public string NwEng { get; set; } = "~ALL~";

        public List<SelectListItem> Engineers { get; set; } = new();

        public List<CeaHistoryAlarmModel> Alarms { get; set; } = new();
    }
}