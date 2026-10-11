namespace AlarmSystem.Models
{
    public class MapNode
    {
        public string Name { get; set; } = string.Empty;

        public string NWEngineer { get; set; } = string.Empty;

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}