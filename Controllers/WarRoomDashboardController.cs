using AlarmSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class WarRoomDashboardController : Controller
    {
        private readonly IWarRoomService _service;

        public WarRoomDashboardController(IWarRoomService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await _service.GetDashboardAsync();
            return View(vm);
        }
    }
}