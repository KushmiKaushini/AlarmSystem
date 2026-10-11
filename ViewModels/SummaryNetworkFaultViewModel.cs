using AlarmSystem.Models;

namespace AlarmSystem.ViewModels
{
    public class SummaryNetworkFaultViewModel
    {
        public NetworkSummaryModel Summary { get; set; } = new();

        public List<SummaryNetworkFaultModel> Details { get; set; } = new();
    }
}