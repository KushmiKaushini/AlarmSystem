using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class FaultDetailsController : Controller
    {
        private readonly FaultDetailsService _service;

        public FaultDetailsController(FaultDetailsService service)
        {
            _service = service;
        }

        // ============================
        // DETAILS PAGE
        // ============================
        public IActionResult Details(int id)
        {
            FaultDetailsViewModel vm = new();

            vm.Detail = _service.GetFaultDetail(id);

            vm.WorkOrders = _service.GetWorkOrders(id);

            vm.Comments = _service.GetComments(id);

            return View(vm);
        }

        // ============================
        // ESCALATE
        // ============================
        [HttpPost]
        public IActionResult Escalate(int id)
        {
            // Later we'll convert the old TMS escalation
            // logic here.

            TempData["Success"] =
                "Escalation logic will be added.";

            return RedirectToAction(
                "Details",
                new { id = id });
        }

        
    }
}

