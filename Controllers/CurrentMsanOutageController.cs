using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using System.IO;

namespace AlarmSystem.Controllers
{
    public class CurrentMsanOutageController : Controller
    {
        private readonly CurrentMsanOutageService _service;

        public CurrentMsanOutageController(CurrentMsanOutageService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            var data = _service.GetCurrentMsanOutages();

            ViewBag.Total = data.Count;

            return View(data);
        }
        public IActionResult ExportToExcel()
{
    var list = _service.GetCurrentMsanOutages();

    using var workbook = new XLWorkbook();

    var ws = workbook.Worksheets.Add("Current MSAN Outages");

    ws.Cell(1, 1).Value = "ID";
    ws.Cell(1, 2).Value = "Platform";
    ws.Cell(1, 3).Value = "Fault Time";
    ws.Cell(1, 4).Value = "NW Engineer";
    ws.Cell(1, 5).Value = "Province";
    ws.Cell(1, 6).Value = "Region";
    ws.Cell(1, 7).Value = "Date";
    ws.Cell(1, 8).Value = "Node";
    ws.Cell(1, 9).Value = "Fault Occurred At";
    ws.Cell(1, 10).Value = "Pending Hours";

    int row = 2;

    foreach (var item in list)
    {
        ws.Cell(row, 1).Value = row - 1;
        ws.Cell(row, 2).Value = item.PLATFORM;
        ws.Cell(row, 3).Value = item.FAULT_TIME;
        ws.Cell(row, 4).Value = item.NW_ENG;
        ws.Cell(row, 5).Value = item.PROVINCE;
        ws.Cell(row, 6).Value = item.REGION;
        ws.Cell(row, 7).Value = item.DATE;
        ws.Cell(row, 8).Value = item.NODE;
        ws.Cell(row, 9).Value = item.FAULT_OCCURED_AT;
        ws.Cell(row, 10).Value = item.PENDING_HOURS;

        row++;
    }

    ws.Columns().AdjustToContents();

    using var stream = new MemoryStream();

    workbook.SaveAs(stream);

    stream.Position = 0;

    return File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "CurrentMsanOutages.xlsx");
}
    }
}