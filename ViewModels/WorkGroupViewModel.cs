using AlarmSystem.Models;

namespace AlarmSystem.ViewModels
{
    public class WorkGroupViewModel
    {
        public string Area { get; set; } = "All";

        public string Region { get; set; } = "All";

        public string Province { get; set; } = "All";

        public string WorkGroup { get; set; } = "All";

        public List<string> Areas { get; set; } = new();

        public List<string> Regions { get; set; } = new();

        public List<string> Provinces { get; set; } = new();

        public List<string> WorkGroups { get; set; } = new();

        public List<WorkGroupOrder> Orders { get; set; } = new();

        public List<WorkGroupOrder> AllOrders { get; set; } = new();
    }
}