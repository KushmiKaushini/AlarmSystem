namespace AlarmSystem.Models
{
    public class RegionFaultSummaryModel
    {
        public string REGION { get; set; } = "";
        public string PROVINCE { get; set; } = "";

        public int CEA { get; set; }
        public int MSAN { get; set; }
        public int SLBN { get; set; }
    }
}