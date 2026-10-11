using AlarmSystem.Models.Alarm;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class AlarmDetailsController : Controller
    {
        private readonly AlarmService _service;

        public AlarmDetailsController(AlarmService service)
        {
            _service = service;
        }

       public IActionResult Details(int id)
{
    AlarmDetailsViewModel model = new AlarmDetailsViewModel();

    model.Alarm = _service.GetAlarmDetails(id);

    if (model.Alarm == null)
    {
        return NotFound();
    }

    model.AutoEscalations = _service.GetAutoEscalations(id);

    model.SmsEscalations = _service.GetSmsEscalations(id);

    // Find linked ticket (if escalation already exists)
    model.TicketId = _service.TicketExists(
        model.Alarm.NODE,
        model.Alarm.FAULT);

    return View(model);
}
    }
}