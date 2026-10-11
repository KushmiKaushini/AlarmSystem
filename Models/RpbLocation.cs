namespace AlarmSystem.Models
{
    public class RpbLocation
    {
        public string Location { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string NWENG { get; set; } = "";
        public string SiteCode { get; set; } = "";
        public int Status { get; set; }
    }
}