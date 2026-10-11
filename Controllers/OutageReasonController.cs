using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AlarmSystem.Models;

namespace AlarmSystem.Controllers
{
    public class OutageReasonController : Controller
    {
        private readonly AlarmService _alarmService;

        public OutageReasonController(AlarmService alarmService)
        {
            _alarmService = alarmService;
        }

        public IActionResult Index(
            string platform = "~ALL~",
            string nwEng = "~ALL~")
        {
            OutageReasonViewModel vm = new();

            vm.SelectedPlatform = platform;
            vm.SelectedNwEng = nwEng;

            vm.Platforms =
                _alarmService
                .GetPlatforms()
                .Select(x => new SelectListItem
                {
                    Text = x,
                    Value = x
                })
                .ToList();

            vm.NetworkEngineers =
                _alarmService
                .GetNetworkEngineers()
                .Select(x => new SelectListItem
                {
                    Text = x,
                    Value = x
                })
                .ToList();

            vm.Alarms =
                _alarmService.GetOutageReasons(platform, nwEng);

            return View(vm);
        }
        public IActionResult Edit(int id)
{
    var alarm = _alarmService.GetOutageReasons("~ALL~", "~ALL~")
                             .FirstOrDefault(x => x.ID == id);

    if (alarm == null)
        return NotFound();

    
    return View(alarm);
}

[HttpPost]
public IActionResult Edit(OutageReasonModel model)
{
    _alarmService.UpdateTag(model.ID, model.TAG);

    return RedirectToAction(nameof(Index));
}
    }
}