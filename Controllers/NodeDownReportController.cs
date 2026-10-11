using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class NodeDownReportController : Controller
    {
        private readonly NodeDownReportService _service;

        public NodeDownReportController(NodeDownReportService service)
        {
            _service = service;
        }

        public IActionResult Index(
            DateTime? from,
            DateTime? to,
            string nwEngineer = "~ALL~")
        {
            DateTime fromDate = from ?? DateTime.Today;
            DateTime toDate = to ?? DateTime.Today;

            NodeDownReportViewModel vm = new()
            {
                From = fromDate,
                To = toDate,
                SelectedEngineer = nwEngineer,

                Engineers = _service.GetEngineers(),

                Reports = _service.GetReport(
                    fromDate,
                    toDate,
                    nwEngineer)
            };

            return View(vm);
        }
    }
}