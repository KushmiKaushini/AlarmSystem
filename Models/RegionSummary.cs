namespace AlarmSystem.Models
{
    public class RegionSummary
    {
        public string REGION { get; set; } = string.Empty;

        public string PROVINCE { get; set; } = string.Empty;

        public int CEA { get; set; }

        public int MSAN { get; set; }

        public int SLBN { get; set; }
    }
}