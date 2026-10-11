using AlarmSystem.Models.ViewModels;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarmSystem.Controllers
{
    public class AlarmHistoryRegionController : Controller
    {
        private readonly IAlarmHistoryRegionService _service;

        public AlarmHistoryRegionController(
            IAlarmHistoryRegionService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(
            DateTime? from,
            DateTime? to,
            string platform,
            string region)
        {
            AlarmHistoryRegionViewModel vm =
                new AlarmHistoryRegionViewModel();

            vm.From = from;
            vm.To = to;
            vm.Platform = platform;
            vm.Region = region;

            vm.Platforms = (await _service.GetPlatformsAsync())
                .Select(x => new SelectListItem
                {
                    Text = x,
                    Value = x
                }).ToList();

            vm.Regions = (await _service.GetRegionsAsync())
                .Select(x => new SelectListItem
                {
                    Text = x,
                    Value = x
                }).ToList();

            vm.Records =
                await _service.GetReportAsync(
                    from,
                    to,
                    platform,
                    region);

            return View(vm);
        }
    }
}