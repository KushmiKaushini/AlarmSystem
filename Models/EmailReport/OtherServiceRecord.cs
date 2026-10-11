namespace AlarmSystem.Models.EmailReport
{
    /// <summary>
    /// Represents one service category (MPLS, DATA, ISP, TX INT'L, IPTV) in the
    /// Type 4 Other Services combined table.
    /// When Rows is empty (or HasOutage is false), the template renders "No outage".
    /// </summary>
    public class OtherServiceRecord
    {
        public string                ServiceName { get; set; } = string.Empty;
        public bool                  HasOutage   { get; set; } = false;
        public List<NetworkFaultRow> Rows        { get; set; } = new();
    }
}
