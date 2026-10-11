using AlarmSystem.Models;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class EditFaultController : Controller
    {
        private readonly FaultService _faultService;

        public EditFaultController(FaultService faultService)
        {
            _faultService = faultService;
        }

        public IActionResult Index(long id)
        {
            var ticket = _faultService.GetTicket(id);

            if (ticket == null)
                return NotFound();

            return View(ticket);
        }

        [HttpPost]
        public IActionResult UpdateTags(long id, string tag)
        {
            _faultService.UpdateTicketTags(id, tag);

            return RedirectToAction("Index", new { id });
        }

        [HttpPost]
        public IActionResult UpdateOfficer(long id, string officer)
        {
            _faultService.UpdateResponsibleOfficer(id, officer);

            return RedirectToAction("Index", new { id });
        }
    }
}