using AlarmSystem.Models;
using AlarmSystem.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using AlarmSystem.Services.Interfaces;

namespace AlarmSystem.Controllers
{
    public class CarrierFaultExportController : Controller
    {
      private readonly ICarrierFaultExportService _service;
      public CarrierFaultExportController(ICarrierFaultExportService service)
{
    _service = service;
}

        // OPEN PAGE
       public async Task<IActionResult> Index()
{
    var model = await _service.GetCarrierFaultsAsync();
    return View(model);
}

        // DOWNLOAD EXCEL
       public async Task<IActionResult> ExportExcel()
{
    var data = await _service.GetCarrierFaultsAsync();

            using var workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Add("Carrier Faults");

            // Report Title
ws.Cell("A1").Value = "CARRIER CUSTOMER FAULTS";
ws.Range("A1:G1").Merge();
ws.Cell("A1").Style.Font.Bold = true;
ws.Cell("A1").Style.Font.FontSize = 18;
ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

// Generated Date
ws.Cell("A2").Value = "Generated : " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");

int headerRow = 4;

ws.Cell(headerRow, 1).Value = "Problem No";
ws.Cell(headerRow, 2).Value = "Customer";
ws.Cell(headerRow, 3).Value = "Circuit";
ws.Cell(headerRow, 4).Value = "Reported Time";
ws.Cell(headerRow, 5).Value = "Priority";
ws.Cell(headerRow, 6).Value = "Status";
ws.Cell(headerRow, 7).Value = "COF";

var header = ws.Range(headerRow, 1, headerRow, 7);

header.Style.Font.Bold = true;
header.Style.Fill.BackgroundColor = XLColor.RoyalBlue;
header.Style.Font.FontColor = XLColor.White;
header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(1, 1).Value = "Problem No";
            ws.Cell(1, 2).Value = "Customer";
            ws.Cell(1, 3).Value = "Circuit";
            ws.Cell(1, 4).Value = "Reported Time";
            ws.Cell(1, 5).Value = "Priority";
            ws.Cell(1, 6).Value = "Status";
            ws.Cell(1, 7).Value = "COF";

            int row = 5;

            foreach (var item in data)
            {
                ws.Cell(row, 1).Value = item.problem_no;
                ws.Cell(row, 2).Value = item.customer_name;
                ws.Cell(row, 3).Value = item.circuit_name;
                ws.Cell(row, 4).Value = item.reportedtime;
                ws.Cell(row, 5).Value = item.priority;
                ws.Cell(row, 6).Value = item.problem_status;
                ws.Cell(row, 7).Value = item.COF;

                ws.Row(row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                row++;
            }

            ws.SheetView.FreezeRows(4);

ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "CarrierCustomerFaults.xlsx");
        }

        // BACK BUTTON
        public IActionResult Back()
        {
            return RedirectToAction("Index", "WarRoom");
        }
    }
}