namespace AlarmSystem.Models.EmailReport
{
    /// <summary>
    /// Represents a single row in the Type 4 SLT 4G Sites down table.
    /// </summary>
    public class Slt4GSiteRecord
    {
        public int    No        { get; set; }
        public string Vendor    { get; set; } = string.Empty;
        public string Date      { get; set; } = string.Empty;
        public string Ne        { get; set; } = string.Empty;
        public string SubRegion { get; set; } = string.Empty;
        public string Reason    { get; set; } = string.Empty;
        public string NoOfDays  { get; set; } = string.Empty;
        public string AttendBy  { get; set; } = string.Empty;
    }
}
