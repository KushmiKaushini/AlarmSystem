using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using System.IO;

namespace AlarmSystem.Controllers
{
    public class SummaryNetworkFaultsController : Controller
    {
        private readonly SummaryNetworkFaultService _service;

        public SummaryNetworkFaultsController(SummaryNetworkFaultService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            SummaryNetworkFaultViewModel vm = new SummaryNetworkFaultViewModel
            {
                Summary = _service.GetSummary(),
                Details = _service.GetDetails()
            };

            return View(vm);
        }

       public IActionResult Export()
{
    var data = _service.GetDetails();

    using var workbook = new XLWorkbook();

    var ws = workbook.Worksheets.Add("Network Fault Summary");

    // Header
    ws.Cell(1, 1).Value = "REGION";
    ws.Cell(1, 2).Value = "PROVINCE";

    ws.Cell(1, 3).Value = "Dialog_LT1D";
    ws.Cell(1, 4).Value = "Dialog_1D_3D";
    ws.Cell(1, 5).Value = "Dialog_3D_7D";
    ws.Cell(1, 6).Value = "Dialog_GT7D";

    ws.Cell(1, 7).Value = "Mobitel_LT1D";
    ws.Cell(1, 8).Value = "Mobitel_1D_3D";
    ws.Cell(1, 9).Value = "Mobitel_3D_7D";
    ws.Cell(1, 10).Value = "Mobitel_GT7D";

    ws.Cell(1, 11).Value = "Airtel_LT1D";
    ws.Cell(1, 12).Value = "Airtel_1D_3D";
    ws.Cell(1, 13).Value = "Airtel_3D_7D";
    ws.Cell(1, 14).Value = "Airtel_GT7D";

    ws.Cell(1, 15).Value = "Hutch_LT1D";
    ws.Cell(1, 16).Value = "Hutch_1D_3D";
    ws.Cell(1, 17).Value = "Hutch_3D_7D";
    ws.Cell(1, 18).Value = "Hutch_GT7D";

        int row = 2;

    foreach (var item in data)
    {
        ws.Cell(row, 1).Value = item.REGION;
        ws.Cell(row, 2).Value = item.PROVINCE;

        ws.Cell(row, 3).Value = item.Dialog_LT1D;
        ws.Cell(row, 4).Value = item.Dialog_1D_3D;
        ws.Cell(row, 5).Value = item.Dialog_3D_7D;
        ws.Cell(row, 6).Value = item.Dialog_GT7D;

        ws.Cell(row, 7).Value = item.Mobitel_LT1D;
        ws.Cell(row, 8).Value = item.Mobitel_1D_3D;
        ws.Cell(row, 9).Value = item.Mobitel_3D_7D;
        ws.Cell(row, 10).Value = item.Mobitel_GT7D;

        ws.Cell(row, 11).Value = item.Airtel_LT1D;
        ws.Cell(row, 12).Value = item.Airtel_1D_3D;
        ws.Cell(row, 13).Value = item.Airtel_3D_7D;
        ws.Cell(row, 14).Value = item.Airtel_GT7D;

        ws.Cell(row, 15).Value = item.Hutch_LT1D;
        ws.Cell(row, 16).Value = item.Hutch_1D_3D;
        ws.Cell(row, 17).Value = item.Hutch_3D_7D;
        ws.Cell(row, 18).Value = item.Hutch_GT7D;

        row++;
    }

    ws.Columns().AdjustToContents();

    using var stream = new MemoryStream();

    workbook.SaveAs(stream);

    stream.Position = 0;

    return File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "SummaryNetworkFaults.xlsx");
}
    }
}