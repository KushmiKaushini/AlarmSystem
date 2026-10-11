using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class ConfigController : Controller
    {
        private readonly ConfigService _configService;

        public ConfigController(ConfigService configService)
        {
            _configService = configService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "System Configuration";
            return View();
        }

        [HttpGet]
                        [HttpGet]
        public IActionResult GetEmployeeName(string serviceNo)
        {
            if (string.IsNullOrWhiteSpace(serviceNo))
                return Json(new { success = false, message = "Please enter a valid Service No." });

            var details = _configService.GetEmployeeDetailsByServiceNo(serviceNo);
            if (details != null)
            {
                string nameVal = "";
                string azureVal = "";
                string deptVal = "";
                try
                {
                    var propN = details.GetType().GetProperty("name");
                    if (propN != null) nameVal = propN.GetValue(details)?.ToString() ?? "";
                    var propA = details.GetType().GetProperty("azureAccount");
                    if (propA != null) azureVal = propA.GetValue(details)?.ToString() ?? "";
                    var propD = details.GetType().GetProperty("department");
                    if (propD != null) deptVal = propD.GetValue(details)?.ToString() ?? "";
                }
                catch { }

                if (string.IsNullOrWhiteSpace(nameVal))
                {
                    nameVal = _configService.SearchEmployeeByServiceNo(serviceNo);
                    azureVal = _configService.FormatAzureEmailHelper(nameVal, serviceNo);
                }

                return Json(new { success = true, name = nameVal, azureAccount = azureVal, department = deptVal, data = details });
            }

            var name = _configService.SearchEmployeeByServiceNo(serviceNo);
            if (!string.IsNullOrWhiteSpace(name))
            {
                var azure = _configService.FormatAzureEmailHelper(name, serviceNo);
                return Json(new { success = true, name = name, azureAccount = azure, data = new { name = name, serviceNo = serviceNo.Trim(), azureAccount = azure } });
            }

            return Json(new { success = false, message = "Employee record not found in Azure Cloud directory" });
        }

        [HttpGet]
        public IActionResult GetEmployeeDetails(string serviceNo)
        {
            if (string.IsNullOrWhiteSpace(serviceNo))
                return Json(new { success = false, message = "Please enter a valid Service No." });

            var details = _configService.GetEmployeeDetailsByServiceNo(serviceNo);
            if (details != null)
            {
                string nameVal = "";
                string azureVal = "";
                try
                {
                    var propN = details.GetType().GetProperty("name");
                    if (propN != null) nameVal = propN.GetValue(details)?.ToString() ?? "";
                    var propA = details.GetType().GetProperty("azureAccount");
                    if (propA != null) azureVal = propA.GetValue(details)?.ToString() ?? "";
                }
                catch { }

                return Json(new { success = true, name = nameVal, azureAccount = azureVal, data = details });
            }

            var name = _configService.SearchEmployeeByServiceNo(serviceNo);
            if (!string.IsNullOrWhiteSpace(name))
            {
                var azure = _configService.FormatAzureEmailHelper(name, serviceNo);
                return Json(new { success = true, name = name, azureAccount = azure, data = new { name = name, serviceNo = serviceNo.Trim(), azureAccount = azure } });
            }

            return Json(new { success = false, message = "Employee record not found in Azure Cloud directory" });
        }

        // --- SYS ADMIN ACTIONS ---

        public IActionResult SysAdmins()
        {
            ViewData["Title"] = "Sys Admins Management";
            var model = _configService.GetSysAdmins();
            return View(model);
        }

                [HttpGet]
        public IActionResult GetProfileEscalations(string profile)
        {
            if (string.IsNullOrWhiteSpace(profile))
            {
                var profiles = _configService.GetProfiles();
                if (profiles.Any()) profile = profiles.First().ProfileName;
            }

            var tiers = _configService.GetProfileEscalations(profile ?? "BROADBAND");
            return Json(new { success = true, profile, data = tiers });
        }

        public IActionResult Profiles()
        {
            ViewData["Title"] = "System Profiles";
            var model = _configService.GetProfiles();
            return View(model);
        }

        public IActionResult ProfileAdmins()
        {
            ViewData["Title"] = "Profile Admins";
            var model = _configService.GetProfileAdmins();
            return View(model);
        }

        public IActionResult AlarmTypeDelay()
        {
            ViewData["Title"] = "Alarm Type Delay Configuration";
            var model = _configService.GetAlarmTypeDelays();
            return View(model);
        }

        public IActionResult UpdateEscalationContacts()
        {
            ViewData["Title"] = "Update Escalation Contacts";
            var model = _configService.GetEscalationContacts();
            return View(model);
        }

        public IActionResult GpsAdmins()
        {
            ViewData["Title"] = "GPS Admins";
            var admins = _configService.GetGpsAdmins();
            var engineers = _configService.GetNwEngineers();
            ViewBag.Engineers = engineers;
            return View(admins);
        }

        public IActionResult PowerNocUserManagement()
        {
            ViewData["Title"] = "Power NOC User Management";
            var model = _configService.GetPowerNocUsers();
            return View(model);
        }

        public IActionResult ManageNwEngineers()
        {
            ViewData["Title"] = "Manage Network Engineers";
            var model = _configService.GetNwEngineers();
            return View(model);
        }

        // --- PROFILE ADMIN ACTIONS ---

                [HttpGet]
        public IActionResult GetConfigGroups(string profile = "")
        {
            var groups = _configService.GetConfigGroups(profile ?? "");
            return Json(new { success = true, data = groups });
        }

        [HttpGet]
        public IActionResult GetConfigGroupMembers(int groupId, string groupName = "")
        {
            var members = _configService.GetConfigGroupMembers(groupId, groupName ?? "");
            return Json(new { success = true, count = members.Count, data = members });
        }

        public IActionResult GroupsContacts()
        {
            ViewData["Title"] = "Groups & Contacts";
            var groups  = _configService.GetGroups();
            var members = _configService.GetMembers();
            var profiles = _configService.GetProfiles();
            ViewBag.Members  = members;
            ViewBag.Profiles = profiles;
            return View(groups);
        }

        public IActionResult ProfileUsers()
        {
            ViewData["Title"] = "Profile Users";
            var model = _configService.GetProfileUsers();
            return View(model);
        }

        public IActionResult SearchEmployeesByName()
        {
            ViewData["Title"] = "Search Employees by Name";
            var model = _configService.GetAzureCloudEmployeeDirectory();
            return View(model);
        }

        // --- USER ACTIONS ---

        public IActionResult ChangeProfile()
        {
            ViewData["Title"] = "Change Profile";
            var profiles = _configService.GetProfiles();
            ViewBag.Profiles = profiles;
            ViewBag.CurrentUser = _configService.GetCurrentAzureUser();
            return View();
        }

        public IActionResult SetDefaultProfile()
        {
            ViewData["Title"] = "Set Default Profile";
            var profiles = _configService.GetProfiles();
            ViewBag.Profiles = profiles;
            ViewBag.CurrentUser = _configService.GetCurrentAzureUser();
            return View();
        }

                [HttpGet]
        public IActionResult GetHierarchyGroups(string profile, int parentId = 0)
        {
            var groups = _configService.GetHierarchyGroups(profile ?? "ISP", parentId);
            return Json(new { success = true, profile, parentId, data = groups });
        }

        public IActionResult ViewGroupsContacts()
        {
            ViewData["Title"] = "View Groups & Contacts";
            var groups  = _configService.GetGroups();
            var members = _configService.GetMembers();
            ViewBag.Members = members;
            return View(groups);
        }
    }
}
