using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using System.IO;

namespace AlarmSystem.Controllers
{
    public class MsanBatteryBackupController : Controller
    {
        private readonly AlarmService _alarmService;

        public MsanBatteryBackupController(AlarmService alarmService)
        {
            _alarmService = alarmService;
        }

        public IActionResult Index()
        {
            var model = _alarmService.GetMsanBatteryBackup();
            return View(model);
        }

        public IActionResult ExportToExcel()
{
    var data = _alarmService.GetMsanBatteryBackup();

    using var workbook = new XLWorkbook();

    var ws = workbook.Worksheets.Add("MSAN Battery Backup");

    ws.Cell(1, 1).Value = "NODE";
    ws.Cell(1, 2).Value = "REGION";
    ws.Cell(1, 3).Value = "PROVINCE";
    ws.Cell(1, 4).Value = "LEA";
    ws.Cell(1, 5).Value = "NW ENG";
    ws.Cell(1, 6).Value = "NW ENG NAME";
    ws.Cell(1, 7).Value = "HOURS";
    ws.Cell(1, 8).Value = "MINUTES";
    ws.Cell(1, 9).Value = "CUSTOMER COUNT";

    int row = 2;

    foreach (var item in data)
    {
        ws.Cell(row, 1).Value = item.NODE;
        ws.Cell(row, 2).Value = item.Region;
        ws.Cell(row, 3).Value = item.Province;
        ws.Cell(row, 4).Value = item.LEA;
        ws.Cell(row, 5).Value = item.NwEng;
        ws.Cell(row, 6).Value = item.NwEngName;
        ws.Cell(row, 7).Value = item.HOURS;
        ws.Cell(row, 8).Value = item.MINUTES;
        ws.Cell(row, 9).Value = item.CustomerCount;
        row++;
    }

    ws.Columns().AdjustToContents();

    using var stream = new MemoryStream();

    workbook.SaveAs(stream);

    return File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "MSAN_Battery_Backup.xlsx");
}
    }
}