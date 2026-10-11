using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class ViewsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}