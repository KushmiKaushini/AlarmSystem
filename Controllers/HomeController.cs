using System.Diagnostics;
using AlarmSystem.Models;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;
using AlarmSystem.Models.ViewModels;

namespace AlarmSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly FaultService _faultService;

        public HomeController(FaultService faultService)
        {
            _faultService = faultService;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        public IActionResult OtherOperations()
        {
            ViewData["BodyClass"] = "dashboard-bg";
            ViewData["DashboardType"] = AppRoles.Admin;
            return View();
        }

        public IActionResult DashboardEntry(string? justLoggedIn)
        {
            if (!string.IsNullOrWhiteSpace(justLoggedIn))
            {
                TempData["ToastMessage"] = "Welcome back!";
                return RedirectToAction(nameof(Dashboard));
            }

            return RedirectToAction(nameof(Dashboard));
        }

        public IActionResult Dashboard()
        {
            ViewData["BodyClass"] = "dashboard-bg";
            ViewData["DashboardType"] = AppRoles.Admin;
            return View();
        }

        public IActionResult AlarmDashboard(
            DateTime? from,
            DateTime? to)
        {
            ViewData["BodyClass"] = "dashboard-bg";

            // Default: show from start of this year so data is visible
            from ??= new DateTime(DateTime.Today.Year, 1, 1);
            to   ??= DateTime.Today;

            ViewBag.From = from.Value;
            ViewBag.To   = to.Value;

            var nodes = _faultService.GetMapNodes(
                from.Value,
                to.Value.AddDays(1));

            return View(nodes);
        }

        public IActionResult ViewerDashboard()
        {
            ViewData["BodyClass"] = "dashboard-bg";
            ViewData["DashboardType"] = AppRoles.Viewer;
            return View("Dashboard");
        }

        public IActionResult CustomerFaults()
        {
            ViewData["BodyClass"] = "dashboard-bg";
            return View();
        }

        public IActionResult ApiTester()
        {
            ViewData["Title"] = "Starlink";
            ViewData["BodyClass"] = "dashboard-bg api-tester-page";
            return View();
        }

        [ActionName("ViewData")]
        public IActionResult ViewDataPage()
        {
            ViewData["BodyClass"] = "dashboard-bg";
            return View();
        }

        public IActionResult RealtimeAlarm()
        {
            return RenderViewDataSection(
                title: "Realtime Alarm",
                sectionKey: "realtime-alarms",
                description: "Live network alarm records from the realtime feed, grouped for fast review.",
                badge: "Live alarms");
        }

        public IActionResult CustomerFaultOss()
        {
            return RenderViewDataSection(
                title: "Customer Fault (OSS)",
                sectionKey: "customer-fault-oss",
                description: "OSS customer-fault records with CR, circuit, customer, and status details.",
                badge: "OSS faults");
        }

        public IActionResult CustomerDetails()
        {
            return RenderViewDataSection(
                title: "Customer Details",
                sectionKey: "customer-details",
                description: "Customer-oriented records derived from the coordinate table, separated from network element data.",
                badge: "Coordinate data");
        }

        public IActionResult NetworkElementDetails()
        {
            return RenderViewDataSection(
                title: "Network Element Details",
                sectionKey: "network-element-details",
                description: "Network element records from the coordinate table, including node, IP, and coordinate metadata.",
                badge: "Element data");
        }

        public IActionResult UserManagement()
        {
            ViewData["BodyClass"] = "dashboard-bg";
            return View();
        }

        private IActionResult RenderViewDataSection(string title, string sectionKey, string description, string badge)
        {
            ViewData["BodyClass"] = "dashboard-bg";
            ViewData["DataSectionTitle"] = title;
            ViewData["DataSectionKey"] = sectionKey;
            ViewData["DataSectionDescription"] = description;
            ViewData["DataSectionBadge"] = badge;

            return View("ViewDataSection");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}