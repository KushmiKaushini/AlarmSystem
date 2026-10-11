using AlarmSystem.Models;
using AlarmSystem.Services;
using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class WorkGroupsController : Controller
    {
        private readonly WorkGroupService _service;

        public WorkGroupsController(WorkGroupService service)
        {
            _service = service;
        }

        public IActionResult Index(
            string area = "All",
            string region = "All",
            string province = "All",
            string workGroup = "All")
        {
            WorkGroupViewModel vm = new WorkGroupViewModel();

            vm.Area = area;
            vm.Region = region;
            vm.Province = province;
            vm.WorkGroup = workGroup;

            vm.Areas = new List<string>
            {
                "All",
                "HQ",
                "REGION"
            };

            vm.Regions = _service.GetRegions(area) ?? new List<string>();
            if (!vm.Regions.Contains("All")) vm.Regions.Insert(0, "All");

            vm.Provinces = _service.GetProvinces(region) ?? new List<string>();
            if (!vm.Provinces.Contains("All")) vm.Provinces.Insert(0, "All");

            vm.WorkGroups = _service.GetWorkGroups(province) ?? new List<string>();
            if (!vm.WorkGroups.Contains("All")) vm.WorkGroups.Insert(0, "All");

            vm.Orders = _service.GetOrders(
                area,
                region,
                province,
                workGroup);

            vm.AllOrders = (area == "All" && region == "All" && province == "All" && workGroup == "All")
                ? vm.Orders
                : (_service.GetOrders("All", "All", "All", "All") ?? new List<WorkGroupOrder>());

            return View(vm);
        }
    }
}