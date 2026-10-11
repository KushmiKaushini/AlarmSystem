using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class CircuitIdController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string displayName)
        {
            ViewBag.DisplayName = displayName;

            // Temporary values
            ViewBag.CircuitId = "";
            ViewBag.AEnd = "";
            ViewBag.BEnd = "";

            return View();
        }
    }
}