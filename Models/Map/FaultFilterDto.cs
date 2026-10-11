namespace AlarmSystem.Models.Map
{
    public class FaultFilterDto
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string? Carrier { get; set; }   // Dialog/Mobitel/Hutch/Airtel
        public string? Search { get; set; }    // keyword search

        public string? R_GM { get; set; }
        public string? P_DGM { get; set; }
        public string? Region { get; set; }
        public string? Province { get; set; }

        public int Top { get; set; } = 5000;   // safety limit
    }
}