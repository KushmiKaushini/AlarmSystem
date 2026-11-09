using System;
using System.Collections.Generic;

namespace AlarmSystem.Models.EmailReport
{
    public class DailyEmailReportPreviewViewModel
    {
        public int ReportType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string FormattedDate { get; set; } = string.Empty;
        public string ReportDate { get; set; } = string.Empty;
        public string ReportTitle { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public List<DataFaultRow>? Type1Faults { get; set; }
        public List<OpmcFaultRow>? Type2Faults { get; set; }
        public List<DataFaultRow>? Type3Faults { get; set; }
        public Type4EmailViewModel? Type4Data { get; set; }
    }
}
