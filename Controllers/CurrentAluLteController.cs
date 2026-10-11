using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class CurrentAluLteController : Controller
    {
        private readonly AlarmService _alarmService;

        public CurrentAluLteController(AlarmService alarmService)
        {
            _alarmService = alarmService;
        }

        public IActionResult Index()
        {
            var model = _alarmService.GetCurrentAluLte();

            return View(model);
        }
    }
}