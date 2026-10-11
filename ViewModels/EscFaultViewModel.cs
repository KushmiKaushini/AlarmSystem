using AlarmSystem.Models;

namespace AlarmSystem.ViewModels
{
    public class EscFaultViewModel
    {
        public Ticket Ticket { get; set; }

        public List<TicketAttribute> Attributes { get; set; } = new();

        public List<Comment> Comments { get; set; } = new();

        public List<EscalationPrimary> Escalations { get; set; } = new();

        public List<EscalationSecondary> Members { get; set; } = new();

        public List<Group> Groups { get; set; } = new();

        public string NewComment { get; set; } = "";

        public string MessageText { get; set; } = "";

        public string EscalationLevel { get; set; } = "";

        public string EscalationTime { get; set; } = "";

        public long SelectedGroupId { get; set; }

        public List<Group> Level3Groups { get; set; } = new();

public List<Group> Level2Groups { get; set; } = new();

public List<Group> Level1Groups { get; set; } = new();

public List<Group> Level0Groups { get; set; } = new();

public long SelectedLevel3 { get; set; }

public long SelectedLevel2 { get; set; }

public long SelectedLevel1 { get; set; }

public long SelectedLevel0 { get; set; }
    }
}