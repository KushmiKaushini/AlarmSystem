using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class NewTicketController : Controller
    {
        private readonly NewTicketService _service; 
        private readonly GroupService _groupService;

      public NewTicketController(
    NewTicketService service,
    GroupService groupService)
{
    _service = service;
    _groupService = groupService;
}

        // ============================
        // CREATE PAGE
        // ============================
[HttpGet]
public IActionResult Create(long? level3, long? level2)
{
    TicketCreateViewModel vm = _service.LoadPage();

    string profile = "SAT";

    // LEVEL 3
    vm.Level3Groups = _groupService.GetGroups(0, profile);

    // LEVEL 2
    if (level3.HasValue)
    {
        vm.Level3Id = level3.Value;
        vm.Level2Groups = _groupService.GetGroups((int)level3.Value, profile);

        vm.ResponsibleOfficer =
            _groupService.GetGroupName((int)level3.Value);
    }

    // LEVEL 1
    if (level2.HasValue)
    {
        vm.Level2Id = level2.Value;
        vm.Level1Groups = _groupService.GetGroups((int)level2.Value, profile);

        vm.ResponsibleOfficer =
            _groupService.GetGroupName((int)level2.Value);
    }

    return View(vm);
}
        // ============================
        // SAVE TICKET
        // ============================
        [HttpPost]
        public IActionResult Create(TicketCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm = _service.LoadPage();
                return View(vm);
            }

            long ticketId = _service.CreateTicket(vm);

            if (ticketId > 0)
            {
                TempData["Success"] = "Ticket Created Successfully.";

                return RedirectToAction(
                    "Details",
                    "ManualEscalation",
                    new { id = ticketId });
            }

            TempData["Error"] = "Unable to create ticket.";

            vm = _service.LoadPage();
            return View(vm);
        }
    }
}