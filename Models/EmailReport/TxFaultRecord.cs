namespace AlarmSystem.Models.EmailReport
{
    /// <summary>
    /// Represents a single row in the Type 4 TX fault table.
    /// Also used as a row type inside OtherServiceRecord for MPLS/DATA/ISP/TX INT'L/IPTV.
    /// </summary>
    public class TxFaultRecord
    {
        public string Network          { get; set; } = string.Empty;
        public string Description      { get; set; } = string.Empty;
        public string Engineer         { get; set; } = string.Empty;
        public string Province         { get; set; } = string.Empty;
        public string Region           { get; set; } = string.Empty;
        public string ResponsibleGroup { get; set; } = string.Empty;
        public string Remarks          { get; set; } = string.Empty;
        public string FaultOccurredDate{ get; set; } = string.Empty;
        public string FaultOccurredTime{ get; set; } = string.Empty;
        public string PendingHours     { get; set; } = string.Empty;
    }
}
