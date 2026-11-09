namespace AlarmSystem.Models.EmailReport
{
    /// <summary>
    /// Represents a single row in the Type 4 MSAN outage table.
    /// </summary>
    public class MsanFaultRecord
    {
        public string Network               { get; set; } = string.Empty;
        public string Description           { get; set; } = string.Empty;
        public string Engineer              { get; set; } = string.Empty;
        public string Province              { get; set; } = string.Empty;
        public string Region                { get; set; } = string.Empty;
        public string ResponsibleGroup      { get; set; } = string.Empty;
        public string Remarks               { get; set; } = string.Empty;
        public string FaultOccurredDateTime { get; set; } = string.Empty;
        public string PendingHours          { get; set; } = string.Empty;
    }
}
