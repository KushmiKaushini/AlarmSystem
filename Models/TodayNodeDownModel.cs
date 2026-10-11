namespace AlarmSystem.Models
{
    public class TodayNodeDownModel
    {
        public string Site_Name { get; set; }

        public string Node { get; set; }

        public string Network_Eng { get; set; }

        public string Region { get; set; }

        public string Province { get; set; }

        public string Vendor { get; set; }

        public string MSANOLT { get; set; }

        public DateTime? AC_Mains_Failure_time { get; set; }

        public string AC_Mains_Failure { get; set; }

        public DateTime? Battery_Low_time { get; set; }

        public string Battery_Low { get; set; }

        public DateTime? Node_Down_time { get; set; }

        public string Node_Down { get; set; }
    }
}