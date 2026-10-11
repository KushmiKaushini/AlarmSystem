using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;

namespace AlarmSystem.Controllers
{
    public class InboxController : Controller
    {
        private readonly InboxService _inboxService;

        public InboxController(InboxService inboxService)
        {
            _inboxService = inboxService;
        }

        public IActionResult Index()
        {
            var model = _inboxService.GetInbox();
            return View(model);
        }

        public IActionResult ExportToExcel()
{
    var list = _inboxService.GetInbox();

    using var workbook = new XLWorkbook();

    var ws = workbook.Worksheets.Add("SMS Inbox");

    ws.Cell(1, 1).Value = "Time";
    ws.Cell(1, 2).Value = "Sender";
    ws.Cell(1, 3).Value = "Message";
    ws.Cell(1, 4).Value = "Destination";

    int row = 2;

    foreach (var item in list)
    {
        ws.Cell(row, 1).Value = item.Time;
        ws.Cell(row, 2).Value = item.Sender;
        ws.Cell(row, 3).Value = item.Message;
        ws.Cell(row, 4).Value = item.Destination;

        row++;
    }

    ws.Columns().AdjustToContents();

    using var stream = new MemoryStream();

    workbook.SaveAs(stream);

    stream.Position = 0;

    return File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "SMSInbox.xlsx");
}
    }
}