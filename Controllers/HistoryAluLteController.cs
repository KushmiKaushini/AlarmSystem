using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class HistoryAluLteController : Controller
    {
        private readonly AlarmService _alarmService;

        public HistoryAluLteController(AlarmService alarmService)
        {
            _alarmService = alarmService;
        }

        public IActionResult Index()
        {
            var model = _alarmService.GetHistoryAluLte();

            return View(model);
        }
    }
}