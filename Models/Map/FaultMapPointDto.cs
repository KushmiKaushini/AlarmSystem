namespace AlarmSystem.Models.Map
{
    public class FaultMapPointDto
    {
        public int ID { get; set; }
        public string? FAULT { get; set; }
        public DateTime? FAULT_TIME { get; set; }
        public string? FaultKey { get; set; }

        public string? CCT { get; set; }
        public string? EQ_LOCATION_NODE { get; set; }
        public string? NodeName { get; set; }
        public string? FaultType { get; set; }

        public string? CR { get; set; }
        public string? ACCOUNT { get; set; }
        public string? CIRCUIT_NAME { get; set; }
        public string? CIRCUIT_STATUS { get; set; }
        public string? CUSTOMER_NAME { get; set; }
        public string? CUSTOMER_ADDRESS { get; set; }
        public string? IP_ADDRESS { get; set; }

        public string? R_GM { get; set; }
        public string? P_DGM { get; set; }
        public string? REGION { get; set; }
        public string? PROVINCE { get; set; }

        // CCT side coordinates
        public double Lat { get; set; }
        public double Lng { get; set; }

        // CEA side coordinates (optional)
        public double? CeaLat { get; set; }
        public double? CeaLng { get; set; }

        public string? Carrier { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsPriority { get; set; }
    }
}