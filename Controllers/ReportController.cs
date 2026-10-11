using AlarmSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using System.IO;

namespace AlarmSystem.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> NodeDownReport(
            DateTime? from,
            DateTime? to,
            string customer = "~ALL~")
        {
            // Default From Date (First Day of Current Year)
            if (!from.HasValue)
            {
                from = new DateTime(DateTime.Now.Year, 1, 1);
            }

            // Default To Date (Today)
            if (!to.HasValue)
            {
                to = DateTime.Today;
            }

            // Default Customer
            if (string.IsNullOrWhiteSpace(customer))
            {
                customer = "~ALL~";
            }

            var reports = await _reportService.GetNodeDownReportAsync(
                from.Value,
                to.Value,
                customer);

            ViewBag.From = from.Value.ToString("yyyy-MM-dd");
            ViewBag.To = to.Value.ToString("yyyy-MM-dd");
            ViewBag.Customer = customer;

            return View(reports);
        }
        [HttpGet]
public async Task<IActionResult> ExportNodeDownReport(
    DateTime from,
    DateTime to,
    string customer = "~ALL~")
{
    var reports = await _reportService.GetNodeDownReportAsync(from, to, customer);

    using var workbook = new ClosedXML.Excel.XLWorkbook();

    var worksheet = workbook.Worksheets.Add("Node Down Report");

    worksheet.Cell(1, 1).Value = "ID";
    worksheet.Cell(1, 2).Value = "NODE";
    worksheet.Cell(1, 3).Value = "FAULT";
    worksheet.Cell(1, 4).Value = "FAULT TIME";
    worksheet.Cell(1, 5).Value = "PLATFORM";
    worksheet.Cell(1, 6).Value = "STATUS";
    worksheet.Cell(1, 7).Value = "REGION";
    worksheet.Cell(1, 8).Value = "PROVINCE";
    worksheet.Cell(1, 9).Value = "NW ENG";
    worksheet.Cell(1,10).Value = "ALARM TYPE";
    worksheet.Cell(1,11).Value = "NODE IP";

    int row = 2;

    foreach (var item in reports)
    {
        worksheet.Cell(row, 1).Value = item.ID;
        worksheet.Cell(row, 2).Value = item.NODE;
        worksheet.Cell(row, 3).Value = item.FAULT;
        worksheet.Cell(row, 4).Value = item.FAULT_TIME?.ToString("dd-MMM-yyyy HH:mm");
        worksheet.Cell(row, 5).Value = item.PLATFORM;
        worksheet.Cell(row, 6).Value = item.FAULT_STATUS;
        worksheet.Cell(row, 7).Value = item.REGION;
        worksheet.Cell(row, 8).Value = item.PROVINCE;
        worksheet.Cell(row, 9).Value = item.NW_ENG;
        worksheet.Cell(row,10).Value = item.ALARM_TYPE;
        worksheet.Cell(row,11).Value = item.SOURCEIP;

        row++;
    }

    worksheet.Columns().AdjustToContents();

    using var stream = new MemoryStream();

    workbook.SaveAs(stream);

    stream.Position = 0;

    return File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"NodeDownReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
}
    }
}