namespace AlarmSystem.Models.Map
{
    public class MapPointDto
    {
        public string? CR { get; set; }
        public string? ACCOUNT { get; set; }
        public string? SERVICE { get; set; }

        // optional (if you want later)
        public string? CUSR_NAME { get; set; }     // customer/carrier
        public string? CCT { get; set; }           // circuit id
        public string? EQ_LOCATION_NODE { get; set; }

        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}