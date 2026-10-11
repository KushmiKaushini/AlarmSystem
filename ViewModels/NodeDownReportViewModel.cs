using AlarmSystem.Models;

namespace AlarmSystem.ViewModels
{
    public class NodeDownReportViewModel
    {
        public DateTime From { get; set; } = DateTime.Today;

        public DateTime To { get; set; } = DateTime.Today;

        public string SelectedEngineer { get; set; } = "~ALL~";

        public List<NetworkEngineerModel> Engineers { get; set; } = new();

        public List<NodeDownReportModel> Reports { get; set; } = new();
    }
}