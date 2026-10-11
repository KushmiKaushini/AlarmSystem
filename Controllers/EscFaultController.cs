using AlarmSystem.Models;
using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class EscFaultController : Controller
    {
        private readonly FaultService _faultService;

        public EscFaultController(FaultService faultService)
        {
            _faultService = faultService;
        }

        public IActionResult Details(long id)
        {
            EscFaultViewModel vm = new EscFaultViewModel();

            vm.Ticket = _faultService.GetTicket(id);

            if (vm.Ticket == null)
            {
                return Content("Ticket is NULL");
            }

            vm.Attributes = _faultService.GetAttributes(vm.Ticket.ID);

            vm.Comments = _faultService.GetComments(vm.Ticket.ID);

            vm.Escalations = _faultService.GetEscalations(vm.Ticket.ID);

 if (vm.Escalations.Count > 0)
{
    vm.Members = _faultService.GetEscalationMembers(vm.Escalations[0].ID);
}

vm.MessageText = _faultService.GetMessage(vm.Ticket.ID);

            vm.Groups = _faultService.GetGroups();
            vm.Level3Groups = _faultService.GetLevel3Groups();

            if (vm.Level3Groups.Count > 0)
{
    vm.Level2Groups = _faultService.GetChildGroups(vm.Level3Groups[0].ID);

    if (vm.Level2Groups.Count > 0)
    {
        vm.Level1Groups = _faultService.GetChildGroups(vm.Level2Groups[0].ID);

        if (vm.Level1Groups.Count > 0)
        {
            vm.Level0Groups = _faultService.GetChildGroups(vm.Level1Groups[0].ID);
        }
    }
}

            return View(vm);
        }


        // 👇👇 Details() method-ku keela add pannu

        [HttpPost]
        public IActionResult Ack(long id)
        {
            _faultService.AckTicket(id);

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public IActionResult Clear(long id)
        {
            _faultService.ClearTicket(id);

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public IActionResult Close(long id)
        {
            _faultService.CloseTicket(id);

            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
public IActionResult GetChildGroups(long parentId)
{
    var groups = _faultService.GetChildGroups(parentId);

    return Json(groups);
}

        [HttpPost]
public IActionResult AddComment(long id, string newComment)
{
    if (!string.IsNullOrWhiteSpace(newComment))
    {
        _faultService.SubmitComment(id, newComment, "SYSTEM");
    }

    return RedirectToAction("Details", new { id });
}
[HttpPost]
public IActionResult Escalate(long id, long groupId, string escalationLevel)
{
    string groupName = _faultService.GetGroupName(groupId);

    _faultService.InsertEscalation(
        id,
        groupId,
        groupName,
        escalationLevel,
        "SYSTEM",
        "SYSTEM");

    long escId = _faultService.GetLastEscalationId();

    var members = _faultService.GetGroupMembers(groupId);

    string message = _faultService.GetMessage(id);

    foreach (var m in members)
    {
        _faultService.InsertEscalationSecondary(
            escId,
            m.SVC,
            m.NAME,
            m.MOBILE,
            _faultService.GetCli(),
            message);
    }

    return RedirectToAction("Details", new { id });
}


    }
}