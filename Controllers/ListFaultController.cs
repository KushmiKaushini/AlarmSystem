using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class ListFaultController : Controller
    {
        private readonly FaultService _service;

        public ListFaultController(FaultService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            var faults = _service.GetAllFaults();

            return View(faults);
        }
    }
}