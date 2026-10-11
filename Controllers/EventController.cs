using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventService _service;

        public EventsController(EventService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View(_service.GetEvents());
        }
    }
}