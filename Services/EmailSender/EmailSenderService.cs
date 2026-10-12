using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AlarmSystem.Services;

namespace AlarmSystem.Services.EmailSender
{
    /// <summary>
    /// Service for sending emails using SMTP.
    /// Registered as a Singleton; uses <see cref="IServiceScopeFactory"/> to resolve
    /// the Scoped <see cref="FaultReportService"/> on-demand, avoiding the
    /// "cannot consume scoped service from singleton" DI lifetime conflict.
    /// </summary>
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSenderService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public EmailSenderService(
            IConfiguration configuration,
            ILogger<EmailSenderService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _scopeFactory = scopeFactory;
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
                // Create a per-call scope so we can resolve the Scoped FaultReportService
                // from this Singleton service without violating DI lifetime rules.
                await using var scope = _scopeFactory.CreateAsyncScope();
                var faultReportService = scope.ServiceProvider.GetRequiredService<FaultReportService>();
                var html = await faultReportService.GetEmailPreviewHtmlAsync(reportType, reportDate);
                string dateStr = reportDate.ToString("dd MMM yyyy");
                string timeStr = DateTime.Now.ToString("hh:00 tt");
                string subject = reportType switch
                {
                    1 => $"Pending Data Faults Related to Regional Network Teams as at {timeStr} ({dateStr})",
                    2 => $"Pending Data fault related to OPMC as at {timeStr} ({dateStr})",
                    3 => $"Pending Data Faults Related to SLT NOC - Fault Management Teams as at {timeStr} ({dateStr})",
                    4 => $"Pending Network Failures as at {timeStr} on {dateStr}",
                    _ => $"Daily Network Report as at {timeStr} – {dateStr}"
                };

                var recipientList = recipients.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
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