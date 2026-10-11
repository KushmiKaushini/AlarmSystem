using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class SummaryCarrierFaultsController : Controller
    {
        private readonly SummaryCarrierFaultService _service;

        public SummaryCarrierFaultsController(SummaryCarrierFaultService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            SummaryCarrierFaultViewModel vm = new SummaryCarrierFaultViewModel
            {
                Summary = _service.GetSummary(),
                Details = _service.GetDetails()
            };

            return View(vm);
        }

        public IActionResult Export()
        {
            return View();
        }
    }
}