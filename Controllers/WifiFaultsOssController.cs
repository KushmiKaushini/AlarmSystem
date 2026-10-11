using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class WifiFaultsOssController : Controller
    {
        private readonly AlarmService _alarmService;

        public WifiFaultsOssController(AlarmService alarmService)
        {
            _alarmService = alarmService;
        }

        public IActionResult Index()
        {
            var model = _alarmService.GetWifiFaultsOss();
            return View(model);
        }
    }
}