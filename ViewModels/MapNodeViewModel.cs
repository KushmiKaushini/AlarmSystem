namespace AlarmSystem.Models.ViewModels
{
    public class MapNodeViewModel
    {
        public string Location { get; set; } = "";

        public string Progress { get; set; } = "";

        public string NWENG { get; set; } = "";

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public int Status { get; set; }

        public string? MSAN { get; set; }

public string? CEANode { get; set; }

public string? RPB { get; set; }
    }
}