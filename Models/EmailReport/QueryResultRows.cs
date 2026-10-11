namespace AlarmSystem.Models.EmailReport
{
    /// <summary>
    /// Maps the result of the Type 1 / Type 3 SELECT query
    /// (CLARITY_WO_STATUS or CLARITY_DATA_NW_FAULTS rows).
    /// </summary>
    public class DataFaultRow
    {
        public long   No                  { get; set; }
        public string DataCircuitId       { get; set; } = string.Empty;
        public string DetailsOfCustomer   { get; set; } = string.Empty;
        public string FaultDate           { get; set; } = string.Empty;
        public string FaultTime           { get; set; } = string.Empty;
        public string WorkOrdersHeldUpAt  { get; set; } = string.Empty;
        public string Description         { get; set; } = string.Empty;
        public string DurationOfComplaint { get; set; } = string.Empty;
        // extras stored alongside (useful for routing/debugging)
        public string Region              { get; set; } = string.Empty;
        public string Province            { get; set; } = string.Empty;
    }

    /// <summary>Maps the Type 2 OPMC query result.</summary>
    public class OpmcFaultRow
    {
        public long   No            { get; set; }
        public string ReportedDate  { get; set; } = string.Empty;
        public string CircuitName   { get; set; } = string.Empty;
        public string CustomerName  { get; set; } = string.Empty;
        public string OutageTime    { get; set; } = string.Empty;
        public string Lea           { get; set; } = string.Empty;
        public string RelevantOpmc  { get; set; } = string.Empty;
        public string Dgm           { get; set; } = string.Empty;
        public string Gm            { get; set; } = string.Empty;
        public string RecentUpdate  { get; set; } = string.Empty;
    }

    /// <summary>Maps the Type 4-A summary COUNT query (single row).</summary>
    public class SummaryCountRow
    {
        public int MsanCount      { get; set; }
        public int CeaNodeCount   { get; set; }
        public int Slt4GSiteCount { get; set; }
    }

    /// <summary>Maps the Type 4-B MSAN query result rows.</summary>
    public class MsanRow
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

    /// <summary>Maps the Type 4-C SLT 4G Sites query result rows.</summary>
    public class LteRow
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

    /// <summary>Maps both Type 4-D (TX) and Type 4-E (Other Services) query rows.</summary>
    public class NetworkFaultRow
    {
        public string Network           { get; set; } = string.Empty;
        public string Description       { get; set; } = string.Empty;
        public string Engineer          { get; set; } = string.Empty;
        public string Province          { get; set; } = string.Empty;
        public string Region            { get; set; } = string.Empty;
        public string ResponsibleGroup  { get; set; } = string.Empty;
        public string Remarks           { get; set; } = string.Empty;
        public string FaultOccurredDate { get; set; } = string.Empty;
        public string FaultOccurredTime { get; set; } = string.Empty;
        public string PendingHours      { get; set; } = string.Empty;
    }

    // ======================
    // COMPREHENSIVE TYPE 4 VIEWMODELS (matching user specification)
    // ======================

    /// <summary>
    /// Represents a row in the MSAN NETWORK FAILURES table (Table 1)
    /// </summary>
    public class MsanNetworkFailureRow
    {
        /// <summary>Row number</summary>
        public int No { get; set; }

        /// <summary>Network platform (e.g., MSAN)</summary>
        public string Network { get; set; } = string.Empty;

        /// <summary>Node name/device identifier</summary>
        public string Node { get; set; } = string.Empty;

        /// <summary>Cleaned description/remarks (stripped of leading date/time stamps, etc.)</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Network engineer responsible</summary>
        public string NetworkEngineer { get; set; } = string.Empty;

        /// <summary>Province</summary>
        public string Province { get; set; } = string.Empty;

        /// <summary>Region</summary>
        public string Region { get; set; } = string.Empty;

        /// <summary>Fault detected timestamp</summary>
        public DateTime FaultDetected { get; set; }

        /// <summary>Pending hours (calculated as difference between FaultDetected and now)</summary>
        public int PendingHours { get; set; }

        /// <summary>Color code (GREEN, PINK, YELLOW, WHITE) based on pending hours and fault status</summary>
        public string ColorCode { get; set; } = "WHITE";

        /// <summary>Background color for the row (derived from ColorCode or set directly)</summary>
        public string BackgroundColor { get; set; } = "#FFFFFF";
    }

    /// <summary>
    /// Represents a row in the SLT 4G SITES DOWN table (Table 2)
    /// </summary>
    public class Slt4GSitesDownRow
    {
        /// <summary>Row number</summary>
        public int No { get; set; }

        /// <summary>Vendor equipment manufacturer</summary>
        public string Vendor { get; set; } = string.Empty;

        /// <summary>Date fault was detected</summary>
        public string Date { get; set; } = string.Empty;

        /// <summary>Network Element identifier</summary>
        public string Ne { get; set; } = string.Empty;

        /// <summary>Sub-region or location details</summary>
        public string SubRegion { get; set; } = string.Empty;

        /// <summary>Reason for site being down</summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>Number of days since fault detection</summary>
        public int NoOfDays { get; set; }

        /// <summary>Attending technician or team</summary>
        public string AttendBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a row in the standard technology tables (Tables 3-8: TX, MPLS, DATA, ISP, TX INT'L, IPTV)
    /// </summary>
    public class StandardTechnologyRow
    {
        /// <summary>Row number</summary>
        public int No { get; set; }

        /// <summary>Technology type (TX, MPLS, DATA, ISP, TX INT'L, IPTV)</summary>
        public string Technology { get; set; } = string.Empty;

        /// <summary>Node name/device identifier</summary>
        public string Node { get; set; } = string.Empty;

        /// <summary>Description of the fault</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Network engineer responsible</summary>
        public string NetworkEngineer { get; set; } = string.Empty;

        /// <summary>Province</summary>
        public string Province { get; set; } = string.Empty;

        /// <summary>Region</summary>
        public string Region { get; set; } = string.Empty;

        /// <summary>Fault date</summary>
        public string FaultDate { get; set; } = string.Empty;

        /// <summary>Fault time</summary>
        /// <summary>
        /// Pending hours (calculated as difference between fault time and now)
        /// </summary>
        public int PendingHours { get; set; }
    }

    /// <summary>
    /// Represents a row in the TRANSMISSION (TX) NETWORK FAILURES table (Table 3)
    /// </summary>
    public class TxNetworkFailureRow
    {
        /// <summary>Row number</summary>
        public int No { get; set; }

        /// <summary>Network platform (e.g., TX, TRANSMISSION, DWDM, SDH)</summary>
        public string Network { get; set; } = string.Empty;

        /// <summary>Cleaned description/remarks</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Network engineer responsible</summary>
        public string Engineer { get; set; } = string.Empty;

        /// <summary>Province</summary>
        public string Province { get; set; } = string.Empty;

        /// <summary>Region</summary>
        public string Region { get; set; } = string.Empty;

        /// <summary>Responsible group (DCRO)</summary>
        public string ResponsibleGroup { get; set; } = string.Empty;

        /// <summary>Remarks (cleaned status comment)</summary>
        public string Remarks { get; set; } = string.Empty;

        /// <summary>Fault occurred date</summary>
        public string FaultOccurredDate { get; set; } = string.Empty;

        /// <summary>Fault occurred time</summary>
        public string FaultTime { get; set; } = string.Empty;

        /// <summary>Pending hours</summary>
        public int PendingHours { get; set; }
    }
}
