using AlarmSystem.Models;
using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;

namespace AlarmSystem.Controllers
{
    public class ManualEscalationController : Controller
    {
        private readonly EscalationService _service;

        public ManualEscalationController(EscalationService service)
        {
            _service = service;
        }

      public IActionResult Index(string type = "ALL", string platform = "ALL")
{
    List<Ticket> list = _service.GetTickets();

    string headingPlatform = platform;   // <-- Add this line

    if (type != "ALL")
    {
        list = list.Where(x => x.TICKET_TYPE == type).ToList();
    }

    // Platform Mapping
   // Platform Mapping
if (platform == "NETWORK")
{
    platform = "VPN";
}
else if (platform == "LTE/CDMA")
{
    platform = "LTE";
}

// POWER TAGED
if (platform == "POWER TAGED")
{
    list = list.Where(x => x.TAG == "POWER").ToList();
}
else if (platform != "ALL")
{
    list = list.Where(x => x.PLATFORM == platform).ToList();
}

    // Heading
    if (headingPlatform != "ALL")
    {
        ViewBag.PageTitle = headingPlatform + " FAULTS";
    }
    else if (type != "ALL")
    {
        ViewBag.PageTitle = type;
    }
    else
    {
        ViewBag.PageTitle = "ALL FAULTS";
    }

    ViewBag.SelectedType = type;
    ViewBag.SelectedPlatform = headingPlatform;

    return View(list);
}

        public IActionResult Details(long id,
                                     long? level3Id,
                                     long? level2Id,
                                     long? level1Id,
                                     long? editAttributeId)
        {
            EscalationDetailsViewModel vm = new EscalationDetailsViewModel();

            vm.EditAttributeId = editAttributeId;

           vm.Primary = _service.GetPrimaryByTicket(id);

if (vm.Primary != null)
{
    vm.SecondaryList = _service.GetSecondary(vm.Primary.ID);
}
else
{
    vm.SecondaryList = new List<EscalationSecondary>();
}

            vm.Ticket = _service.GetTicket(id);

            vm.Attributes = _service.GetAttributes(id);

            vm.Comments = _service.GetComments(id);

            vm.Escalations = _service.GetEscalationsByTicket(id);

            // Level 3 groups
            vm.Level3Groups = _service.GetGroups(0, "SAT");

            // Level 2 groups
            if (level3Id.HasValue)
            {
                vm.Level2Groups = _service.GetGroups((int)level3Id.Value, "SAT");
            }

            // Level 1 groups
            if (level2Id.HasValue)
            {
                vm.Level1Groups = _service.GetGroups((int)level2Id.Value, "SAT");
            }

            // Level 0 groups
            if (level1Id.HasValue)
            {
                vm.Level0Groups = _service.GetGroups((int)level1Id.Value, "SAT");
            }

            return View(vm);
        }


        // ===== ADD THIS METHOD =====
        public IActionResult DeleteEscalation(long id)
        {
            _service.DeleteEscalation(id);

            return Redirect(Request.Headers["Referer"].ToString());
        }

        public IActionResult ExportToExcel(string type = "ALL", string platform = "ALL")
{
    List<Ticket> list = _service.GetTickets();

    if (type != "ALL")
    {
        list = list.Where(x => x.TICKET_TYPE == type).ToList();
    }

    if (platform == "NETWORK")
    {
        platform = "VPN";
    }
    else if (platform == "LTE/CDMA")
    {
        platform = "LTE";
    }

    if (platform == "POWER TAGED")
    {
        list = list.Where(x => x.TAG == "POWER").ToList();
    }
    else if (platform != "ALL")
    {
        list = list.Where(x => x.PLATFORM == platform).ToList();
    }

    using var workbook = new XLWorkbook();

    var ws = workbook.Worksheets.Add("Manual Escalations");

    ws.Cell(1, 1).Value = "ID";
    ws.Cell(1, 2).Value = "NODE";
    ws.Cell(1, 3).Value = "SEVERITY";
    ws.Cell(1, 4).Value = "TAG";
    ws.Cell(1, 5).Value = "PLATFORM";
    ws.Cell(1, 6).Value = "DESCRIPTION";
    ws.Cell(1, 7).Value = "ISSUE TIME";
    ws.Cell(1, 8).Value = "REPORTING TIME";
    ws.Cell(1, 9).Value = "STATUS";

    int row = 2;

    foreach (var item in list)
    {
        ws.Cell(row, 1).Value = item.ID;
        ws.Cell(row, 2).Value = item.NODE;
        ws.Cell(row, 3).Value = item.SEVERITY;
        ws.Cell(row, 4).Value = item.TAG;
        ws.Cell(row, 5).Value = item.PLATFORM;
        ws.Cell(row, 6).Value = item.DESCRIPTION;
        ws.Cell(row, 7).Value = item.ISSUE_TIME?.ToString("yyyy-MM-dd HH:mm");
        ws.Cell(row, 8).Value = item.REPORTING_TIME?.ToString("yyyy-MM-dd HH:mm");
        ws.Cell(row, 9).Value = item.STATUS;

        row++;
    }

    ws.Columns().AdjustToContents();

    using var stream = new MemoryStream();

    workbook.SaveAs(stream);

    stream.Position = 0;

    return File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "ManualEscalations.xlsx");
}


    }
}