using System.Threading.Tasks;

namespace AlarmSystem.Services.EmailSender
{
    /// <summary>
    /// Interface for sending emails.
    /// </summary>
    public interface IEmailSenderService
    {
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="to">Recipient email address(es).</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email body (HTML or plain text).</param>
        /// <param name="isHtml">Whether the body is HTML.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Sends a report email asynchronously.
        /// </summary>
        /// <param name="reportType">The type of report (1-4).</param>
        /// <param name="reportDate">The date for which the report is generated.</param>
        /// <param name="recipients">Semicolon-separated list of recipient email addresses.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task SendReportEmailAsync(int reportType, System.DateTime reportDate, string recipients);
    }
}