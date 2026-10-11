using AlarmSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarmSystem.ViewModels
{
    public class MsanHistoryViewModel
    {
        public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-4);

        public DateTime ToDate { get; set; } = DateTime.Today;

        public string SelectedEngineer { get; set; } = "ALL";

        public List<SelectListItem> Engineers { get; set; } = new();

        public List<MsanHistoryAlarmModel> Alarms { get; set; } = new();
    }
}