using AlarmSystem.Models.ViewModels;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarmSystem.Controllers
{
    public class AlarmHistoryProvinceController : Controller
    {
        private readonly IAlarmHistoryProvinceService _service;

        public AlarmHistoryProvinceController(
            IAlarmHistoryProvinceService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(
            DateTime? from,
            DateTime? to,
            string platform,
            string province)
        {
            AlarmHistoryProvinceViewModel vm =
                new AlarmHistoryProvinceViewModel();

            vm.From = from;
            vm.To = to;
            vm.Platform = platform;
            vm.Province = province;

            vm.Platforms = (await _service.GetPlatformsAsync())
                .Select(x => new SelectListItem
                {
                    Text = x,
                    Value = x
                }).ToList();

            vm.Provinces = (await _service.GetProvincesAsync())
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
                    province);

            return View(vm);
        }
    }
}