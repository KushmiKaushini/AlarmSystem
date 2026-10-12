using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;

namespace AlarmSystem.Controllers
{
    public class AlarmController : Controller
    {
        private readonly AlarmService _service;

        public AlarmController(AlarmService service)
        {
            _service = service;
        }

        //=====================================================
        // ALARM LIST
        //=====================================================
        public IActionResult Index(
            string platform = "~ALL~",
            string nwEng = "~ALL~",
            string alarmType = "~ALL~")
        {
            var alarms = _service.GetAlarms(platform, nwEng, alarmType);

            ViewBag.Platform = platform;
            ViewBag.NwEng = nwEng;
            ViewBag.AlarmType = alarmType;

            ViewBag.Platforms = _service.GetPlatforms();

ViewBag.AlarmTypes = _service.GetAlarmTypes();

ViewBag.NetworkEngineers = _service.GetNetworkEngineers();

            return View(alarms);
        }

        //=====================================================
        // ALARM DETAILS
        //=====================================================
     public IActionResult Details(int id)
{
    var alarm = _service.GetAlarmDetails(id);

    if (alarm == null)
        return NotFound();

    var vm = new AlarmDetailsViewModel
    {
        Alarm = alarm,
        TicketId = _service.GetTicketIdFromAlarm(id),
        AutoEscalations = _service.GetAutoEscalations(id),
        SmsEscalations = _service.GetSmsEscalations(id)
    };

    ViewBag.ExistingTicket = _service.TicketExists(
        alarm.NODE,
        alarm.FAULT);

    return View(vm);
}

        //=====================================================
        // CLOSE ALARM
        //=====================================================
        [HttpPost]
        public IActionResult Close(int id)
        {
            bool result = _service.CloseAlarm(id);

            if (result)
            {
                TempData["Success"] = "Alarm Closed Successfully.";
            }
            else
            {
                TempData["Error"] = "Unable to Close Alarm.";
            }

            return RedirectToAction(nameof(Index));
        }
        //=====================================================
// CREATE ESCALATION
//=====================================================

[HttpPost]
public IActionResult CreateEscalation(int id)
{
    var alarm = _service.GetAlarmDetails(id);

    if (alarm == null)
    {
        return NotFound();
    }

    int existingTicket = _service.TicketExists(
        alarm.NODE,
        alarm.FAULT);

    if (existingTicket > 0)
    {
        return RedirectToAction(
            "Details",
            "Ticket",
            new { id = existingTicket });
    }

    string reportingBy = User.Identity?.Name ?? "SYSTEM";

    int ticketId = _service.CreateTicketFault(
        alarm.NODE,
        "CRITICAL",
        "FAULT",
        "",
        alarm.PLATFORM,
        "NONE",
        alarm.FAULT,
        "MANUAL",
        alarm.FAULT_TIME,
        DateTime.Now,
        reportingBy,
        "OTHER",
        "",
        $"{alarm.NODE} {alarm.FAULT} {alarm.FAULT_TIME}",
        "DEFAULT"
    );

    if (ticketId > 0)
    {
        _service.AddAlarmReference(ticketId, alarm.ID);

        TempData["Success"] = "Escalation Created Successfully.";

        return RedirectToAction(
            "Details",
            "Ticket",
            new { id = ticketId });
    }

    TempData["Error"] = "Unable to Create Escalation.";

    return RedirectToAction(nameof(Details), new { id });
}

public IActionResult ExportToExcel(
    string platform = "~ALL~",
    string nwEng = "~ALL~",
    string alarmType = "~ALL~")
{
    var alarms = _service.GetAlarms(platform, nwEng, alarmType);

    using var workbook = new XLWorkbook();

    var ws = workbook.Worksheets.Add("Alarm List");

    ws.Cell(1, 1).Value = "ID";
    ws.Cell(1, 2).Value = "NODE";
    ws.Cell(1, 3).Value = "PLATFORM";
    ws.Cell(1, 4).Value = "FAULT";
    ws.Cell(1, 5).Value = "ALARM TYPE";
    ws.Cell(1, 6).Value = "FAULT TIME";
    ws.Cell(1, 7).Value = "HOURS";
    ws.Cell(1, 8).Value = "NW ENGINEER";

    int row = 2;

    foreach (var item in alarms)
    {
        ws.Cell(row, 1).SetValue(item.ID);
        ws.Cell(row, 2).SetValue(item.NODE ?? string.Empty);
        ws.Cell(row, 3).SetValue(item.PLATFORM ?? string.Empty);
        ws.Cell(row, 4).SetValue(item.FAULT ?? string.Empty);
        ws.Cell(row, 5).SetValue(item.ALARM_TYPE ?? string.Empty);
        if (item.FAULT_TIME != default && item.FAULT_TIME != DateTime.MinValue)
        {
            ws.Cell(row, 6).SetValue(item.FAULT_TIME);
            ws.Cell(row, 6).Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
        }
        else
        {
            ws.Cell(row, 6).SetValue(string.Empty);
        }
        ws.Cell(row, 7).SetValue(item.HOURS);
        ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0";
        ws.Cell(row, 8).SetValue(item.NW_ENG ?? string.Empty);

        row++;
    }

    ws.Columns().AdjustToContents();

    using var stream = new MemoryStream();

    workbook.SaveAs(stream);

    stream.Position = 0;

    return File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "AlarmList.xlsx");
}
    }
}