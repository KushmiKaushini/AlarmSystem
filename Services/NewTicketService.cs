using AlarmSystem.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarmSystem.Services
{
    public class NewTicketService
    {
        public TicketCreateViewModel LoadPage()
        {
            TicketCreateViewModel vm = new TicketCreateViewModel();

            vm.TicketTypes = new List<SelectListItem>()
            {
                new SelectListItem{ Text="~SELECT~", Value="" },
                new SelectListItem{ Text="FAULT", Value="FAULT"},
                new SelectListItem{ Text="COMMON ISSUE", Value="COMMON ISSUE"},
                new SelectListItem{ Text="PLANNED EVENT", Value="PLANNED EVENT"},
                new SelectListItem{ Text="PROBLEM", Value="PROBLEM"},
            };

            vm.Platforms = new List<SelectListItem>()
{
    new SelectListItem{ Text="~SELECT~", Value="" },

    new SelectListItem{ Text="BROADBAND", Value="BROADBAND"},
    new SelectListItem{ Text="CDMA", Value="CDMA"},
    new SelectListItem{ Text="DATA CIRCUIT", Value="DATA CIRCUIT"},
    new SelectListItem{ Text="DATACOM", Value="DATACOM"},
    new SelectListItem{ Text="EHD", Value="EHD"},
    new SelectListItem{ Text="IPTV", Value="IPTV"},
    new SelectListItem{ Text="ISP", Value="ISP"},
    new SelectListItem{ Text="ITMC", Value="ITMC"},
    new SelectListItem{ Text="LTE", Value="LTE"},
    new SelectListItem{ Text="MSAN", Value="MSAN"},
    new SelectListItem{ Text="NTMC", Value="NTMC"},
    new SelectListItem{ Text="OTHER", Value="OTHER"},
    new SelectListItem{ Text="PO", Value="PO"},
    new SelectListItem{ Text="POWER", Value="POWER"},
    new SelectListItem{ Text="RPB", Value="RPB"},
    new SelectListItem{ Text="S200", Value="S200"},
    new SelectListItem{ Text="SAT", Value="SAT"},
    new SelectListItem{ Text="SLT NOC", Value="SLT NOC"},
    new SelectListItem{ Text="SOFTX", Value="SOFTX"},
    new SelectListItem{ Text="TX", Value="TX"},
    new SelectListItem{ Text="VPN", Value="VPN"},
    new SelectListItem{ Text="WIFI", Value="WIFI"}
};

            vm.Severities = new List<SelectListItem>()
            {
                new SelectListItem{ Text="CRITICAL", Value="CRITICAL"},
                new SelectListItem{ Text="MAJOR", Value="MAJOR"},
                new SelectListItem{ Text="MINOR", Value="MINOR"},
            };

            vm.ReportingPlatforms = new List<SelectListItem>()
            {
                new SelectListItem{ Text="~SELECT~", Value="" },
                new SelectListItem{ Text="OTHER", Value="OTHER"},
                new SelectListItem{ Text="NOC", Value="NOC"},
            };

            vm.ResponsibleOfficers = new List<SelectListItem>()
{
    new SelectListItem{ Text="~SELECT~", Value="" },
    new SelectListItem{ Text="BACK OFFICE", Value="BACK OFFICE"},
    new SelectListItem{ Text="BB NOC", Value="BB NOC"},
    new SelectListItem{ Text="CALL CENTER", Value="CALL CENTER"},
    new SelectListItem{ Text="CDMA NOC", Value="CDMA NOC"},
    new SelectListItem{ Text="CLARITY", Value="CLARITY"},
    new SelectListItem{ Text="DATACOM", Value="DATACOM"},
    new SelectListItem{ Text="EHD", Value="EHD"},
    new SelectListItem{ Text="IPTV NOC", Value="IPTV NOC"},
    new SelectListItem{ Text="ISP", Value="ISP"},
    new SelectListItem{ Text="ITMC", Value="ITMC"},
    new SelectListItem{ Text="LTE NOC", Value="LTE NOC"},
    new SelectListItem{ Text="NTMC", Value="NTMC"},
    new SelectListItem{ Text="OTHER", Value="OTHER"},
    new SelectListItem{ Text="POWER NOC", Value="POWER NOC"},
    new SelectListItem{ Text="SAT", Value="SAT"},
    new SelectListItem{ Text="SOFTX", Value="SOFTX"},
    new SelectListItem{ Text="VPN", Value="VPN"},
    new SelectListItem{ Text="WIFI NOC", Value="WIFI NOC"}
};

            return vm;
        }

        public long CreateTicket(TicketCreateViewModel vm)
        {
            return 0;
        }
    }
}