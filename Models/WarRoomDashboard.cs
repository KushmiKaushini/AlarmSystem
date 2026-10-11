using System;
using System.Collections.Generic;

namespace AlarmSystem.Models
{
    public class WarRoomDashboard
    {
        public List<PlatformSummary> PlatformSummary { get; set; } = new();

        public List<RegionSummary> RegionSummary { get; set; } = new();

        public List<OperatorSummary> OperatorSummary { get; set; } = new();

        public List<OperatorDetail> OperatorDetails { get; set; } = new();

        public DateTime UpdatedTime { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}