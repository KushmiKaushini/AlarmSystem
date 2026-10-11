using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AlarmSystem.Services;

namespace AlarmSystem.Services.EmailSender
{
    /// <summary>
    /// Service for sending emails using SMTP.
    /// </summary>
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSenderService> _logger;
        private readonly FaultReportService _faultReportService;

        public EmailSenderService(
            IConfiguration configuration,
            ILogger<EmailSenderService> logger,
            FaultReportService faultReportService)
        {
            _configuration = configuration;
            _logger = logger;
            _faultReportService = faultReportService;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpConfig = _configuration.GetSection("EmailReporting:Smtp");
                var host = smtpConfig["Host"];
                var port = int.Parse(smtpConfig["Port"] ?? "587");
                var username = smtpConfig["Username"];
                var password = smtpConfig["Password"];
                var enableSsl = bool.Parse(smtpConfig["EnableSsl"] ?? "true");
                var fromAddress = smtpConfig["FromAddress"];

                using var mailMessage = new MailMessage
                {
                    From = !string.IsNullOrEmpty(fromAddress)
                        ? new MailAddress(fromAddress)
                        : new MailAddress("no-reply@slt.com.lk"),
                    Subject = subject,
                    Body = body?.Trim(),
                    IsBodyHtml = isHtml
                };
                mailMessage.To.Add(to);

                // Force a clean HTML content type if it is an HTML email
                if (isHtml)
                {
                    // Note: System.Net.Mail.MailMessage uses IsBodyHtml to set the header.
                    // If the body is still rendering as plain text, it is usually because
                    // the content is already HTML-encoded (double-encoded).
                }

                using var smtpClient = new SmtpClient(host, port)
                {
                    Credentials = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)
                        ? new NetworkCredential(username, password)
                        : null,
                    EnableSsl = enableSsl
                };

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }

        public async Task SendReportEmailAsync(int reportType, DateTime reportDate, string recipients)
        {
            try
            {
                var html = await _faultReportService.GetEmailPreviewHtmlAsync(reportType, reportDate);
                string dateStr = reportDate.ToString("dd MMM yyyy");
                string subject = reportType switch
                {
                    1 => $"Pending Data Faults Related to Regional Network Teams on {dateStr}",
                    2 => $"Pending Data fault related to OPMC {dateStr}",
                    3 => $"Pending Data Faults Related to SLT NOC - Fault Management Teams on {dateStr}",
                    4 => $"Pending Network Failures as at 08:00 AM on {dateStr}",
                    _ => $"Daily Network Report – {dateStr}"
                };

                var recipientList = recipients.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var recipient in recipientList)
                {
                    await SendEmailAsync(recipient.Trim(), subject, html, true);
                }

                _logger.LogInformation("Report {ReportType} sent for {ReportDate} to {Recipients}", reportType, reportDate, recipients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send report {ReportType} for {ReportDate}", reportType, reportDate);
                throw;
            }
        }
    }
}