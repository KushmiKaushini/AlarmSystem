using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class LteBatteryBackupController : Controller
    {
        private readonly AlarmService _alarmService;

        public LteBatteryBackupController(AlarmService alarmService)
        {
            _alarmService = alarmService;
        }

        public IActionResult Index()
        {
            return View(_alarmService.GetLteBatteryBackup());
        }
    }
}