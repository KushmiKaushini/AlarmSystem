using System;

namespace AlarmSystem.Models.Alarm
{
    public class AlarmModel
    {
        public int ID { get; set; }

        public string NODE { get; set; }

        public string PLATFORM { get; set; }

        public DateTime FAULT_TIME { get; set; }

        public DateTime? CLOSE_TIME { get; set; }


        public string FAULT { get; set; }

        public string ALARM_TYPE { get; set; }

        public int HOURS { get; set; }

        public string NW_ENG { get; set; }

        public decimal REVENUE { get; set; }

       


public string? TAG { get; set; }

public string? FAULT_STATUS { get; set; }

public string? VENDOR { get; set; }

public string? PROVINCE { get; set; }

public string? REGION { get; set; }

public string? ACK_STATUS { get; set; }

public string? ACK_BY { get; set; }

public DateTime? ACK_TIME { get; set; }

public string? REF_ID { get; set; }

public string? SOURCEIP { get; set; }

    }
}