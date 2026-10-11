using AlarmSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlarmSystem.ViewModels
{
    public class TicketCreateViewModel
    {
        // Basic Details
        public string TicketType { get; set; }
        public string Node { get; set; }
        public string Platform { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public string ReportingPlatform { get; set; }

        // Tags
        public List<string> SelectedTags { get; set; } = new();

        // Dates
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // Responsible Officer
        public string ResponsibleOfficer { get; set; }

        // Selected Escalation Groups
        public long? Level3Id { get; set; }
        public long? Level2Id { get; set; }
        public long? Level1Id { get; set; }

        // Escalation Lists
        public List<EscalationGroup> Level3Groups { get; set; } = new();
        public List<EscalationGroup> Level2Groups { get; set; } = new();
        public List<EscalationGroup> Level1Groups { get; set; } = new();

        // Dropdown Lists
        public List<SelectListItem> TicketTypes { get; set; } = new();
        public List<SelectListItem> Platforms { get; set; } = new();
        public List<SelectListItem> Severities { get; set; } = new();
        public List<SelectListItem> ReportingPlatforms { get; set; } = new();

        // Tags
        public List<SelectListItem> Tags { get; set; } = new();

        public List<SelectListItem> ResponsibleOfficers { get; set; } = new();
    }
}