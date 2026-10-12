using System.Collections.Generic;

namespace AlarmSystem.Configuration
{
    /// <summary>
    /// Configuration options for the email reporting feature.
    /// </summary>
    public class EmailReportingOptions
    {
        /// <summary>
        /// Whether the email reporting feature is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// SMTP configuration for sending emails.
        /// </summary>
        public SmtpOptions Smtp { get; set; } = new SmtpOptions();

        /// <summary>
        /// Whether automated emails should only be dispatched on working days (Monday - Friday).
        /// </summary>
        public bool WorkingDaysOnly { get; set; } = true;

        /// <summary>
        /// Start hour for automated dispatch (24-hour format, e.g. 8 for 08:00 AM).
        /// </summary>
        public int StartHour { get; set; } = 8;

        /// <summary>
        /// End hour for automated dispatch (inclusive 24-hour format, e.g. 17 for 05:00 PM).
        /// Set to 23 or 0 to run throughout the day.
        /// </summary>
        public int EndHour { get; set; } = 17;

        /// <summary>
        /// Interval in hours between automated dispatches (default: 1 for hourly).
        /// </summary>
        public int IntervalHours { get; set; } = 1;

        /// <summary>
        /// Default recipient(s) for network engineers when a type-specific recipient is not specified.
        /// </summary>
        public string? DefaultRecipient { get; set; }

        /// <summary>
        /// Recipient email addresses for each report type.
        /// Key: Report type (1-4), Value: Semicolon or comma-separated list of email addresses.
        /// </summary>
        public Dictionary<int, string> Recipients { get; set; } = new Dictionary<int, string>();

        /// <summary>
        /// Schedule for sending each report (time of day).
        /// Key: Report type (1-4), Value: Time of day in 24-hour format (e.g., "08:00").
        /// </summary>
        public Dictionary<int, string> Schedule { get; set; } = new Dictionary<int, string>();
    }

    /// <summary>
    /// SMTP configuration options.
    /// </summary>
    public class SmtpOptions
    {
        /// <summary>
        /// SMTP server host.
        /// </summary>
        public string Host { get; set; } = "smtp.office365.com";

        /// <summary>
        /// SMTP server port.
        /// </summary>
        public int Port { get; set; } = 587;

        /// <summary>
        /// SMTP username.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// SMTP password.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Whether to enable SSL.
        /// </summary>
        public bool EnableSsl { get; set; } = true;

        /// <summary>
        /// From email address.
        /// </summary>
        public string? FromAddress { get; set; }
    }
}