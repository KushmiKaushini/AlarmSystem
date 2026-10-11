namespace AlarmSystem.Models.EmailReport
{
    /// <summary>
    /// Represents a single row in the Type 2 – Pending OPMC Faults email table.
    /// </summary>
    public class OpmcFaultReport
    {
        public long   No             { get; set; }
        public string ReportedDate   { get; set; } = string.Empty;
        public string CircuitName    { get; set; } = string.Empty;
        public string CustomerName   { get; set; } = string.Empty;
        public string OutageTime     { get; set; } = string.Empty;
        public string Lea            { get; set; } = string.Empty;
        public string RelevantOpmc   { get; set; } = string.Empty;
        public string Dgm            { get; set; } = string.Empty;
        public string Gm             { get; set; } = string.Empty;
        public string RecentUpdate   { get; set; } = string.Empty;
    }
}
