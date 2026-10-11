using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class RpbSummaryController : Controller
    {
        private readonly IRpbSummaryService _service;

        public RpbSummaryController(IRpbSummaryService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await _service.GetSummaryAsync();

            return View(vm);
        }
    }
}