using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using AlarmSystem.Models.EmailReport;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    /// <summary>
    /// Provides browser-accessible preview endpoints and Excel export endpoints
    /// for all 4 daily email report types for SLT NOC.
    ///
    /// Routes:
    ///   GET /DailyEmailReport                       → Index (selector page with live connection status)
    ///   GET /DailyEmailReport/Preview?type=1..4&date=YYYY-MM-DD → Live rendered standalone HTML email preview with Download Excel button
    ///   GET /DailyEmailReport/Export?type=1..4&date=YYYY-MM-DD  → Download structured .xlsx Excel report
    ///   GET /DailyEmailReport/ExportExcel?type=1..4&date=...    → Route alias for Excel export
    ///   GET /DailyEmailReport/Subject?type=1..4&date=YYYY-MM-DD → Subject line as plain text
    ///   GET /DailyEmailReport/CheckConnection       → Live DB connection status JSON
    /// </summary>
    [AllowAnonymous]
    public class DailyEmailReportController : Controller
    {
        private readonly FaultReportService _faultReportService;

        // Shared corporate styling constants (SLT NOC theme)
        private static readonly XLColor BrandHeaderBg = XLColor.FromArgb(26, 60, 94); // Navy #1A3C5E
        private static readonly XLColor BrandHeaderFont = XLColor.White;
        private static readonly XLColor BorderGray = XLColor.FromArgb(210, 214, 220);
        private static readonly XLColor ZebraLight = XLColor.FromArgb(248, 249, 250);

        public DailyEmailReportController(FaultReportService faultReportService)
        {
            _faultReportService = faultReportService;
        }

        // ── Index — Type selector landing page ───────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var status = await _faultReportService.GetDatabaseConnectionStatusAsync();
            return View(status);
        }

        // ── CheckConnection — Live DB health diagnostics ─────────────────────────
        [HttpGet]
        public async Task<IActionResult> CheckConnection()
        {
            var status = await _faultReportService.GetDatabaseConnectionStatusAsync();
            return Json(status);
        }

        // ── Preview — Render the email HTML for the selected type and date ─────────
        [HttpGet]
        public async Task<IActionResult> Preview(int type = 1, DateTime? date = null)
        {
            if (type < 1 || type > 4)
                type = 1;

            var effectiveDate = date ?? await _faultReportService.GetLatestDataDateAsync() ?? DateTime.Today;
            var html = await _faultReportService.GetEmailPreviewHtmlAsync(type, effectiveDate);
            return Content(html, "text/html");
        }

        // ── Subject — Returns only the email subject string as plain text ─────────
        [HttpGet]
        public async Task<IActionResult> Subject(int type = 1, DateTime? date = null)
        {
            var effectiveDate = date ?? await _faultReportService.GetLatestDataDateAsync() ?? DateTime.Today;
            string dateStr = effectiveDate.ToString("dd MMM yyyy");

            string subject = type switch
            {
                1 => $"Pending Data Faults Related to Regional Network Teams as at 07:00 AM on {dateStr}",
                2 => $"Pending Data fault related to OPMC as at 07:00 AM on {dateStr}",
                3 => $"Pending Data Faults Related to SLT NOC - Fault Management Teams as at 07:00 AM on {dateStr}",
                4 => $"Pending Network Failures as at 07:00 AM on {dateStr}",
                _ => $"Daily Network Report as at 07:00 AM – {dateStr}"
            };

            return Content(subject, "text/plain");
        }

        // ── Export / ExportExcel — Download structured .xlsx Excel report ──────────
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Export(int type = 1, DateTime? date = null)
        {
            if (type < 1 || type > 4)
                type = 1;

            var effectiveDate = date ?? await _faultReportService.GetLatestDataDateAsync() ?? DateTime.Today;
            string dateStr = effectiveDate.ToString("dd MMM yyyy");
            string fileDate = effectiveDate.ToString("yyyyMMdd");

            using var workbook = new XLWorkbook();

            string fileName;

            switch (type)
            {
                case 1:
                {
                    fileName = $"Pending_Regional_Network_Faults_{fileDate}.xlsx";
                    var data = await _faultReportService.GetType1RegionalFaultsAsync(effectiveDate);
                    BuildType1Worksheet(workbook, data, dateStr);
                    break;
                }
                case 2:
                {
                    fileName = $"Pending_OPMC_Faults_{fileDate}.xlsx";
                    var data = await _faultReportService.GetType2OpmcFaultsAsync(effectiveDate);
                    BuildType2Worksheet(workbook, data, dateStr);
                    break;
                }
                case 3:
                {
                    fileName = $"Pending_NOC_Fault_Management_{fileDate}.xlsx";
                    var data = await _faultReportService.GetType3SltNocFaultsAsync(effectiveDate);
                    BuildType3Worksheet(workbook, data, dateStr);
                    break;
                }
                case 4:
                default:
                {
                    fileName = $"Pending_Network_Failures_{fileDate}.xlsx";
                    var data = await _faultReportService.GetType4ReportDataAsync(effectiveDate);
                    BuildType4Workbook(workbook, data, dateStr);
                    break;
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(stream.ToArray(), contentType, fileName);
        }

        [AllowAnonymous]
        [HttpGet]
        public Task<IActionResult> ExportExcel(int type = 1, DateTime? date = null) => Export(type, date);

        // ────────────────────────────────────────────────────────────────────────
        // CLOSEDXML EXCEL BUILDERS (Structured, crisp corporate styling)
        // ────────────────────────────────────────────────────────────────────────

        private void BuildType1Worksheet(XLWorkbook workbook, List<DataFaultRow> rows, string dateStr)
        {
            var ws = workbook.Worksheets.Add("Regional Network Faults");
            ws.ShowGridLines = true;

            // Title Banner
            ws.Cell(1, 1).Value = "SLT NOC – PENDING DATA FAULTS RELATED TO REGIONAL NETWORK TEAMS";
            ws.Range(1, 1, 1, 10).Merge();
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            ws.Cell(1, 1).Style.Fill.BackgroundColor = BrandHeaderBg;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(1).Height = 30;

            // Subtitle
            ws.Cell(2, 1).Value = $"Report Snapshot: {dateStr} at 07:00 AM | Generated: {DateTime.Now:dd-MMM-yyyy HH:mm} | Active Faults: {rows.Count}";
            ws.Range(2, 1, 2, 10).Merge();
            ws.Cell(2, 1).Style.Font.Italic = true;
            ws.Cell(2, 1).Style.Font.FontSize = 10;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(2).Height = 20;

            // Headers
            int headerRow = 4;
            string[] headers =
            {
                "No", "Data Circuit ID", "Details Of Customer", "Fault Date", "Fault Time",
                "Work Orders Held Up At", "Description", "Duration Of Complaint", "Region", "Province"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(headerRow, i + 1).Value = headers[i];
            }

            ApplyHeaderStyle(ws.Range(headerRow, 1, headerRow, headers.Length));
            ws.Row(headerRow).Height = 26;

            int rowIdx = 5;
            if (rows.Count == 0)
            {
                ws.Cell(rowIdx, 1).Value = "No pending faults found.";
                ws.Range(rowIdx, 1, rowIdx, headers.Length).Merge();
                ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(rowIdx, 1).Style.Font.Italic = true;
                ApplyBorder(ws.Range(rowIdx, 1, rowIdx, headers.Length));
                rowIdx++;
            }
            else
            {
                foreach (var item in rows)
                {
                    ws.Cell(rowIdx, 1).SetValue(item.No);
                    ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 2).SetValue(item.DataCircuitId ?? string.Empty);
                    ws.Cell(rowIdx, 2).Style.Font.Bold = true;

                    ws.Cell(rowIdx, 3).SetValue(item.DetailsOfCustomer ?? string.Empty);

                    ws.Cell(rowIdx, 4).SetValue(item.FaultDate ?? string.Empty);
                    ws.Cell(rowIdx, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 5).SetValue(item.FaultTime ?? string.Empty);
                    ws.Cell(rowIdx, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 6).SetValue(item.WorkOrdersHeldUpAt ?? string.Empty);
                    ws.Cell(rowIdx, 7).SetValue(item.Description ?? string.Empty);

                    ws.Cell(rowIdx, 8).SetValue(item.DurationOfComplaint ?? string.Empty);
                    ws.Cell(rowIdx, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 9).SetValue(item.Region ?? string.Empty);
                    ws.Cell(rowIdx, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 10).SetValue(item.Province ?? string.Empty);
                    ws.Cell(rowIdx, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    var rowRange = ws.Range(rowIdx, 1, rowIdx, headers.Length);
                    ApplyBorder(rowRange);

                    if (rowIdx % 2 == 0)
                    {
                        rowRange.Style.Fill.BackgroundColor = ZebraLight;
                    }

                    ws.Row(rowIdx).Height = 20;
                    rowIdx++;
                }
            }

            ws.Columns().AdjustToContents(10.0, 50.0);
        }

        private void BuildType2Worksheet(XLWorkbook workbook, List<OpmcFaultRow> rows, string dateStr)
        {
            var ws = workbook.Worksheets.Add("Pending OPMC Faults");
            ws.ShowGridLines = true;

            // Title Banner
            ws.Cell(1, 1).Value = "SLT NOC – PENDING DATA FAULTS RELATED TO OPMC";
            ws.Range(1, 1, 1, 10).Merge();
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            ws.Cell(1, 1).Style.Fill.BackgroundColor = BrandHeaderBg;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(1).Height = 30;

            // Subtitle
            ws.Cell(2, 1).Value = $"Report Snapshot: {dateStr} at 07:00 AM | Generated: {DateTime.Now:dd-MMM-yyyy HH:mm} | Active Faults: {rows.Count}";
            ws.Range(2, 1, 2, 10).Merge();
            ws.Cell(2, 1).Style.Font.Italic = true;
            ws.Cell(2, 1).Style.Font.FontSize = 10;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(2).Height = 20;

            // Headers
            int headerRow = 4;
            string[] headers =
            {
                "No", "REPORTED DATE", "CIRCUIT NAME", "CUSTOMER", "OUTAGE TIME",
                "LEA", "RELEVANT OPMC", "DGM", "GM", "DESCRIPTION / RECENT UPDATE"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(headerRow, i + 1).Value = headers[i];
            }

            ApplyHeaderStyle(ws.Range(headerRow, 1, headerRow, headers.Length));
            ws.Row(headerRow).Height = 26;

            int rowIdx = 5;
            if (rows.Count == 0)
            {
                ws.Cell(rowIdx, 1).Value = "No pending OPMC faults found.";
                ws.Range(rowIdx, 1, rowIdx, headers.Length).Merge();
                ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(rowIdx, 1).Style.Font.Italic = true;
                ApplyBorder(ws.Range(rowIdx, 1, rowIdx, headers.Length));
                rowIdx++;
            }
            else
            {
                foreach (var item in rows)
                {
                    ws.Cell(rowIdx, 1).SetValue(item.No);
                    ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 2).SetValue(item.ReportedDate ?? string.Empty);
                    ws.Cell(rowIdx, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 3).SetValue(item.CircuitName ?? string.Empty);
                    ws.Cell(rowIdx, 3).Style.Font.Bold = true;

                    ws.Cell(rowIdx, 4).SetValue(item.CustomerName ?? string.Empty);

                    ws.Cell(rowIdx, 5).SetValue(item.OutageTime ?? string.Empty);
                    ws.Cell(rowIdx, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 6).SetValue(item.Lea ?? string.Empty);
                    ws.Cell(rowIdx, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 7).SetValue(item.RelevantOpmc ?? string.Empty);
                    ws.Cell(rowIdx, 8).SetValue(item.Dgm ?? string.Empty);
                    ws.Cell(rowIdx, 9).SetValue(item.Gm ?? string.Empty);
                    ws.Cell(rowIdx, 10).SetValue(item.RecentUpdate ?? string.Empty);

                    var rowRange = ws.Range(rowIdx, 1, rowIdx, headers.Length);
                    ApplyBorder(rowRange);

                    if (rowIdx % 2 == 0)
                    {
                        rowRange.Style.Fill.BackgroundColor = ZebraLight;
                    }

                    ws.Row(rowIdx).Height = 20;
                    rowIdx++;
                }
            }

            ws.Columns().AdjustToContents(10.0, 50.0);
        }

        private void BuildType3Worksheet(XLWorkbook workbook, List<DataFaultRow> rows, string dateStr)
        {
            var ws = workbook.Worksheets.Add("NOC Fault Management");
            ws.ShowGridLines = true;

            // Title Banner
            ws.Cell(1, 1).Value = "SLT NOC – PENDING DATA FAULTS RELATED TO FAULT MANAGEMENT TEAMS";
            ws.Range(1, 1, 1, 10).Merge();
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            ws.Cell(1, 1).Style.Fill.BackgroundColor = BrandHeaderBg;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(1).Height = 30;

            // Subtitle
            ws.Cell(2, 1).Value = $"Report Snapshot: {dateStr} at 07:00 AM | Generated: {DateTime.Now:dd-MMM-yyyy HH:mm} | Active Faults: {rows.Count}";
            ws.Range(2, 1, 2, 10).Merge();
            ws.Cell(2, 1).Style.Font.Italic = true;
            ws.Cell(2, 1).Style.Font.FontSize = 10;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(2).Height = 20;

            // Headers
            int headerRow = 4;
            string[] headers =
            {
                "No", "Data Circuit ID", "Details Of Customer", "Fault Date", "Fault Time",
                "Work Orders Held Up At", "Description", "Duration Of Complaint", "Region", "Province"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(headerRow, i + 1).Value = headers[i];
            }

            ApplyHeaderStyle(ws.Range(headerRow, 1, headerRow, headers.Length));
            ws.Row(headerRow).Height = 26;

            int rowIdx = 5;
            if (rows.Count == 0)
            {
                ws.Cell(rowIdx, 1).Value = "No pending faults found.";
                ws.Range(rowIdx, 1, rowIdx, headers.Length).Merge();
                ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(rowIdx, 1).Style.Font.Italic = true;
                ApplyBorder(ws.Range(rowIdx, 1, rowIdx, headers.Length));
                rowIdx++;
            }
            else
            {
                foreach (var item in rows)
                {
                    ws.Cell(rowIdx, 1).SetValue(item.No);
                    ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 2).SetValue(item.DataCircuitId ?? string.Empty);
                    ws.Cell(rowIdx, 2).Style.Font.Bold = true;

                    ws.Cell(rowIdx, 3).SetValue(item.DetailsOfCustomer ?? string.Empty);

                    ws.Cell(rowIdx, 4).SetValue(item.FaultDate ?? string.Empty);
                    ws.Cell(rowIdx, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 5).SetValue(item.FaultTime ?? string.Empty);
                    ws.Cell(rowIdx, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 6).SetValue(item.WorkOrdersHeldUpAt ?? string.Empty);
                    ws.Cell(rowIdx, 7).SetValue(item.Description ?? string.Empty);

                    ws.Cell(rowIdx, 8).SetValue(item.DurationOfComplaint ?? string.Empty);
                    ws.Cell(rowIdx, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 9).SetValue(item.Region ?? string.Empty);
                    ws.Cell(rowIdx, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(rowIdx, 10).SetValue(item.Province ?? string.Empty);
                    ws.Cell(rowIdx, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    var rowRange = ws.Range(rowIdx, 1, rowIdx, headers.Length);
                    ApplyBorder(rowRange);

                    if (rowIdx % 2 == 0)
                    {
                        rowRange.Style.Fill.BackgroundColor = ZebraLight;
                    }

                    ws.Row(rowIdx).Height = 20;
                    rowIdx++;
                }
            }

            ws.Columns().AdjustToContents(10.0, 50.0);
        }

        private void BuildType4Workbook(XLWorkbook workbook, Type4EmailViewModel model, string dateStr)
        {
            // ── SHEET 1: Summary & Legend ───────────────────────────────────────
            var wsSummary = workbook.Worksheets.Add("Summary");
            wsSummary.ShowGridLines = true;

            wsSummary.Cell(1, 1).Value = "SLT NOC – PENDING NETWORK FAILURES COMPREHENSIVE REPORT";
            wsSummary.Range(1, 1, 1, 5).Merge();
            wsSummary.Cell(1, 1).Style.Font.Bold = true;
            wsSummary.Cell(1, 1).Style.Font.FontSize = 14;
            wsSummary.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            wsSummary.Cell(1, 1).Style.Fill.BackgroundColor = BrandHeaderBg;
            wsSummary.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            wsSummary.Row(1).Height = 30;

            wsSummary.Cell(2, 1).Value = $"As at 07:00 AM on {dateStr} | Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}";
            wsSummary.Range(2, 1, 2, 5).Merge();
            wsSummary.Cell(2, 1).Style.Font.Italic = true;
            wsSummary.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            wsSummary.Row(2).Height = 20;

            // Summary Table
            wsSummary.Cell(4, 1).Value = "CATEGORY / METRIC";
            wsSummary.Cell(4, 2).Value = "TOTAL OUTAGES";
            var sumHdr = wsSummary.Range(4, 1, 4, 2);
            ApplyHeaderStyle(sumHdr);
            wsSummary.Row(4).Height = 24;

            wsSummary.Cell(5, 1).Value = "Number of MSAN Failures";
            wsSummary.Cell(5, 2).SetValue(model.TotalMsanCount);
            wsSummary.Cell(5, 2).Style.Font.Bold = true;
            wsSummary.Cell(5, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyBorder(wsSummary.Range(5, 1, 5, 2));

            wsSummary.Cell(6, 1).Value = "Number of CEA / TX Nodes Down";
            wsSummary.Cell(6, 2).SetValue(model.TotalCeaNodeCount);
            wsSummary.Cell(6, 2).Style.Font.Bold = true;
            wsSummary.Cell(6, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyBorder(wsSummary.Range(6, 1, 6, 2));

            wsSummary.Cell(7, 1).Value = "SLT 4G Sites Down";
            wsSummary.Cell(7, 2).SetValue(model.TotalSlt4GCount);
            wsSummary.Cell(7, 2).Style.Font.Bold = true;
            wsSummary.Cell(7, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyBorder(wsSummary.Range(7, 1, 7, 2));

            wsSummary.Cell(8, 1).Value = "Total Network Failures";
            wsSummary.Cell(8, 1).Style.Font.Bold = true;
            wsSummary.Cell(8, 2).SetValue(model.TotalMsanCount + model.TotalCeaNodeCount + model.TotalSlt4GCount);
            wsSummary.Cell(8, 2).Style.Font.Bold = true;
            wsSummary.Cell(8, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            wsSummary.Range(8, 1, 8, 2).Style.Fill.BackgroundColor = XLColor.FromArgb(235, 241, 248);
            ApplyBorder(wsSummary.Range(8, 1, 8, 2));

            // SLA Color Coding Legend
            wsSummary.Cell(10, 1).Value = "SLA COLOR CODE";
            wsSummary.Cell(10, 2).Value = "OUTAGE DURATION CRITERIA";
            var legHdr = wsSummary.Range(10, 1, 10, 2);
            ApplyHeaderStyle(legHdr);
            wsSummary.Row(10).Height = 24;

            wsSummary.Cell(11, 1).Value = "Soft Pink";
            wsSummary.Cell(11, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(232, 194, 194);
            wsSummary.Cell(11, 2).Value = "More than 8 Hours (> 8 hrs)";
            ApplyBorder(wsSummary.Range(11, 1, 11, 2));

            wsSummary.Cell(12, 1).Value = "Yellow";
            wsSummary.Cell(12, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 0);
            wsSummary.Cell(12, 2).Value = "4 to 8 Hours (4–8 Hours)";
            ApplyBorder(wsSummary.Range(12, 1, 12, 2));

            wsSummary.Cell(13, 1).Value = "White";
            wsSummary.Cell(13, 1).Style.Fill.BackgroundColor = XLColor.White;
            wsSummary.Cell(13, 2).Value = "0 to 4 Hours (0–4 Hours)";
            ApplyBorder(wsSummary.Range(13, 1, 13, 2));

            wsSummary.Cell(14, 1).Value = "Soft Green";
            wsSummary.Cell(14, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 234, 211);
            wsSummary.Cell(14, 2).Value = "Up / Monitoring";
            ApplyBorder(wsSummary.Range(14, 1, 14, 2));

            wsSummary.Columns().AdjustToContents(15.0, 50.0);

            // ── SHEET 2: MSAN Network Failures ──────────────────────────────────
            var wsMsan = workbook.Worksheets.Add("MSAN Failures");
            wsMsan.ShowGridLines = true;

            wsMsan.Cell(1, 1).Value = $"MSAN NETWORK FAILURES – {dateStr}";
            wsMsan.Range(1, 1, 1, 8).Merge();
            wsMsan.Cell(1, 1).Style.Font.Bold = true;
            wsMsan.Cell(1, 1).Style.Font.FontSize = 13;
            wsMsan.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            wsMsan.Cell(1, 1).Style.Fill.BackgroundColor = BrandHeaderBg;
            wsMsan.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            wsMsan.Row(1).Height = 28;

            int msanHdrRow = 3;
            string[] msanHeaders = { "No", "Network", "Node", "Description", "Network Engineer", "Province", "Region", "Fault Detected" };
            for (int i = 0; i < msanHeaders.Length; i++)
            {
                wsMsan.Cell(msanHdrRow, i + 1).Value = msanHeaders[i];
            }
            ApplyHeaderStyle(wsMsan.Range(msanHdrRow, 1, msanHdrRow, msanHeaders.Length));
            wsMsan.Row(msanHdrRow).Height = 26;

            int mRow = 4;
            if (model.MsanFaults == null || model.MsanFaults.Count == 0)
            {
                wsMsan.Cell(mRow, 1).Value = "No pending MSAN faults found";
                wsMsan.Range(mRow, 1, mRow, msanHeaders.Length).Merge();
                wsMsan.Cell(mRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wsMsan.Cell(mRow, 1).Style.Font.Italic = true;
                ApplyBorder(wsMsan.Range(mRow, 1, mRow, msanHeaders.Length));
            }
            else
            {
                foreach (var item in model.MsanFaults)
                {
                    wsMsan.Cell(mRow, 1).SetValue(item.No);
                    wsMsan.Cell(mRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsMsan.Cell(mRow, 2).SetValue(item.Network ?? string.Empty);
                    wsMsan.Cell(mRow, 3).SetValue(item.Node ?? string.Empty);
                    wsMsan.Cell(mRow, 3).Style.Font.Bold = true;
                    wsMsan.Cell(mRow, 4).SetValue(item.Description ?? string.Empty);
                    wsMsan.Cell(mRow, 5).SetValue(item.Engineer ?? string.Empty);
                    wsMsan.Cell(mRow, 6).SetValue(item.Province ?? string.Empty);
                    wsMsan.Cell(mRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsMsan.Cell(mRow, 7).SetValue(item.Region ?? string.Empty);
                    wsMsan.Cell(mRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsMsan.Cell(mRow, 8).SetValue(item.FaultDetectedDate ?? string.Empty);
                    wsMsan.Cell(mRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    var rowRange = wsMsan.Range(mRow, 1, mRow, msanHeaders.Length);
                    ApplyBorder(rowRange);

                    var bg = ColorFromHex(item.BackgroundColor, XLColor.White);
                    if (bg != XLColor.White)
                    {
                        rowRange.Style.Fill.BackgroundColor = bg;
                    }

                    wsMsan.Row(mRow).Height = 20;
                    mRow++;
                }
            }
            wsMsan.Columns().AdjustToContents(10.0, 50.0);

            // ── SHEET 3: SLT 4G Sites Down ──────────────────────────────────────
            var ws4G = workbook.Worksheets.Add("SLT 4G Sites Down");
            ws4G.ShowGridLines = true;

            ws4G.Cell(1, 1).Value = $"SLT 4G SITES DOWN – {dateStr}";
            ws4G.Range(1, 1, 1, 8).Merge();
            ws4G.Cell(1, 1).Style.Font.Bold = true;
            ws4G.Cell(1, 1).Style.Font.FontSize = 13;
            ws4G.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            ws4G.Cell(1, 1).Style.Fill.BackgroundColor = BrandHeaderBg;
            ws4G.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws4G.Row(1).Height = 28;

            int gHdrRow = 3;
            string[] gHeaders = { "No", "Vendor", "Date", "NE", "SubRegion", "Reason", "No Of Days", "AttendBy" };
            for (int i = 0; i < gHeaders.Length; i++)
            {
                ws4G.Cell(gHdrRow, i + 1).Value = gHeaders[i];
            }
            ApplyHeaderStyle(ws4G.Range(gHdrRow, 1, gHdrRow, gHeaders.Length));
            ws4G.Row(gHdrRow).Height = 26;

            int gRow = 4;
            if (model.Slt4GFaults == null || model.Slt4GFaults.Count == 0)
            {
                ws4G.Cell(gRow, 1).Value = "No pending SLT 4G faults found";
                ws4G.Range(gRow, 1, gRow, gHeaders.Length).Merge();
                ws4G.Cell(gRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws4G.Cell(gRow, 1).Style.Font.Italic = true;
                ApplyBorder(ws4G.Range(gRow, 1, gRow, gHeaders.Length));
            }
            else
            {
                foreach (var item in model.Slt4GFaults)
                {
                    ws4G.Cell(gRow, 1).SetValue(item.No);
                    ws4G.Cell(gRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws4G.Cell(gRow, 2).SetValue(item.Vendor ?? string.Empty);
                    ws4G.Cell(gRow, 3).SetValue(item.Date ?? string.Empty);
                    ws4G.Cell(gRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws4G.Cell(gRow, 4).SetValue(item.Ne ?? string.Empty);
                    ws4G.Cell(gRow, 4).Style.Font.Bold = true;
                    ws4G.Cell(gRow, 5).SetValue(item.SubRegion ?? string.Empty);
                    ws4G.Cell(gRow, 6).SetValue(item.Reason ?? string.Empty);
                    ws4G.Cell(gRow, 7).SetValue(item.NoOfDays);
                    ws4G.Cell(gRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws4G.Cell(gRow, 8).SetValue(item.AttendBy ?? string.Empty);

                    var rowRange = ws4G.Range(gRow, 1, gRow, gHeaders.Length);
                    ApplyBorder(rowRange);

                    var bg = ColorFromHex(item.BackgroundColor, XLColor.White);
                    if (bg != XLColor.White)
                    {
                        rowRange.Style.Fill.BackgroundColor = bg;
                    }

                    ws4G.Row(gRow).Height = 20;
                    gRow++;
                }
            }
            ws4G.Columns().AdjustToContents(10.0, 50.0);

            // ── SHEET 4: Transmission (TX) Failures ─────────────────────────────
            var wsTx = workbook.Worksheets.Add("TX Network Failures");
            wsTx.ShowGridLines = true;

            wsTx.Cell(1, 1).Value = $"TRANSMISSION (TX) NETWORK FAILURES – {dateStr}";
            wsTx.Range(1, 1, 1, 11).Merge();
            wsTx.Cell(1, 1).Style.Font.Bold = true;
            wsTx.Cell(1, 1).Style.Font.FontSize = 13;
            wsTx.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            wsTx.Cell(1, 1).Style.Fill.BackgroundColor = BrandHeaderBg;
            wsTx.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            wsTx.Row(1).Height = 28;

            int txHdrRow = 3;
            string[] txHeaders =
            {
                "No", "Network", "Description", "Engineer", "Province", "Region",
                "Responsible Group", "Remarks", "Fault Occurred Date", "Time", "Pending Hours"
            };
            for (int i = 0; i < txHeaders.Length; i++)
            {
                wsTx.Cell(txHdrRow, i + 1).Value = txHeaders[i];
            }
            ApplyHeaderStyle(wsTx.Range(txHdrRow, 1, txHdrRow, txHeaders.Length));
            wsTx.Row(txHdrRow).Height = 26;

            int tRow = 4;
            if (model.TxFaults == null || model.TxFaults.Count == 0)
            {
                wsTx.Cell(tRow, 1).Value = "No outage";
                wsTx.Range(tRow, 1, tRow, txHeaders.Length).Merge();
                wsTx.Cell(tRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wsTx.Cell(tRow, 1).Style.Font.Italic = true;
                ApplyBorder(wsTx.Range(tRow, 1, tRow, txHeaders.Length));
            }
            else
            {
                foreach (var item in model.TxFaults)
                {
                    wsTx.Cell(tRow, 1).SetValue(item.No);
                    wsTx.Cell(tRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsTx.Cell(tRow, 2).SetValue(item.Network ?? string.Empty);
                    wsTx.Cell(tRow, 3).SetValue(item.Description ?? string.Empty);
                    wsTx.Cell(tRow, 4).SetValue(item.Engineer ?? string.Empty);
                    wsTx.Cell(tRow, 5).SetValue(item.Province ?? string.Empty);
                    wsTx.Cell(tRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsTx.Cell(tRow, 6).SetValue(item.Region ?? string.Empty);
                    wsTx.Cell(tRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsTx.Cell(tRow, 7).SetValue(item.ResponsibleGroup ?? string.Empty);
                    wsTx.Cell(tRow, 8).SetValue(item.Remarks ?? string.Empty);
                    wsTx.Cell(tRow, 9).SetValue(item.FaultOccurredDate ?? string.Empty);
                    wsTx.Cell(tRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsTx.Cell(tRow, 10).SetValue(item.Time ?? string.Empty);
                    wsTx.Cell(tRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wsTx.Cell(tRow, 11).SetValue(item.PendingHours);
                    wsTx.Cell(tRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    var rowRange = wsTx.Range(tRow, 1, tRow, txHeaders.Length);
                    ApplyBorder(rowRange);

                    var bg = ColorFromHex(item.BackgroundColor, XLColor.White);
                    if (bg != XLColor.White)
                    {
                        rowRange.Style.Fill.BackgroundColor = bg;
                    }

                    wsTx.Row(tRow).Height = 20;
                    tRow++;
                }
            }
            wsTx.Columns().AdjustToContents(10.0, 50.0);
        }

        // ── Shared ClosedXML Styling Helpers ─────────────────────────────────────

        private static void ApplyHeaderStyle(IXLRange headerRange)
        {
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 10;
            headerRange.Style.Font.FontColor = BrandHeaderFont;
            headerRange.Style.Fill.BackgroundColor = BrandHeaderBg;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Alignment.WrapText = true;

            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.OutsideBorderColor = BorderGray;
            headerRange.Style.Border.InsideBorderColor = BorderGray;
        }

        private static void ApplyBorder(IXLRange range)
        {
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.OutsideBorderColor = BorderGray;
            range.Style.Border.InsideBorderColor = BorderGray;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        private static XLColor ColorFromHex(string? hex, XLColor fallback)
        {
            if (string.IsNullOrWhiteSpace(hex)) return fallback;
            try
            {
                hex = hex.Trim();
                if (hex.StartsWith("#"))
                    hex = hex.Substring(1);

                if (hex.Length == 6)
                {
                    byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                    byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                    byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                    return XLColor.FromArgb(r, g, b);
                }

                return XLColor.FromHtml("#" + hex);
            }
            catch
            {
                return fallback;
            }
        }
    }
}
