namespace AlarmSystem.Models
{
    public class RpbSummary
    {
        public string NWEng { get; set; } = "";
        public string Designation { get; set; } = "";
        public int DistributedUnits { get; set; }
        public int Installed { get; set; }
        public decimal CompletedP { get; set; }
    }
}