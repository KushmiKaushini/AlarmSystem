using AlarmSystem.Models;
using AlarmSystem.Models.ViewModels;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarmSystem.Controllers
{
    public class AlarmHistoryEngineerController : Controller
    {
        private readonly IAlarmHistoryEngineerService _service;

        public AlarmHistoryEngineerController(IAlarmHistoryEngineerService service)
        {
            _service = service;
        }

       public IActionResult Index()
{
    return View();
}

        #region MSAN

        public async Task<IActionResult> Msan(
            DateTime? from,
            DateTime? to,
            string engineer)
        {
            AlarmHistoryEngineerViewModel vm = new AlarmHistoryEngineerViewModel
            {
                From = from,
                To = to,
                NetworkEngineer = engineer
            };

            vm.Engineers = (await _service.GetEngineersAsync())
                .Select(x => new SelectListItem
                {
                    Text = x,
                    Value = x
                }).ToList();

            vm.Records = await _service.GetMsanAsync(
                from,
                to,
                engineer);

            return View(vm);
        }

        #endregion

        #region ALL

        public async Task<IActionResult> All(
            DateTime? from,
            DateTime? to,
            string platform,
            string engineer)
        {
            AlarmHistoryEngineerViewModel vm = new AlarmHistoryEngineerViewModel
            {
                From = from,
                To = to,
                Platform = platform,
                NetworkEngineer = engineer
            };

            vm.Platforms = (await _service.GetPlatformsAsync())
                .Select(x => new SelectListItem
                {
                    Text = x,
                    Value = x
                }).ToList();

            vm.Engineers = (await _service.GetEngineersAsync())
                .Select(x => new SelectListItem
                {
                    Text = x,
                    Value = x
                }).ToList();

            vm.Records = await _service.GetAllAsync(
                from,
                to,
                platform,
                engineer);

            return View(vm);
        }

        #endregion

        #region DETAILS

        public IActionResult Details(int id)
        {
            AlarmHistoryEngineer model = new AlarmHistoryEngineer();

            // TODO:
            // Load the record using ID from the database

            return View(model);
        }

        #endregion

        #region EXPORT

        public IActionResult Export(
            DateTime? from,
            DateTime? to,
            string platform,
            string engineer)
        {
            // TODO:
            // Export to Excel after stored procedure conversion

            return Content("Export functionality will be implemented.");
        }

        #endregion
    }
}