using AlarmSystem.Models.ViewModels;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarmSystem.Controllers
{
    public class ReportCeaPortDownController : Controller
    {
        private readonly IReportCeaPortDownService _service;

        public ReportCeaPortDownController(
            IReportCeaPortDownService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(
            DateTime? from,
            DateTime? to,
            string customer)
        {
            // Default dates (same as TMS)
            if (from == null)
                from = new DateTime(DateTime.Now.Year, 1, 1);

            if (to == null)
                to = DateTime.Today;

            if (string.IsNullOrEmpty(customer))
                customer = "~ALL~";

            ReportCeaPortDownViewModel vm =
                new ReportCeaPortDownViewModel();

            vm.From = from;
            vm.To = to;
            vm.Customer = customer;

            vm.Customers = (await _service.GetCustomersAsync())
                .Select(x => new SelectListItem
                {
                    Text = x,
                    Value = x
                })
                .ToList();

            vm.Records = await _service.GetReportAsync(
                from,
                to,
                customer);

            return View(vm);
        }
    }
}