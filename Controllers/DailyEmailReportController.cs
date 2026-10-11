using AlarmSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    /// <summary>
    /// Provides browser-accessible preview endpoints for all 4 daily email report types.
    ///
    /// Routes:
    ///   GET /DailyEmailReport                       → Index (selector page)
    ///   GET /DailyEmailReport/Preview?type=1..4&date=YYYY-MM-DD → Live rendered standalone HTML email
    ///   GET /DailyEmailReport/Subject?type=1..4&date=YYYY-MM-DD → Subject line as plain text
    /// </summary>
    [AllowAnonymous]
    public class DailyEmailReportController : Controller
    {
        private readonly FaultReportService _faultReportService;

        public DailyEmailReportController(FaultReportService faultReportService)
        {
            _faultReportService = faultReportService;
        }

        // ── Index — Type selector landing page ───────────────────────────────────
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ── Preview — Render the email HTML for the selected type and date ─────────
        [HttpGet]
        public async Task<IActionResult> Preview(int type = 1, DateTime? date = null)
        {
            if (type < 1 || type > 4)
                type = 1;

            var html = await _faultReportService.GetEmailPreviewHtmlAsync(type, date);
            return Content(html, "text/html");
        }

        // ── Subject — Returns only the email subject string as plain text ─────────
        [HttpGet]
        public IActionResult Subject(int type = 1, DateTime? date = null)
        {
            var effectiveDate = date ?? DateTime.Today;
            string dateStr = effectiveDate.ToString("dd MMM yyyy");

            string subject = type switch
            {
                1 => $"Pending Data Faults Related to Regional Network Teams on {dateStr}",
                2 => $"Pending Data fault related to OPMC {dateStr}",
                3 => $"Pending Data Faults Related to SLT NOC - Fault Management Teams on {dateStr}",
                4 => $"Pending Network Failures as at 08:00 AM on {dateStr}",
                _ => $"Daily Network Report – {dateStr}"
            };

            return Content(subject, "text/plain");
        }
    }
}
