namespace AlarmSystem.Models
{
    public class SummaryCarrierFaultModel
    {
        public string REGION { get; set; } = "";
        public string PROVINCE { get; set; } = "";

        // CEA
        public int CEA_LT1D { get; set; }
        public int CEA_1D_3D { get; set; }
        public int CEA_3D_7D { get; set; }
        public int CEA_GT7D { get; set; }

        // MSAN
        public int MSAN_LT1D { get; set; }
        public int MSAN_1D_3D { get; set; }
        public int MSAN_3D_7D { get; set; }
        public int MSAN_GT7D { get; set; }

        // SLBN
        public int SLBN_LT1D { get; set; }
        public int SLBN_1D_3D { get; set; }
        public int SLBN_3D_7D { get; set; }
        public int SLBN_GT7D { get; set; }
    }
}