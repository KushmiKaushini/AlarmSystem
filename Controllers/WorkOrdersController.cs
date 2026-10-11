using AlarmSystem.Models;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class WorkOrdersController : Controller
    {
        private readonly WorkOrderService _service;

        public WorkOrdersController(WorkOrderService service)
        {
            _service = service;
        }

        public IActionResult Index(string workGroup = "CEN-CSC-DATA")
        {
            ViewBag.WorkGroup = workGroup;

            ViewBag.Groups = new List<string>
            {
                "CEN-CSC-DATA",
                "CEN-CSC-NW",
                "CEN-CSC-CC",
                "CEN-CSC-MS",
                "WORK-GROUPS"
            };

            List<WorkOrder> list =
                _service.GetWorkOrders(workGroup);

            return View(list);
        }
    }
}