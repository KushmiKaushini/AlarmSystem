using AlarmSystem.Models;

namespace AlarmSystem.ViewModels
{
    public class ACMainFailureViewModel
    {
        public DateTime From { get; set; } = DateTime.Today;

        public DateTime To { get; set; } = DateTime.Today;

        public string SelectedPlatform { get; set; } = "~ALL~";

        public string SelectedEngineer { get; set; } = "~ALL~";

        public List<PlatformModel> Platforms { get; set; } = new();

        public List<NWEngineerModel> Engineers { get; set; } = new();

        public List<ACMainFailureModel> Reports { get; set; } = new();
    }
}