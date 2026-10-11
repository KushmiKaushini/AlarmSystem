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
        /// Recipient email addresses for each report type.
        /// Key: Report type (1-4), Value: Semicolon-separated list of email addresses.
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