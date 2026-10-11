using AlarmSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarmSystem.ViewModels
{
    public class OutageReasonViewModel
    {
        public string SelectedPlatform { get; set; } = "~ALL~";

        public string SelectedNwEng { get; set; } = "~ALL~";

        public List<SelectListItem> Platforms { get; set; } = new();

        public List<SelectListItem> NetworkEngineers { get; set; } = new();

        public List<OutageReasonModel> Alarms { get; set; } = new();
    }
}