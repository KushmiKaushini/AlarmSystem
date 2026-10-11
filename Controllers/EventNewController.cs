using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class EventNewController : Controller
    {
        private readonly EventNewService _service;

        public EventNewController(EventNewService service)
        {
            _service = service;
        }

        public IActionResult Index(string workGroup = "CEN-CSC-DATA")
        {
            ViewBag.WorkGroup = workGroup;

            ViewBag.Groups = new List<string>
            {
                "CEN-CSC-DATA",
                "CEN-CSC-NW"
            };

            var model = _service.GetEvents(workGroup);

            return View(model);
        }
    }
}