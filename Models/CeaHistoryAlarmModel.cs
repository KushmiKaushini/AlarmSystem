using System;

namespace AlarmSystem.Models
{
    public class CeaHistoryAlarmModel
    {
        public int ID { get; set; }

        public string NODE { get; set; } = "";

        public string IP { get; set; } = "";

        public DateTime FAULT_TIME { get; set; }

        public DateTime? CLOSE_TIME { get; set; }

        public int Minutes { get; set; }

        public string FAULT { get; set; } = "";

        public string NW_ENG { get; set; } = "";

        public string Province { get; set; } = "";

        public string Region { get; set; } = "";

        public string TAG { get; set; } = "";
    }
}