using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarmSystem.Models.ViewModels
{
    public class AlarmHistoryProvinceViewModel
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public string Platform { get; set; } = "";

        public string Province { get; set; } = "";

        public List<SelectListItem> Platforms { get; set; }
            = new();

        public List<SelectListItem> Provinces { get; set; }
            = new();

        public List<AlarmHistoryProvince> Records { get; set; }
            = new();
    }
}