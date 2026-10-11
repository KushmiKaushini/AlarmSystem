using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ClosedXML.Excel;
using System.IO;

namespace AlarmSystem.Controllers
{
    public class GeneratorController : Controller
    {
        private readonly AlarmService _alarmService;

        public GeneratorController(AlarmService alarmService)
        {
            _alarmService = alarmService;
        }

        public IActionResult Index(
            DateTime? from,
            DateTime? to,
            string nwEng = "~ALL~")
        {
            DateTime fromDate = from ?? DateTime.Today.AddDays(-30);
            DateTime toDate = to ?? DateTime.Today;

            ViewBag.From = fromDate;
            ViewBag.To = toDate;

            ViewBag.NwEng =
                new SelectList(
                    _alarmService.GetGeneratorEngineList(),
                    nwEng);

            var model =
                _alarmService.GetGeneratorRunningTime(
                    fromDate,
                    toDate,
                    nwEng);

            return View(model);
        }

        public IActionResult ExportToExcel(
    DateTime from,
    DateTime to,
    string nwEng = "~ALL~")
{
    var data = _alarmService.GetGeneratorRunningTime(from, to, nwEng);

    using XLWorkbook workbook = new XLWorkbook();

    var ws = workbook.Worksheets.Add("Generator");

    ws.Cell(1, 1).Value = "Location";
    ws.Cell(1, 2).Value = "Running Time (Hours)";

    int row = 2;

    foreach (var item in data)
    {
        ws.Cell(row, 1).Value = item.Location;
        ws.Cell(row, 2).Value = item.RunningTimeHours;

        row++;
    }

    ws.Columns().AdjustToContents();

    using MemoryStream stream = new MemoryStream();

    workbook.SaveAs(stream);

    return File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "Generator_Running_Time.xlsx");
}


    }
}