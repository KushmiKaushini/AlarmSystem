namespace AlarmSystem.Models.ViewModels
{
    public class AlarmDashboardViewModel
    {
        public int TotalPlatforms { get; set; }

        public int TotalEngineers { get; set; }

        public int TotalFaults { get; set; }

        public int OpenFaults { get; set; }
    }
}