using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;

namespace AlarmSystem.Controllers
{
    public class ACMainFailureController : Controller
    {
        private readonly ACMainFailureService _service;

        public ACMainFailureController(ACMainFailureService service)
        {
            _service = service;
        }

        public IActionResult Index(
            DateTime? from,
            DateTime? to,
            string platform = "~ALL~",
            string nwEng = "~ALL~")
        {
            DateTime fromDate = from ?? DateTime.Today;
            DateTime toDate = to ?? DateTime.Today;

            // Load report
            var reports = _service.GetReport(
                fromDate,
                toDate,
                platform,
                nwEng);

            Console.WriteLine($"Rows = {reports.Count}");

            // ViewModel
            ACMainFailureViewModel vm = new ACMainFailureViewModel
            {
                From = fromDate,
                To = toDate,

                SelectedPlatform = platform,
                SelectedEngineer = nwEng,

                Platforms = _service.GetPlatforms(),

                Engineers = _service.GetEngineers(),

                Reports = reports
            };

            ViewBag.Count = vm.Reports.Count;

            return View(vm);
        }

        [HttpGet]
public IActionResult Export(
    DateTime from,
    DateTime to,
    string platform = "~ALL~",
    string nwEng = "~ALL~")
{
    var data = _service.GetReport(from, to, platform, nwEng);

    using var workbook = new XLWorkbook();

    var ws = workbook.Worksheets.Add("AC Mains Failure");

    ws.Cell(1, 1).Value = "NODE";
    ws.Cell(1, 2).Value = "ALARM TYPE";
    ws.Cell(1, 3).Value = "PLATFORM";
    ws.Cell(1, 4).Value = "FAULT TIME";
    ws.Cell(1, 5).Value = "CLOSE TIME";
    ws.Cell(1, 6).Value = "REGION";
    ws.Cell(1, 7).Value = "PROVINCE";
    ws.Cell(1, 8).Value = "NW ENG";
    ws.Cell(1, 9).Value = "RTOM";
    ws.Cell(1, 10).Value = "DURATION";

    int row = 2;

    foreach (var item in data)
    {
        ws.Cell(row, 1).Value = item.NODE;
        ws.Cell(row, 2).Value = item.ALARM_TYPE;
        ws.Cell(row, 3).Value = item.PLATFORM;
        ws.Cell(row, 4).Value = item.FAULT_TIME;
        ws.Cell(row, 5).Value = item.CLOSE_TIME;
        ws.Cell(row, 6).Value = item.REGION;
        ws.Cell(row, 7).Value = item.PROVINCE;
        ws.Cell(row, 8).Value = item.NW_ENG;
        ws.Cell(row, 9).Value = item.RTOM;
        ws.Cell(row, 10).Value = item.DURATION;

        row++;
    }

    using var stream = new MemoryStream();
    workbook.SaveAs(stream);

    return File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "ACMainFailure.xlsx");
}

        
    }
}