using AlarmSystem.Models.History;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    public class HistoryController : Controller
    {
        private readonly HistoryService _service;

        public HistoryController(HistoryService service)
        {
            _service = service;
        }

        //========================================
        // HISTORY LIST
        //========================================
        public IActionResult Index(
            string ticketType = "FAULT",
            DateTime? from = null,
            DateTime? to = null)
        {
            if (from == null)
                from = DateTime.Today.AddDays(-30);

            if (to == null)
                to = DateTime.Today;

            var tickets = _service.GetHistoryTickets(
                from.Value,
                to.Value,
                ticketType);

            ViewBag.From = from.Value;
            ViewBag.To = to.Value;
            ViewBag.TicketType = ticketType;

            return View(tickets);
        }
        //========================================
// HISTORY TICKET DETAILS
//========================================
public IActionResult Details(long id)
{
    var ticket = _service.GetHistoryTicket(id);

    if (ticket == null)
    {
        return NotFound();
    }

    ViewBag.Attributes = _service.GetHistoryAttributes(id);
    ViewBag.Comments = _service.GetHistoryComments(id);
    ViewBag.Escalations = _service.GetHistoryEscalations(id);

    return View(ticket);
}

//========================================
// HISTORY ESCALATION DETAILS
//========================================
public IActionResult EscalationDetails(long escId)
{
    var primary = _service.GetHistoryPrimary(escId);

    if (primary == null)
    {
        return NotFound();
    }

    ViewBag.Secondary = _service.GetHistorySecondary(escId);

    return View(primary);
}
    }
}