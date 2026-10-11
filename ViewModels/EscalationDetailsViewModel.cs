using AlarmSystem.Models;

namespace AlarmSystem.ViewModels
{
    public class EscalationDetailsViewModel
    {
        // Current Escalation
        public EscalationPrimary Primary { get; set; }

        // Secondary Escalation list
        public List<EscalationSecondary> SecondaryList { get; set; } = new();

        // Ticket details
        public Ticket Ticket { get; set; }

        // Attributes section
        public List<TicketAttribute> Attributes { get; set; } = new();

        // Comments section
        public List<Comment> Comments { get; set; } = new();

        // Escalation history section
        public List<EscalationPrimary> Escalations { get; set; } = new();

        // Level groups
public List<Group> Level3Groups { get; set; } = new();

public List<Group> Level2Groups { get; set; } = new();

public List<Group> Level1Groups { get; set; } = new();

public List<Group> Level0Groups { get; set; } = new();
public string MessageText { get; set; }

public string EscalationLevel { get; set; }

public long? EditAttributeId { get; set; }

public string NewAttributeValue { get; set; }

    }
}