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
        ws.Cell(row, 1).SetValue(item.NODE ?? string.Empty);
        ws.Cell(row, 2).SetValue(item.ALARM_TYPE ?? string.Empty);
        ws.Cell(row, 3).SetValue(item.PLATFORM ?? string.Empty);
        if (item.FAULT_TIME.HasValue && item.FAULT_TIME.Value != DateTime.MinValue)
        {
            ws.Cell(row, 4).SetValue(item.FAULT_TIME.Value);
            ws.Cell(row, 4).Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
        }
        else
        {
            ws.Cell(row, 4).SetValue(string.Empty);
        }

        if (item.CLOSE_TIME.HasValue && item.CLOSE_TIME.Value != DateTime.MinValue)
        {
            ws.Cell(row, 5).SetValue(item.CLOSE_TIME.Value);
            ws.Cell(row, 5).Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
        }
        else
        {
            ws.Cell(row, 5).SetValue(string.Empty);
        }

        ws.Cell(row, 6).SetValue(item.REGION ?? string.Empty);
        ws.Cell(row, 7).SetValue(item.PROVINCE ?? string.Empty);
        ws.Cell(row, 8).SetValue(item.NW_ENG ?? string.Empty);
        ws.Cell(row, 9).SetValue(item.RTOM ?? string.Empty);
        ws.Cell(row, 10).SetValue(item.DURATION);
        ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00";

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