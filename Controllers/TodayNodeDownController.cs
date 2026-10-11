using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;
using AlarmSystem.Models;
using ClosedXML.Excel;
using System.Data;

namespace AlarmSystem.Controllers
{
    public class TodayNodeDownController : Controller
    {
        private readonly TodayNodeDownService _service;

        public TodayNodeDownController(TodayNodeDownService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            var model = _service.GetTodayNodeDown();
            return View(model);
        }
        public IActionResult ExportExcel()
{
    var data = _service.GetTodayNodeDown();

    using var workbook = new XLWorkbook();

    var ws = workbook.Worksheets.Add("Node Down Report");

    ws.Cell(1, 1).Value = "Site Name";
    ws.Cell(1, 2).Value = "Node";
    ws.Cell(1, 3).Value = "Network Eng";
    ws.Cell(1, 4).Value = "Region";
    ws.Cell(1, 5).Value = "Province";
    ws.Cell(1, 6).Value = "Vendor";
    ws.Cell(1, 7).Value = "MSAN/OLT";
    ws.Cell(1, 8).Value = "AC Mains Failure Time";
    ws.Cell(1, 9).Value = "AC Mains Failure";
    ws.Cell(1,10).Value = "Battery Low Time";
    ws.Cell(1,11).Value = "Battery Low";
    ws.Cell(1,12).Value = "Node Down Time";
    ws.Cell(1,13).Value = "Node Down";

    int row = 2;

    foreach (var item in data)
    {
        ws.Cell(row, 1).Value = item.Site_Name;
        ws.Cell(row, 2).Value = item.Node;
        ws.Cell(row, 3).Value = item.Network_Eng;
        ws.Cell(row, 4).Value = item.Region;
        ws.Cell(row, 5).Value = item.Province;
        ws.Cell(row, 6).Value = item.Vendor;
        ws.Cell(row, 7).Value = item.MSANOLT;
        ws.Cell(row, 8).Value = item.AC_Mains_Failure_time;
        ws.Cell(row, 9).Value = item.AC_Mains_Failure;
        ws.Cell(row,10).Value = item.Battery_Low_time;
        ws.Cell(row,11).Value = item.Battery_Low;
        ws.Cell(row,12).Value = item.Node_Down_time;
        ws.Cell(row,13).Value = item.Node_Down;

        row++;
    }

    ws.Columns().AdjustToContents();

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