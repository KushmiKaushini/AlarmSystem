using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class TroubleTicketController : Controller
    {
        private readonly TroubleTicketService _service;

        public TroubleTicketController(TroubleTicketService service)
        {
            _service = service;
        }

        public IActionResult Index(DateTime? from, DateTime? to)
        {
            TroubleTicketViewModel vm = new TroubleTicketViewModel();

            // Default Dates (Last 30 Days)
            vm.From = from ?? DateTime.Today.AddDays(-30);
            vm.To = to ?? DateTime.Today;

            vm.Tickets = _service.GetTickets(vm.From, vm.To);

            return View(vm);
        }
    }
}