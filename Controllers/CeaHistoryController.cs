using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class CeaHistoryController : Controller
    {
        private readonly CeaHistoryService _service;

        public CeaHistoryController(CeaHistoryService service)
        {
            _service = service;
        }

       public IActionResult Index(
    DateTime? from,
    DateTime? to,
    string nwEng = "~ALL~")
{
    CeaHistoryViewModel vm = new CeaHistoryViewModel();

    // Default dates
    vm.From = from ?? DateTime.Today.AddDays(-30);
    vm.To = to ?? DateTime.Today;

 vm.NwEng = string.IsNullOrWhiteSpace(nwEng)
    ? "ALL"
    : nwEng;

vm.Engineers = _service.GetNetworkEngineers();

// Dropdown shows ~ALL~, but SP needs ALL
string engineer = vm.NwEng;

if (engineer == "~ALL~")
{
    engineer = "ALL";
}

vm.Alarms = _service.GetHistory(
    vm.From.Value,
    vm.To.Value,
    engineer);

    return View(vm);
}
    }
}