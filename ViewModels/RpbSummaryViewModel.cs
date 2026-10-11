using AlarmSystem.Models;

namespace AlarmSystem.Models.ViewModels
{
    public class RpbSummaryViewModel
    {
        public int TotalDistributed { get; set; }

        public int TotalInstalled { get; set; }

        public List<RpbSummary> Summary { get; set; }
            = new();

        public List<RpbLocation> Locations { get; set; }
            = new();
    }
}