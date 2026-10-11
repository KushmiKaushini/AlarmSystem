using System;

namespace AlarmSystem.Models.EmailReport
{
    /// <summary>
    /// Represents a row in the Type 4: Pending Network Failures (MSAN Report) email.
    /// </summary>
    public class Type4EmailViewModel
    {
        /// <summary>
        /// Network platform (e.g., MSAN)
        /// </summary>
        public string Network { get; set; } = string.Empty;

        /// <summary>
        /// Node name
        /// </summary>
        public string Node { get; set; } = string.Empty;

        /// <summary>
        /// Cleaned description/remarks (stripped of leading date/time stamps, etc.)
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Network engineer responsible
        /// </summary>
        public string NetworkEngineer { get; set; } = string.Empty;

        /// <summary>
        /// Province
        /// </summary>
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// Region
        /// </summary>
        public string Region { get; set; } = string.Empty;

        /// <summary>
        /// Fault detected timestamp
        /// </summary>
        public DateTime FaultDetected { get; set; }

                /// <summary>
        /// Pending hours (calculated as difference between FaultDetected and now)
        /// </summary>
        public int PendingHours { get; set; }

        /// <summary>
        /// Color code (GREEN, PINK, YELLOW, WHITE) based on pending hours and fault status
        /// </summary>
        public string ColorCode { get; set; } = "WHITE";

        /// <summary>
        /// Background color for the row (derived from ColorCode)
        /// </summary>
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
}