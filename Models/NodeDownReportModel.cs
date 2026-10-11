namespace AlarmSystem.Models
{
    public class NodeDownReportModel
    {
        public int ID { get; set; }

        public string NODE { get; set; } = "";

        public string FAULT { get; set; } = "";

        public DateTime? FAULT_TIME { get; set; }

        public string PLATFORM { get; set; } = "";

        public string VENDOR { get; set; } = "";

        public string FAULT_STATUS { get; set; } = "";

        public DateTime? CLOSE_TIME { get; set; }

        public string ALARM_TYPE { get; set; } = "";

        public string sourceip { get; set; } = "";

        public string Region1 { get; set; } = "";

        public string Province1 { get; set; } = "";

        public string NW_Designation { get; set; } = "";

        public string RTOM { get; set; } = "";

        public string LEA_Code { get; set; } = "";

        public string LEA_Name { get; set; } = "";

        public string REGION { get; set; } = "";
    }
}