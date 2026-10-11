using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AlarmSystem.Services.EmailSender;

namespace AlarmSystem.Services.EmailReporting
{
    /// <summary>
    /// Background service that sends daily email reports at scheduled times.
    /// Sends all report types at the earliest configured time each day.
    /// </summary>
    public class DailyEmailReportSchedulerService : BackgroundService
    {
        private readonly ILogger<DailyEmailReportSchedulerService> _logger;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IConfiguration _configuration;

        public DailyEmailReportSchedulerService(
            ILogger<DailyEmailReportSchedulerService> logger,
            IEmailSenderService emailSenderService,
            IConfiguration configuration)
        {
            _logger = logger;
            _emailSenderService = emailSenderService;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Email Report Scheduler Service starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Get the schedule configuration
                var scheduleConfig = _configuration.GetSection("EmailReporting:Schedule");
                var schedule = new Dictionary<int, TimeSpan>();
                foreach (var reportType in new[] { 1, 2, 3, 4 })
                {
                    var timeStr = scheduleConfig[$"Type{reportType}"];
                    if (!string.IsNullOrWhiteSpace(timeStr) && TimeSpan.TryParse(timeStr, out var time))
                    {
                        schedule[reportType] = time;
                    }
                    else
                    {
                        // Default times if not configured
                        switch (reportType)
                        {
                            case 1: schedule[reportType] = TimeSpan.Parse("08:00"); break;
                            case 2: schedule[reportType] = TimeSpan.Parse("07:00"); break;
                            case 3: schedule[reportType] = TimeSpan.Parse("00:15"); break;
                            case 4: schedule[reportType] = TimeSpan.Parse("08:00"); break;
                        }
                    }
                }

                // Calculate the next run time (earliest time in the schedule)
                var now = DateTime.Now;
                var earliestTime = schedule.Values.Min();
                var today = now.Date;
                var nextRun = today + earliestTime;
                if (nextRun <= now)
                {
                    nextRun = nextRun.AddDays(1);
                }

                var delay = nextRun - now;
                _logger.LogInformation("Next email report run scheduled for {NextRun} (in {Delay})", nextRun, delay);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                if (stoppingToken.IsCancellationRequested)
                    break;

                // Send all reports
                await SendAllReports();
            }
        }

        private async Task SendAllReports()
        {
            var now = DateTime.Now;
            var reportDate = now.Date; // We want to send the report for today's date

            // Get recipients from configuration
            var recipientsConfig = _configuration.GetSection("EmailReporting:Recipients");

            var reportTypes = new[] { 1, 2, 3, 4 };
            foreach (var reportType in reportTypes)
            {
                var recipientsKey = $"Type{reportType}";
                var recipients = recipientsConfig[recipientsKey];
                if (!string.IsNullOrWhiteSpace(recipients))
                {
                    try
                    {
                        await _emailSenderService.SendReportEmailAsync(reportType, reportDate, recipients);
                        _logger.LogInformation("Report {ReportType} sent successfully for {ReportDate}", reportType, reportDate);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send report {ReportType} for {ReportDate}", reportType, reportDate);
                    }
                }
                else
                {
                    _logger.LogWarning("No recipients configured for report type {ReportType}", reportType);
                }
            }
        }
    }
}