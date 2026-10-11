using AlarmSystem.Models;

namespace AlarmSystem.ViewModels
{
    public class WarRoomDashboardVM
    {
        public WarRoomDashboard Dashboard { get; set; }

        public int TotalFaults { get; set; }

        public int TotalCEA { get; set; }

        public int TotalMSAN { get; set; }

        public int TotalSLBN { get; set; }

        public int TotalDialog { get; set; }

        public int TotalMobitel { get; set; }

        public int TotalHutch { get; set; }

        public int TotalAirtel { get; set; }
    }
}