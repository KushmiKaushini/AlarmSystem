using AlarmSystem.Models.ViewModels;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class SummaryController : Controller
    {
        private readonly ISummaryService _service;

        public SummaryController(ISummaryService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            SummaryViewModel vm = new SummaryViewModel();

            vm.PendingSummary =
                await _service.GetPendingSummaryAsync();

            vm.PendingAlarms =
                await _service.GetPendingAlarmsAsync();

            vm.HistorySummary =
                await _service.GetHistorySummaryAsync();

            vm.HistoryAlarms =
                await _service.GetHistoryAlarmsAsync();

            return View(vm);
        }
    }
}