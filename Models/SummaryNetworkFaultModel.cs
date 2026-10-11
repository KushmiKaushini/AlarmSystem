namespace AlarmSystem.Models
{
    public class SummaryNetworkFaultModel
    {
        public string REGION { get; set; } = "";
        public string PROVINCE { get; set; } = "";

        // Dialog
        public int Dialog_LT1D { get; set; }
        public int Dialog_1D_3D { get; set; }
        public int Dialog_3D_7D { get; set; }
        public int Dialog_GT7D { get; set; }

        // Mobitel
        public int Mobitel_LT1D { get; set; }
        public int Mobitel_1D_3D { get; set; }
        public int Mobitel_3D_7D { get; set; }
        public int Mobitel_GT7D { get; set; }

        // Airtel
        public int Airtel_LT1D { get; set; }
        public int Airtel_1D_3D { get; set; }
        public int Airtel_3D_7D { get; set; }
        public int Airtel_GT7D { get; set; }

        // Hutch
        public int Hutch_LT1D { get; set; }
        public int Hutch_1D_3D { get; set; }
        public int Hutch_3D_7D { get; set; }
        public int Hutch_GT7D { get; set; }
    }
}