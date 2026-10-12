using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class SearchByNodeController : Controller
    {
        private readonly SearchNodeService _service;

        public SearchByNodeController(SearchNodeService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string node)
        {
            var data = await _service.SearchAsync(node ?? "");

            ViewBag.Node = node;
            return View(data);
        }
    }
}