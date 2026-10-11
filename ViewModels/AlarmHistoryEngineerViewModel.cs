using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarmSystem.Models.ViewModels
{
    public class AlarmHistoryEngineerViewModel
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public string Platform { get; set; }

        public string NetworkEngineer { get; set; }

        public List<SelectListItem> Platforms { get; set; }
            = new();

        public List<SelectListItem> Engineers { get; set; }
            = new();

        public List<AlarmHistoryEngineer> Records { get; set; }
            = new();
    }
}