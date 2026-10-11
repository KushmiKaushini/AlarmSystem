using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarmSystem.Models.ViewModels
{
    public class ReportCeaPortDownViewModel
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public string Customer { get; set; } = "~ALL~";

        public List<SelectListItem> Customers { get; set; }
            = new();

        public List<ReportCeaPortDown> Records { get; set; }
            = new();
    }
}