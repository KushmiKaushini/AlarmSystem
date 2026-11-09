namespace AlarmSystem.Models.EmailReport
{
    /// <summary>
    /// Summary counts displayed in the Type 4 email summary mini-table.
    /// </summary>
    public class NetworkFailureSummary
    {
        public int MsanCount      { get; set; }
        public int CeaNodeCount   { get; set; }
        public int Slt4GSiteCount { get; set; }
    }
}
