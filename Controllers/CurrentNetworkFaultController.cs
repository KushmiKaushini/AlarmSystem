using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class CurrentNetworkFaultController : Controller
    {
        private readonly AlarmService _alarmService;

        public CurrentNetworkFaultController(AlarmService alarmService)
        {
            _alarmService = alarmService;
        }

        public IActionResult Index()
        {
            var model = _alarmService.GetCurrentNetworkFaults();

            return View(model);
        }
    }
}