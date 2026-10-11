using AlarmSystem.Models;

namespace AlarmSystem.ViewModels
{
    public class WarRoomDashboardViewModel
    {
        public List<PlatformSummary> DisasterSummary { get; set; } = new();

        public List<RegionSummary> RegionSummary { get; set; } = new();

        public List<OperatorSummary> OperatorSummary { get; set; } = new();

        public List<OperatorRegionSummary> OperatorRegionSummary { get; set; } = new();
    }
}
