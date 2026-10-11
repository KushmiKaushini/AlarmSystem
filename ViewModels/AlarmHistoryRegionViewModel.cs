using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarmSystem.Models.ViewModels
{
    public class AlarmHistoryRegionViewModel
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public string Platform { get; set; }

        public string Region { get; set; }

        public List<SelectListItem> Platforms { get; set; }
            = new();

        public List<SelectListItem> Regions { get; set; }
            = new();

        public List<AlarmHistoryRegion> Records { get; set; }
            = new();
    }
}