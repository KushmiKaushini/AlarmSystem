using System;

namespace AlarmSystem.Models
{
    public class MsanHistoryAlarmModel
    {
        public int ID { get; set; }

        public string NODE { get; set; } = "";

        public string PLATFORM { get; set; } = "";

        public DateTime FAULT_TIME { get; set; }

        public DateTime? CLOSE_TIME { get; set; }

        public int MINUTES { get; set; }

        public string FAULT { get; set; } = "";

        public string NW_Designation { get; set; } = "";

        public string Province { get; set; } = "";

        public string Region { get; set; } = "";

        public string TAG { get; set; } = "";
    }
}