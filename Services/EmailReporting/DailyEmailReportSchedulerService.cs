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
            _logger.LogInformation("Automated Email Report Scheduler Service started (Dispatch starting from 07:00 AM on working days).");

            var executedSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            DateTime lastCheckedDate = DateTime.Today;
            bool weekendNoticeLoggedToday = false;

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                // Reset executed slots on midnight crossover
                if (now.Date != lastCheckedDate)
                {
                    executedSlots.Clear();
                    lastCheckedDate = now.Date;
                    weekendNoticeLoggedToday = false;
                }

                bool isEnabled = _configuration.GetValue<bool?>("EmailReporting:Enabled") ?? true;
                if (!isEnabled)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    continue;
                }

                bool workingDaysOnly = _configuration.GetValue<bool?>("EmailReporting:WorkingDaysOnly") ?? true;
                bool isWorkingDay = now.DayOfWeek >= DayOfWeek.Monday && now.DayOfWeek <= DayOfWeek.Friday;

                // Working days filter: Monday through Friday only
                if (workingDaysOnly && !isWorkingDay)
                {
                    if (!weekendNoticeLoggedToday)
                    {
                        _logger.LogInformation("Today is {DayOfWeek} (Weekend/Non-working day). Automated hourly email dispatch paused for today.", now.DayOfWeek);
                        weekendNoticeLoggedToday = true;
                    }

                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    continue;
                }

                // Schedule window: defaults to 8:00 AM (08:00) to 5:00 PM (17:00)
                int startHour = _configuration.GetValue<int?>("EmailReporting:StartHour") ?? 7;
                int endHour = _configuration.GetValue<int?>("EmailReporting:EndHour") ?? 17;
                int intervalHours = Math.Max(1, _configuration.GetValue<int?>("EmailReporting:IntervalHours") ?? 1);

                // Check if current time is within active hours (from 08:00 to endHour)
                bool isInActiveHours = now.Hour >= startHour && (endHour <= 0 || now.Hour <= endHour);
                bool isHourlySlot = (now.Hour - startHour) % intervalHours == 0;

                if (isInActiveHours && isHourlySlot)
                {
                    var recipientsConfig = _configuration.GetSection("EmailReporting:Recipients");
                    string? defaultRecipient = _configuration["EmailReporting:DefaultRecipient"]
                                                ?? recipientsConfig["NetworkEngineer"]
                                                ?? recipientsConfig["NetworkEngineers"]
                                                ?? recipientsConfig["Default"];

                    foreach (var reportType in new[] { 1, 2, 3, 4 })
                    {
                        string executionKey = $"{now:yyyyMMdd}_{now.Hour:D2}_Type{reportType}";

                        if (!executedSlots.Contains(executionKey))
                        {
                            var recipients = recipientsConfig[reportType.ToString()]
                                             ?? recipientsConfig[$"Type{reportType}"];

                            if (string.IsNullOrWhiteSpace(recipients))
                            {
                                recipients = defaultRecipient;
                            }

                            if (!string.IsNullOrWhiteSpace(recipients))
                            {
                                try
                                {
                                    _logger.LogInformation(
                                        "Triggering automated hourly report Type {ReportType} for {Hour:D2}:00 on {Date:yyyy-MM-dd}. Recipients: {Recipients}",
                                        reportType, now.Hour, now.Date, recipients);

                                    await _emailSenderService.SendReportEmailAsync(reportType, now.Date, recipients);
                                    executedSlots.Add(executionKey);

                                    _logger.LogInformation(
                                        "Successfully dispatched automated hourly report Type {ReportType} to {Recipients}",
                                        reportType, recipients);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex,
                                        "Failed to send automated hourly report Type {ReportType} for slot {Hour:D2}:00",
                                        reportType, now.Hour);
                                }
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Skipping automated hourly report Type {ReportType} for slot {Hour:D2}:00: No recipient configured. Please set EmailReporting:Recipients:{ReportType} or EmailReporting:Recipients:NetworkEngineer in appsettings.json.",
                                    reportType, now.Hour);

                                executedSlots.Add(executionKey);
                            }
                        }
                    }
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}