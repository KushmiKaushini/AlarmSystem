using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class LinksController : Controller
    {
        // OPEN LINKS PAGE
        public IActionResult Index()
        {
            return View();
        }

        // GPS UPDATE
        public IActionResult GpsUpdate()
        {
            return Redirect("/GpsMain");
        }

        // POWER OASIS
        public IActionResult PowerOasis()
        {
            return Redirect("/PowerOasis2");
        }

        // DOWNLOAD ANDROID APP
        public IActionResult DownloadAndroidApp()
        {
            return PhysicalFile(
                Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot",
                "downloads",
                "SAT4.rar"),
                "application/octet-stream",
                "SAT4.rar");
        }
    }
}