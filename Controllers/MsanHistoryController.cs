using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class MsanHistoryController : Controller
    {
        private readonly MsanHistoryService _service;

        public MsanHistoryController(MsanHistoryService service)
        {
            _service = service;
        }

        //=====================================================
        // VIEW / UPDATE OUTAGE REASON FOR MSAN HISTORY ALARMS
        //=====================================================
        [HttpGet]
        public IActionResult Index(
            DateTime? from,
            DateTime? to,
            string nwEng = "~ALL~")
        {
            MsanHistoryViewModel vm = new MsanHistoryViewModel();

            vm.FromDate = from ?? DateTime.Today.AddDays(-4);
            vm.ToDate = to ?? DateTime.Today;

            vm.SelectedEngineer = string.IsNullOrEmpty(nwEng)
                ? "~ALL~"
                : nwEng;

            vm.Engineers = _service.GetNetworkEngineers();

            vm.Alarms = _service.GetHistory(
                vm.FromDate,
                vm.ToDate,
                vm.SelectedEngineer);

            return View(vm);
        }
    }
}