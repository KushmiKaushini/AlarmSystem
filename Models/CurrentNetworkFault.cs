namespace AlarmSystem.Models
{
    public class CurrentNetworkFaultModel
    {
        public string Platform { get; set; } = "";

        public string Node { get; set; } = "";

        public string ResponsibleOfficer { get; set; } = "";

        public string Date { get; set; } = "";

        public string FaultOccuredAt { get; set; } = "";

        public string PendingHours { get; set; } = "";
    }
}