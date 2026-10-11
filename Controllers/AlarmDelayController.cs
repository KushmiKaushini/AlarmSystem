using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using System.IO;

namespace AlarmSystem.Controllers
{
    public class AlarmDelayController : Controller
    {
        private readonly AlarmDelayService _service;

        public AlarmDelayController(AlarmDelayService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            var list = _service.GetAlarmDelays();

            return View(list);
        }

        public IActionResult ExportToExcel()
{
    var list = _service.GetAlarmDelays();

    using var workbook = new XLWorkbook();

    var ws = workbook.Worksheets.Add("Alarm Delay");

    ws.Cell(1, 1).Value = "Platform";
    ws.Cell(1, 2).Value = "Alarm";
    ws.Cell(1, 3).Value = "Delay Minutes";
    ws.Cell(1, 4).Value = "Alarm Type";

    int row = 2;

    foreach (var item in list)
    {
        ws.Cell(row, 1).Value = item.PLATFORM;
        ws.Cell(row, 2).Value = item.ALARM;
        ws.Cell(row, 3).Value = item.DELAY_MINUTES;
        ws.Cell(row, 4).Value = item.ALARM_TYPE;

        row++;
    }

    ws.Columns().AdjustToContents();

    using var stream = new MemoryStream();

    workbook.SaveAs(stream);

    stream.Position = 0;

    return File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "AlarmDelay.xlsx");
}
    }
}