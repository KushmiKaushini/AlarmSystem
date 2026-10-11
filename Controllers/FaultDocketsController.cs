using AlarmSystem.Models;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class FaultDocketsController : Controller
    {
        private readonly FaultDocketService _service;

        public FaultDocketsController(FaultDocketService service)
        {
            _service = service;
        }

        public IActionResult Index(
            string customerType = "~ALL~",
            string workGroup = "~ALL~")
        {
            ViewBag.CustomerTypes = _service.GetCustomerTypes();

            ViewBag.WorkGroups = new List<string>
            {
                "~ALL~",
                "CEN-CSC-NW",
                "CEN-CSC-DATA",
                "CEN-CSC-CC",
                "CEN-CSC-MS"
            };

            ViewBag.SelectedCustomerType = customerType;
            ViewBag.SelectedWorkGroup = workGroup;
            ViewBag.PageTitle = "CLARITY FAULT-DOCKETS";

            List<FaultDocket> list =
                _service.GetFaultDockets(customerType, workGroup);

            return View(list);
        }

        // 👇 ADD THIS HERE
        public IActionResult Export()
        {
            var data = _service.GetExportData();

            return _service.ExportToExcel(data);
        }

        public IActionResult Details(int id)
{
    var vm = new ClarityFaultDetailsViewModel
    {
        Details = _service.GetFaultDetails(id),

        WorkOrders = _service.GetWorkOrders(id),

        Comments = _service.GetComments(id)
    };

    return View(vm);
}

    }
}