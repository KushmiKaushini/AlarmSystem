using System;

namespace AlarmSystem.Models.Reports
{
    public class NodeDownReportModel
    {
        public int ID { get; set; }

        public string NODE { get; set; }

        public string FAULT { get; set; }

        public DateTime? FAULT_TIME { get; set; }

        public string PLATFORM { get; set; }

        public string FAULT_STATUS { get; set; }

        public string REGION { get; set; }

        public string PROVINCE { get; set; }

        public string NW_ENG { get; set; }

        public string ALARM_TYPE { get; set; }

        public string SOURCEIP { get; set; }
    }
}