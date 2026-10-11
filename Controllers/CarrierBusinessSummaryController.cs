using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class CarrierBusinessSummaryController : Controller
    {
        private readonly CarrierBusinessSummaryService _service;

        public CarrierBusinessSummaryController(
            CarrierBusinessSummaryService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            ViewBag.Summary = _service.GetSummary();

            var model = _service.GetFaults();

            return View(model);
        }
        public IActionResult Export()
{
    var data = _service.GetFaults();

    return _service.ExportToExcel(data);
}
    }
}