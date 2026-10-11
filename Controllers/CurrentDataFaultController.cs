using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class CurrentDataFaultController : Controller
    {
        private readonly AlarmService _alarmService;

        public CurrentDataFaultController(AlarmService alarmService)
        {
            _alarmService = alarmService;
        }

        public IActionResult Index()
        {
            var model = _alarmService.GetCurrentDataFaults();

            return View(model);
        }
    }
}