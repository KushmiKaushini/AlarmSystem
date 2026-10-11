using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class PowerAlarmController : Controller
    {
        private readonly PowerAlarmService _service;

        public PowerAlarmController(PowerAlarmService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            var model = _service.GetPowerAlarms();
            return View(model);
        }
    }
}