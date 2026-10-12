using System;
using System.Collections.Generic;

namespace AlarmSystem.Models.EmailReport
{
    /// <summary>
    /// View model for Type 4: Pending Network Failures (Comprehensive Report) email and preview.
    /// </summary>
    public class Type4EmailViewModel
    {
        public string? ReportDate { get; set; }
        public List<Type4MsanFaultItem> MsanFaults { get; set; } = new();
        public List<Type4Slt4GFaultItem> Slt4GFaults { get; set; } = new();
        public List<Type4TxFaultItem> TxFaults { get; set; } = new();

        // ── Summary counts (derived from fault lists) ──────────────────────────────
        /// <summary>Total number of MSAN network failures.</summary>
        public int TotalMsanCount => MsanFaults?.Count ?? 0;

        /// <summary>Total number of CEA / Transmission (TX) node failures.</summary>
        public int TotalCeaNodeCount => TxFaults?.Count ?? 0;

        /// <summary>Total number of SLT 4G sites down.</summary>
        public int TotalSlt4GCount => Slt4GFaults?.Count ?? 0;

        // Compatibility / single-row properties
        public string Network { get; set; } = string.Empty;
        public string Node { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string NetworkEngineer { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public DateTime FaultDetected { get; set; }
        public int PendingHours { get; set; }
        public string ColorCode { get; set; } = "WHITE";
        public string BackgroundColor
        {
            get
            {
                return ColorCode switch
                {
                    "GREEN" => "#D1E2C4",
                    "PINK" => "#E8B4B4",
                    "YELLOW" => "#FFFF00",
                    _ => "#FFFFFF"
                };
            }
        }
    }

    public class Type4MsanFaultItem
    {
        public int No { get; set; }
        public string Network { get; set; } = string.Empty;
        public string Node { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Engineer { get; set; } = string.Empty;
        public string NetworkEngineer { get => Engineer; set => Engineer = value; }
        public string Province { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string FaultDetectedDate { get; set; } = string.Empty;
        public DateTime? FaultDetected { get; set; }
        public int PendingHours { get; set; }
        public string BackgroundColor { get; set; } = "#FFFFFF";
    }

    public class Type4Slt4GFaultItem
    {
        public int No { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Ne { get; set; } = string.Empty;
        public string SubRegion { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public int NoOfDays { get; set; }
        public string AttendBy { get; set; } = string.Empty;

        /// <summary>
        /// Row background color derived from NoOfDays (converted to hours) using the legend palette.
        /// </summary>
        public string BackgroundColor
        {
            get
            {
                int hours = NoOfDays * 24;
                if (hours > 8)   return "#E8C2C2"; // Soft Pink  → More than 8 hrs
                if (hours >= 4)  return "#FFFF00"; // Yellow     → 4–8 Hours
                return "#FFFFFF";                  // White      → 0–4 Hours
            }
        }
    }

    public class Type4TxFaultItem
    {
        public int No { get; set; }
        public string Network { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Engineer { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string ResponsibleGroup { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public string FaultOccurredDate { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string FaultTime { get => Time; set => Time = value; }
        public int PendingHours { get; set; }

        /// <summary>
        /// Row background color derived from PendingHours using the legend palette.
        /// </summary>
        public string BackgroundColor
        {
            get
            {
                if (PendingHours > 8)  return "#E8C2C2"; // Soft Pink → More than 8 hrs
                if (PendingHours >= 4) return "#FFFF00"; // Yellow    → 4–8 Hours
                return "#FFFFFF";                        // White     → 0–4 Hours
            }
        }
    }
}