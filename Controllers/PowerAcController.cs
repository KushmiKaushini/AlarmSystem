using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers;

public class PowerAcController : Controller
{
    private readonly PowerAcService _service;

    public PowerAcController(PowerAcService service)
    {
        _service = service;
    }

    public IActionResult Index()
    {
        ViewBag.PageTitle = "POWER & AC";

        var data = _service.GetAlarms();

        return View(data);
    }
}