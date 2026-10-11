using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using AlarmSystem.Models;
using AlarmSystem.Models.EmailReport;

namespace AlarmSystem.Services
{
    /// <summary>
    /// Fetches live operational fault records from SQL Server and maps them into
    /// clean, responsive, inline-styled HTML daily email report templates for SLT NOC.
    /// Supports Types 1, 2, 3, and 4 with dynamic real-time SQL querying, full column mapping,
    /// and fallback empty-state handling.
    /// </summary>
    public class FaultReportService
    {
        private readonly string _connectionString;

        // ── Shared Inline CSS Styles ───────────────────────────────────────────
        private const string FontBase   = "font-family:Arial,Helvetica,sans-serif;font-size:12px;color:#1a1a1a;";
        private const string ThStyle    = "background-color:#1a3c5e;color:#ffffff;font-size:11px;padding:6px 8px;text-align:center;border:1px solid #cccccc;";
        private const string TdStyle    = "padding:6px 8px;border:1px solid #cccccc;vertical-align:top;";
        private const string TableStyle = "border-collapse:collapse;width:100%;font-family:Arial,sans-serif;font-size:12px;margin-bottom:16px;";

        // ── Standard SLT NOC Signature (Types 1, 2, 3) ─────────────────────────
                private static readonly string StandardFooter = @"
                    <br>
                    <p style=""margin:4px 0;"">Best Regards,</p>
                    <p style=""margin:4px 0;""><b>SLT NOC – Service Assurance Team</b></p>
                    <hr style=""border:none;border-top:1px solid #cccccc;margin:10px 0;"">
                    <table cellpadding=""0"" cellspacing=""0"" style=""font-family:Arial,Helvetica,sans-serif;font-size:11px;color:#444;"">
                        <tr>
                            <td style=""padding-right:14px;vertical-align:top;"">
                                <p style=""margin:2px 0;""><b>Sri Lanka Telecom PLC</b></p>
                                <p style=""margin:2px 0;"">6<sup>th</sup> Floor, OTS Building, Lotus Road, Colombo 01, Sri Lanka</p>
                                <p style=""margin:2px 0;"">Email: <a href=""mailto:sateam@slt.com.lk"" style=""color:#1a3c5e;"">sateam@slt.com.lk</a></p>
                                <p style=""margin:2px 0;"">Tel: +94 11 202 9595&nbsp;&nbsp;|&nbsp;&nbsp;Fax: +94 11 254 3087</p>
                            </td>
                        </tr>
                    </table>";

                public FaultReportService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TMS") ?? string.Empty;
        }

        private static string GetString(SqlDataReader r, string colName, string defaultValue = "N/A")
        {
            if (r[colName] == DBNull.Value) return defaultValue;
            var val = r[colName]?.ToString();
            return string.IsNullOrWhiteSpace(val) ? defaultValue : val.Trim();
        }

        private static int GetInt32(SqlDataReader r, string colName, int defaultValue = 0)
        {
            if (r[colName] == DBNull.Value) return defaultValue;
            return Convert.ToInt32(r[colName]);
        }

        private static long GetInt64(SqlDataReader r, string colName, long defaultValue = 0)
        {
            if (r[colName] == DBNull.Value) return defaultValue;
            return Convert.ToInt64(r[colName]);
        }

        private static DateTime GetDateTime(SqlDataReader r, string colName, DateTime defaultValue = default)
        {
            if (r[colName] == DBNull.Value) return defaultValue;
            return Convert.ToDateTime(r[colName]);
        }

        // ────────────────────────────────────────────────────────────────────────
        // MASTER PREVIEW ENTRY POINT (Parameterized by Type & Optional Target Date)
        // ────────────────────────────────────────────────────────────────────────
        public async Task<string> GetEmailPreviewHtmlAsync(int type, DateTime? targetDate = null)
        {
            return type switch
            {
                1 => await GenerateType1EmailAsync(targetDate),
                2 => await GenerateType2EmailAsync(targetDate),
                3 => await GenerateType3EmailAsync(targetDate),
                4 => await GenerateType4ComprehensiveEmailAsync(targetDate),
                _ => await GenerateType1EmailAsync(targetDate)
            };
        }

        // ────────────────────────────────────────────────────────────────────────
        // TYPE 1: Regional Network Teams (DataFaultRow Mapping)
        // ────────────────────────────────────────────────────────────────────────
        public async Task<List<DataFaultRow>> GetType1RegionalFaultsAsync(DateTime? targetDate = null)
        {
            var rows = new List<DataFaultRow>();

            const string query = @"
                SELECT TOP 100
                    ROW_NUMBER() OVER(ORDER BY ISNULL(w.STARTED_TIME, f.reportedtime) DESC) AS No,
                    ISNULL(NULLIF(f.circuit_name, ''), ISNULL(NULLIF(w.CIRCUIT, ''), 'N/A')) AS DataCircuitId,
                    ISNULL(NULLIF(f.customer_name, ''), ISNULL(NULLIF(w.CUSTOMER, ''), 'N/A')) AS DetailsOfCustomer,
                    CONVERT(varchar(12), ISNULL(w.STARTED_TIME, f.reportedtime), 101) AS FaultDate,
                    CONVERT(varchar(8),  ISNULL(w.STARTED_TIME, f.reportedtime), 108) AS FaultTime,
                    ISNULL(NULLIF(w.WORK_GROUP, ''), ISNULL(NULLIF(f.workgroup, ''), 'N/A')) AS WorkOrdersHeldUpAt,
                    ISNULL(NULLIF(f.description, ''), ISNULL(NULLIF(w.CIRCUIT_TYPE, ''), 'N/A')) AS Description,
                    CASE
                        WHEN ISNULL(w.STARTED_TIME, f.reportedtime) IS NULL THEN '00:00:00'
                        WHEN DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) < 0 THEN '00:00:00'
                        ELSE
                            CAST(DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) / 3600 AS VARCHAR) + ':' +
                            RIGHT('00' + CAST((DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) % 3600) / 60 AS VARCHAR), 2) + ':' +
                            RIGHT('00' + CAST(DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) % 60 AS VARCHAR), 2)
                    END AS DurationOfComplaint,
                    ISNULL(NULLIF(w.REGION, ''), 'N/A') AS Region,
                    ISNULL(NULLIF(w.PROVINCE, ''), 'N/A') AS Province
                FROM dbo.CLARITY_WO_STATUS w
                LEFT JOIN dbo.CLARITY_DATA_NW_FAULTS f ON f.problem_no = w.CLARITY_REF
                WHERE (
                    w.WO_STATUS IN ('OPEN', 'PENDING', 'IN PROGRESS', 'INPROGRESS', 'ASSIGNED', 'ACKNOWLEDGED', 'Acknowledged', 'Open', 'Pending', 'In Progress')
                    OR (UPPER(ISNULL(w.WO_STATUS, '')) NOT IN ('CLOSED', 'RESOLVED', 'CANCELLED') AND ISNULL(w.WO_STATUS, '') != '')
                    OR w.WO_STATUS IS NULL
                )
                AND (
                    f.problem_status IS NULL
                    OR UPPER(f.problem_status) NOT IN ('CLOSED', 'RESOLVED', 'CANCELLED')
                )
                AND (@TargetDate IS NULL OR CAST(ISNULL(w.STARTED_TIME, f.reportedtime) AS DATE) <= CAST(@TargetDate AS DATE))
                ORDER BY ISNULL(w.STARTED_TIME, f.reportedtime) DESC";

            Console.WriteLine($"[FaultReportService] Executing Type 1 SQL Query (TargetDate: {targetDate?.ToString("yyyy-MM-dd") ?? "REAL-TIME LIVE"})...");

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd  = new SqlCommand(query, conn);
                cmd.Parameters.Add("@TargetDate", SqlDbType.Date).Value = (object?)targetDate ?? DBNull.Value;

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rows.Add(new DataFaultRow
                    {
                        No                  = GetInt64(reader, "No"),
                        DataCircuitId       = GetString(reader, "DataCircuitId", "N/A"),
                        DetailsOfCustomer   = GetString(reader, "DetailsOfCustomer", "N/A"),
                        FaultDate           = GetString(reader, "FaultDate", "N/A"),
                        FaultTime           = GetString(reader, "FaultTime", "N/A"),
                        WorkOrdersHeldUpAt  = GetString(reader, "WorkOrdersHeldUpAt", "N/A"),
                        Description         = GetString(reader, "Description", "N/A"),
                        DurationOfComplaint = GetString(reader, "DurationOfComplaint", "00:00:00"),
                        Region              = GetString(reader, "Region", "N/A"),
                        Province            = GetString(reader, "Province", "N/A")
                    });
                }
                Console.WriteLine($"[FaultReportService] Type 1 successfully retrieved {rows.Count} rows.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FaultReportService] ERROR in Type 1 query: {ex.Message}");
            }

            return rows;
        }

        private async Task<string> GenerateType1EmailAsync(DateTime? targetDate)
        {
            var rows = await GetType1RegionalFaultsAsync(targetDate);
            string dateStr = (targetDate ?? DateTime.Today).ToString("dd MMM yyyy");

            var sb = new StringBuilder();
            if (rows.Count == 0)
            {
                sb.Append(@"<tr><td colspan=""8"" class=""no-data"">No pending faults found.</td></tr>");
            }
            else
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var item = rows[i];
                    string bg = i % 2 == 0 ? "#ffffff" : "#f5f5f5";
                    sb.Append($@"
                    <tr style=""background-color:{bg};"">
                        <td style=""{TdStyle}text-align:center;"">{item.No}</td>
                        <td style=""{TdStyle}font-weight:bold;"">{System.Net.WebUtility.HtmlEncode(item.DataCircuitId)}</td>
                        <td style=""{TdStyle}text-align:left;"">{System.Net.WebUtility.HtmlEncode(item.DetailsOfCustomer)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.FaultDate)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.FaultTime)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.WorkOrdersHeldUpAt)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.Description)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.DurationOfComplaint)}</td>
                    </tr>");
                }
            }

            string inner = $@"
            <p style=""font-size:14px;"">Dear All,</p>
            <p>Pending Data Faults related to Regional Network Teams on <b>{dateStr}</b> are as follows.</p>

            <table style=""{TableStyle}"">
                {DataCircuitTableHeader()}
                {sb}
            </table>

            <p>Best Regards,<br>
               <b>Lalith Perera</b><br>
               <span style=""color:#1a6e2e;font-weight:bold;"">SLT NOC – Service Assurance Team</span>
            </p>
            <hr style=""border:none;border-top:1px solid #cccccc;margin:10px 0;"">
            <table cellpadding=""0"" cellspacing=""0"" style=""font-family:Arial,Helvetica,sans-serif;font-size:11px;color:#444;"">
                <tr>
                    <td>
                        <p style=""margin:2px 0;""><b>Sri Lanka Telecom PLC</b></p>
                        <p style=""margin:2px 0;"">6<sup>th</sup> Floor, OTS Building, Lotus Road, Colombo 01, Sri Lanka</p>
                        <p style=""margin:2px 0;"">Email: <a href=""mailto:sateam@slt.com.lk"" style=""color:#1a3c5e;"">sateam@slt.com.lk</a>&nbsp;&nbsp;|&nbsp;&nbsp;Tel: +94 11 202 9595</p>
                    </td>
                </tr>
            </table>";

            return WrapDocument(inner);
        }

        // ────────────────────────────────────────────────────────────────────────
        // TYPE 2: OPMC Faults (OpmcFaultRow Mapping)
        // ────────────────────────────────────────────────────────────────────────
        public async Task<List<OpmcFaultRow>> GetType2OpmcFaultsAsync(DateTime? targetDate = null)
        {
            var rows = new List<OpmcFaultRow>();

            const string query = @"
                SELECT TOP 100
                    ROW_NUMBER() OVER(ORDER BY ISNULL(w.STARTED_TIME, f.reportedtime) DESC) AS No,
                    CONVERT(varchar(20), ISNULL(w.STARTED_TIME, f.reportedtime), 120) AS ReportedDate,
                    ISNULL(NULLIF(f.circuit_name, ''), ISNULL(NULLIF(w.CIRCUIT, ''), 'N/A')) AS CircuitName,
                    ISNULL(NULLIF(f.customer_name, ''), ISNULL(NULLIF(w.CUSTOMER, ''), 'N/A')) AS CustomerName,
                    CASE
                        WHEN ISNULL(w.STARTED_TIME, f.reportedtime) IS NULL THEN '00:00'
                        WHEN DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) < 0 THEN '00:00'
                        ELSE
                            CAST(DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) / 3600 AS VARCHAR) + ':' +
                            RIGHT('00' + CAST((DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) % 3600) / 60 AS VARCHAR), 2)
                    END AS OutageTime,
                    ISNULL(NULLIF(lm.LEA, ''), ISNULL(NULLIF(w.AREA, ''), 'N/A')) AS Lea,
                    ISNULL(NULLIF(lm.OPMC, ''), 'N/A') AS RelevantOpmc,
                    ISNULL(NULLIF(w.OPMC_NW, ''), 'N/A') AS Dgm,
                    ISNULL(NULLIF(w.REGION, ''), 'N/A') AS Gm,
                    ISNULL((SELECT TOP 1 cc.description
                            FROM dbo.CLARITY_COMMENTS cc
                            WHERE cc.docket_id = CAST(w.CLARITY_REF AS VARCHAR)
                            ORDER BY cc.event_time DESC), 'No update') AS RecentUpdate
                FROM dbo.CLARITY_WO_STATUS w
                LEFT JOIN dbo.CLARITY_DATA_NW_FAULTS f ON f.problem_no = w.CLARITY_REF
                LEFT JOIN dbo.LEA_MAPPING lm ON lm.RTOM = w.AREA
                WHERE (
                    w.WO_STATUS IN ('OPEN', 'PENDING', 'IN PROGRESS', 'INPROGRESS', 'ASSIGNED', 'ACKNOWLEDGED', 'Acknowledged', 'Open', 'Pending', 'In Progress')
                    OR (UPPER(ISNULL(w.WO_STATUS, '')) NOT IN ('CLOSED', 'RESOLVED', 'CANCELLED') AND ISNULL(w.WO_STATUS, '') != '')
                    OR w.WO_STATUS IS NULL
                )
                AND (w.OPMC_NW = 'OPMC' OR lm.OPMC IS NOT NULL OR w.AREA IS NOT NULL)
                AND (@TargetDate IS NULL OR CAST(ISNULL(w.STARTED_TIME, f.reportedtime) AS DATE) <= CAST(@TargetDate AS DATE))
                ORDER BY ISNULL(w.STARTED_TIME, f.reportedtime) DESC";

            Console.WriteLine($"[FaultReportService] Executing Type 2 SQL Query (TargetDate: {targetDate?.ToString("yyyy-MM-dd") ?? "REAL-TIME LIVE"})...");

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd  = new SqlCommand(query, conn);
                cmd.Parameters.Add("@TargetDate", SqlDbType.Date).Value = (object?)targetDate ?? DBNull.Value;

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rows.Add(new OpmcFaultRow
                    {
                        No           = GetInt64(reader, "No"),
                        ReportedDate = GetString(reader, "ReportedDate", "N/A"),
                        CircuitName  = GetString(reader, "CircuitName", "N/A"),
                        CustomerName = GetString(reader, "CustomerName", "N/A"),
                        OutageTime   = GetString(reader, "OutageTime", "00:00"),
                        Lea          = GetString(reader, "Lea", "N/A"),
                        RelevantOpmc = GetString(reader, "RelevantOpmc", "N/A"),
                        Dgm          = GetString(reader, "Dgm", "N/A"),
                        Gm           = GetString(reader, "Gm", "N/A"),
                        RecentUpdate = GetString(reader, "RecentUpdate", "No update")
                    });
                }
                Console.WriteLine($"[FaultReportService] Type 2 successfully retrieved {rows.Count} rows.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FaultReportService] ERROR in Type 2 query: {ex.Message}");
            }

            return rows;
        }

        private async Task<string> GenerateType2EmailAsync(DateTime? targetDate)
        {
            var records = await GetType2OpmcFaultsAsync(targetDate);
            string dateStr = (targetDate ?? DateTime.Today).ToString("dd MMM yyyy");

            var sb = new StringBuilder();
            if (records.Count == 0)
            {
                sb.Append(@"<tr><td colspan=""10"" class=""no-data"">No pending OPMC faults found.</td></tr>");
            }
            else
            {
                for (int i = 0; i < records.Count; i++)
                {
                    var item = records[i];
                    string bg = i % 2 == 0 ? "#ffffff" : "#f5f5f5";
                    sb.Append($@"
                    <tr style=""background-color:{bg};"">
                        <td style=""{TdStyle}text-align:center;"">{item.No}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.ReportedDate)}</td>
                        <td style=""{TdStyle}font-weight:bold;"">{System.Net.WebUtility.HtmlEncode(item.CircuitName)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.CustomerName)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.OutageTime)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.Lea)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.RelevantOpmc)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.Dgm)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.Gm)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.RecentUpdate)}</td>
                    </tr>");
                }
            }

            string inner = $@"
            <p style=""font-size:14px;"">Dear All,</p>
            <p>Pending Data fault related to OPMC <b>{dateStr}</b> at 07:00 AM are as follows.</p>

            <table style=""{TableStyle}"">
                <tr>
                    <th style=""{ThStyle}"">No</th>
                    <th style=""{ThStyle}"">REPORTED DATE</th>
                    <th style=""{ThStyle}"">CIRCUIT NAME</th>
                    <th style=""{ThStyle}"">CUSTOMER</th>
                    <th style=""{ThStyle}"">OUTAGE TIME</th>
                    <th style=""{ThStyle}"">LEA</th>
                    <th style=""{ThStyle}"">RELEVENT OPMC</th>
                    <th style=""{ThStyle}"">DGM</th>
                    <th style=""{ThStyle}"">GM</th>
                    <th style=""{ThStyle}"">Recent Update</th>
                </tr>
                {sb}
            </table>
            {StandardFooter}";

            return WrapDocument(inner);
        }

        // ────────────────────────────────────────────────────────────────────────
        // TYPE 3: SLT NOC Fault Management Teams (DataFaultRow Mapping)
        // ────────────────────────────────────────────────────────────────────────
        public async Task<List<DataFaultRow>> GetType3SltNocFaultsAsync(DateTime? targetDate = null)
        {
            var rows = new List<DataFaultRow>();

            const string query = @"
                SELECT TOP 100
                    ROW_NUMBER() OVER(ORDER BY f.reportedtime DESC) AS No,
                    ISNULL(NULLIF(f.circuit_name, ''), 'N/A') AS DataCircuitId,
                    ISNULL(NULLIF(f.customer_name, ''), 'N/A') AS DetailsOfCustomer,
                    CONVERT(varchar(12), f.reportedtime, 101) AS FaultDate,
                    CONVERT(varchar(8),  f.reportedtime, 108) AS FaultTime,
                    ISNULL(NULLIF(f.workgroup, ''), 'N/A') AS WorkOrdersHeldUpAt,
                    ISNULL(NULLIF(f.description, ''), 'N/A') AS Description,
                    CASE
                        WHEN f.reportedtime IS NULL THEN '00:00:00'
                        WHEN DATEDIFF(SECOND, f.reportedtime, GETDATE()) < 0 THEN '00:00:00'
                        ELSE
                            CAST(DATEDIFF(SECOND, f.reportedtime, GETDATE()) / 3600 AS VARCHAR) + ':' +
                            RIGHT('00' + CAST((DATEDIFF(SECOND, f.reportedtime, GETDATE()) % 3600) / 60 AS VARCHAR), 2) + ':' +
                            RIGHT('00' + CAST(DATEDIFF(SECOND, f.reportedtime, GETDATE()) % 60 AS VARCHAR), 2)
                    END AS DurationOfComplaint
                FROM dbo.CLARITY_DATA_NW_FAULTS f
                WHERE (
                    f.problem_status IN ('OPEN', 'PENDING', 'IN PROGRESS', 'INPROGRESS', 'ASSIGNED', 'ACKNOWLEDGED', 'Acknowledged', 'Open', 'Pending', 'In Progress')
                    OR (UPPER(ISNULL(f.problem_status, '')) NOT IN ('CLOSED', 'RESOLVED', 'CANCELLED') AND ISNULL(f.problem_status, '') != '')
                    OR f.problem_status IS NULL
                )
                AND (@TargetDate IS NULL OR CAST(f.reportedtime AS DATE) <= CAST(@TargetDate AS DATE))
                ORDER BY f.reportedtime DESC";

            Console.WriteLine($"[FaultReportService] Executing Type 3 SQL Query (TargetDate: {targetDate?.ToString("yyyy-MM-dd") ?? "REAL-TIME LIVE"})...");

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd  = new SqlCommand(query, conn);
                cmd.Parameters.Add("@TargetDate", SqlDbType.Date).Value = (object?)targetDate ?? DBNull.Value;

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rows.Add(new DataFaultRow
                    {
                        No                  = GetInt64(reader, "No"),
                        DataCircuitId       = GetString(reader, "DataCircuitId", "N/A"),
                        DetailsOfCustomer   = GetString(reader, "DetailsOfCustomer", "N/A"),
                        FaultDate           = GetString(reader, "FaultDate", "N/A"),
                        FaultTime           = GetString(reader, "FaultTime", "N/A"),
                        WorkOrdersHeldUpAt  = GetString(reader, "WorkOrdersHeldUpAt", "N/A"),
                        Description         = GetString(reader, "Description", "N/A"),
                        DurationOfComplaint = GetString(reader, "DurationOfComplaint", "00:00:00")
                    });
                }
                Console.WriteLine($"[FaultReportService] Type 3 successfully retrieved {rows.Count} rows.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FaultReportService] ERROR in Type 3 query: {ex.Message}");
            }

            return rows;
        }

        private async Task<string> GenerateType3EmailAsync(DateTime? targetDate)
        {
            var rows = await GetType3SltNocFaultsAsync(targetDate);
            string dateStr = (targetDate ?? DateTime.Today).ToString("dd MMM yyyy");

            var sb = new StringBuilder();
            if (rows.Count == 0)
            {
                sb.Append(@"<tr><td colspan=""8"" class=""no-data"">No pending faults found.</td></tr>");
            }
            else
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var item = rows[i];
                    string bg = i % 2 == 0 ? "#ffffff" : "#f5f5f5";
                    sb.Append($@"
                    <tr style=""background-color:{bg};"">
                        <td style=""{TdStyle}text-align:center;"">{item.No}</td>
                        <td style=""{TdStyle}font-weight:bold;"">{System.Net.WebUtility.HtmlEncode(item.DataCircuitId)}</td>
                        <td style=""{TdStyle}text-align:left;"">{System.Net.WebUtility.HtmlEncode(item.DetailsOfCustomer)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.FaultDate)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.FaultTime)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.WorkOrdersHeldUpAt)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.Description)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.DurationOfComplaint)}</td>
                    </tr>");
                }
            }

            string inner = $@"
            <p style=""font-size:14px;"">Dear All,</p>
            <p>Pending Data Faults Related to SLT NOC - Fault Management Teams as at 00:00hrs on <b>{dateStr}</b> are attached herewith please.</p>

            <table style=""{TableStyle}"">
                {DataCircuitTableHeader()}
                {sb}
            </table>
            {StandardFooter}";

            return WrapDocument(inner);
        }

        // ────────────────────────────────────────────────────────────────────────
        // COMPREHENSIVE TYPE 4: Daily Network Failures Report (All Technologies)
        // ────────────────────────────────────────────────────────────────────────
        public async Task<Type4ReportData> GetType4ComprehensiveReportDataAsync(DateTime? targetDate = null)
        {
            var reportData = new Type4ReportData();

            // Define date range: End date is targetDate or Now. Start date is 30 days prior.
            DateTime endDate = targetDate ?? DateTime.Now;
            DateTime startDate = endDate.AddDays(-30);

            // ─── TABLE 1: MSAN NETWORK FAILURES ──────────────────────────────────
            const string msanQuery = @"
                SELECT
                    ROW_NUMBER() OVER(ORDER BY f.FAULT_TIME DESC) AS [No],
                    f.PLATFORM AS [Network],
                    f.NODE AS [Node],
                    LTRIM(RTRIM(CASE
                        WHEN CHARINDEX('If Memo=--', f.REMARKS) > 0
                            THEN SUBSTRING(f.REMARKS, CHARINDEX('If Memo=--', f.REMARKS) + 11, LEN(f.REMARKS))
                        WHEN CHARINDEX(':', f.REMARKS) > 0 AND ISDATE(SUBSTRING(f.REMARKS, 1, CHARINDEX(':', f.REMARKS) - 1)) = 1
                            THEN SUBSTRING(f.REMARKS, CHARINDEX(':', f.REMARKS) + 1, LEN(f.REMARKS))
                        ELSE f.REMARKS
                    END)) AS [Description],
                    f.NW_ENG AS [NetworkEngineer],
                    f.PROVINCE AS [Province],
                    f.REGION AS [Region],
                    f.FAULT_TIME AS [FaultDetected],
                    f.FAULT_STATUS AS [FaultStatus]
                FROM FAULTS f
                WHERE f.FAULT_STATUS = 'OPEN'
                  AND f.FAULT_TIME BETWEEN @StartDate AND @EndDate
                  AND f.SOURCEIP NOT LIKE '77%'
                  AND f.PLATFORM = 'MSAN' AND f.ALARM_TYPE = 'Node Name'
                  AND (ISNULL(f.REGION, '') NOT IN ('HQ','CENTRAL','HEADQUARTERS','HQ-NOC','IDC HQ NOC')
                       AND ISNULL(f.PROVINCE, '') NOT LIKE '%HQ%'
                       AND ISNULL(f.NW_ENG, '') NOT LIKE '%HQ%')
                ORDER BY f.FAULT_TIME DESC";

            // ─── TABLE 2: SLT 4G SITES DOWN ─────────────────────────────────────
            const string fourGQuery = @"
                SELECT
                    ROW_NUMBER() OVER(ORDER BY f.FAULT_TIME DESC) AS [No],
                    CASE
                        WHEN f.PLATFORM LIKE '%HUAWEI%' THEN 'HUAWEI'
                        WHEN f.PLATFORM LIKE '%ZTE%' THEN 'ZTE'
                        WHEN f.PLATFORM LIKE '%NOKIA%' THEN 'NOKIA'
                        WHEN f.PLATFORM LIKE '%ERICSSON%' THEN 'ERICSSON'
                        ELSE 'UNKNOWN'
                    END AS [Vendor],
                    CONVERT(VARCHAR(10), f.FAULT_TIME, 120) AS [Date],
                    f.NW_ENG AS [Ne],
                    f.REGION AS [SubRegion],
                    LTRIM(RTRIM(CASE
                        WHEN CHARINDEX('If Memo=--', f.REMARKS) > 0
                            THEN SUBSTRING(f.REMARKS, CHARINDEX('If Memo=--', f.REMARKS) + 11, LEN(f.REMARKS))
                        WHEN CHARINDEX(':', f.REMARKS) > 0 AND ISDATE(SUBSTRING(f.REMARKS, 1, CHARINDEX(':', f.REMARKS) - 1)) = 1
                            THEN SUBSTRING(f.REMARKS, CHARINDEX(':', f.REMARKS) + 1, LEN(f.REMARKS))
                        ELSE f.REMARKS
                    END)) AS [Reason],
                    DATEDIFF(DAY, f.FAULT_TIME, GETDATE()) AS [NoOfDays],
                    f.NW_ENG AS [AttendBy]
                FROM FAULTS f
                WHERE f.FAULT_STATUS = 'OPEN'
                  AND f.FAULT_TIME BETWEEN @StartDate AND @EndDate
                  AND f.SOURCEIP NOT LIKE '77%'
                  AND f.PLATFORM IN ('4G', '5G', 'MOBILE', 'RAN', 'SLT 4G')
                  AND (ISNULL(f.REGION, '') NOT IN ('HQ','CENTRAL','HEADQUARTERS','HQ-NOC','IDC HQ NOC')
                       AND ISNULL(f.PROVINCE, '') NOT LIKE '%HQ%'
                       AND ISNULL(f.NW_ENG, '') NOT LIKE '%HQ%')
                ORDER BY f.FAULT_TIME DESC";

            // ─── TABLE 3: TRANSMISSION (TX) NETWORK FAILURES ─────────────────────
            const string txQuery = @"
                SELECT
                    ROW_NUMBER() OVER(ORDER BY f.FAULT_TIME DESC) AS [No],
                    f.PLATFORM AS [Network],
                    LTRIM(RTRIM(CASE
                        WHEN CHARINDEX('If Memo=--', f.REMARKS) > 0
                            THEN SUBSTRING(f.REMARKS, CHARINDEX('If Memo=--', f.REMARKS) + 11, LEN(f.REMARKS))
                        WHEN CHARINDEX(':', f.REMARKS) > 0 AND ISDATE(SUBSTRING(f.REMARKS, 1, CHARINDEX(':', f.REMARKS) - 1)) = 1
                            THEN SUBSTRING(f.REMARKS, CHARINDEX(':', f.REMARKS) + 1, LEN(f.REMARKS))
                        ELSE f.REMARKS
                    END)) AS [Description],
                    f.NW_ENG AS [Engineer],
                    f.PROVINCE AS [Province],
                    f.REGION AS [Region],
                    'DCRO' AS [ResponsibleGroup],
                    LTRIM(RTRIM(CASE
                        WHEN CHARINDEX('If Memo=--', f.REMARKS) > 0
                            THEN SUBSTRING(f.REMARKS, CHARINDEX('If Memo=--', f.REMARKS) + 11, LEN(f.REMARKS))
                        WHEN CHARINDEX(':', f.REMARKS) > 0 AND ISDATE(SUBSTRING(f.REMARKS, 1, CHARINDEX(':', f.REMARKS) - 1)) = 1
                            THEN SUBSTRING(f.REMARKS, CHARINDEX(':', f.REMARKS) + 1, LEN(f.REMARKS))
                        ELSE f.REMARKS
                    END)) AS [Remarks],
                    CONVERT(VARCHAR(10), f.FAULT_TIME, 120) AS [FaultOccurredDate],
                    CONVERT(VARCHAR(8), f.FAULT_TIME, 108) AS [FaultTime],
                    DATEDIFF(HOUR, f.FAULT_TIME, GETDATE()) AS [PendingHours]
                FROM FAULTS f
                WHERE f.FAULT_STATUS = 'OPEN'
                  AND f.FAULT_TIME BETWEEN @StartDate AND @EndDate
                  AND f.SOURCEIP NOT LIKE '77%'
                  AND f.PLATFORM IN ('TX', 'TRANSMISSION', 'DWDM', 'SDH')
                  AND (ISNULL(f.REGION, '') NOT IN ('HQ','CENTRAL','HEADQUARTERS','HQ-NOC','IDC HQ NOC')
                       AND ISNULL(f.PROVINCE, '') NOT LIKE '%HQ%'
                       AND ISNULL(f.NW_ENG, '') NOT LIKE '%HQ%')
                ORDER BY f.FAULT_TIME DESC";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // Execute Table 1: MSAN
                using (var cmd = new SqlCommand(msanQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var faultTime = GetDateTime(reader, "FaultDetected");
                        var status = GetString(reader, "FaultStatus");
                        int pendingHours = (int)(DateTime.Now - faultTime).TotalHours;

                        // Calculate MSAN row color
                        string backgroundColor;
                        if (status != null && (status.Equals("MONITORING", StringComparison.OrdinalIgnoreCase) || status.Equals("UP", StringComparison.OrdinalIgnoreCase)))
                            backgroundColor = "#D1E2C4"; // Light Green
                        else if (pendingHours > 8)
                            backgroundColor = "#E8B4B4"; // Pink
                        else if (pendingHours >= 4)
                            backgroundColor = "#FFFF00"; // Yellow
                        else
                            backgroundColor = "#FFFFFF"; // White

                        reportData.MsanFailures.Add(new MsanNetworkFailureRow
                        {
                            No = (int)GetInt64(reader, "No"),
                            Network = GetString(reader, "Network"),
                            Node = GetString(reader, "Node"),
                            Description = GetString(reader, "Description"),
                            NetworkEngineer = GetString(reader, "NetworkEngineer"),
                            Province = GetString(reader, "Province"),
                            Region = GetString(reader, "Region"),
                            FaultDetected = faultTime,
                            BackgroundColor = backgroundColor
                        });
                    }
                }

                // Execute Table 2: SLT 4G
                using (var cmd = new SqlCommand(fourGQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        reportData.Slt4GSitesDown.Add(new Slt4GSitesDownRow
                        {
                            No = (int)GetInt64(reader, "No"),
                            Vendor = GetString(reader, "Vendor"),
                            Date = GetString(reader, "Date"),
                            Ne = GetString(reader, "Ne"),
                            SubRegion = GetString(reader, "SubRegion"),
                            Reason = GetString(reader, "Reason"),
                            NoOfDays = GetInt32(reader, "NoOfDays"),
                            AttendBy = GetString(reader, "AttendBy")
                        });
                    }
                }

                // Execute Table 3: TX
                using (var cmd = new SqlCommand(txQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        reportData.TxRows.Add(new TxNetworkFailureRow
                        {
                            No = (int)GetInt64(reader, "No"),
                            Network = GetString(reader, "Network"),
                            Description = GetString(reader, "Description"),
                            Engineer = GetString(reader, "Engineer"),
                            Province = GetString(reader, "Province"),
                            Region = GetString(reader, "Region"),
                            ResponsibleGroup = GetString(reader, "ResponsibleGroup"),
                            Remarks = GetString(reader, "Remarks"),
                            FaultOccurredDate = GetString(reader, "FaultOccurredDate"),
                            FaultTime = GetString(reader, "FaultTime"),
                            PendingHours = GetInt32(reader, "PendingHours")
                        });
                    }
                }

                Console.WriteLine($"[FaultReportService] Comprehensive Type 4 successfully processed {reportData.TotalCount} rows (MSAN: {reportData.MsanFailures.Count}, 4G: {reportData.Slt4GSitesDown.Count}, TX: {reportData.TxRows.Count}).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FaultReportService] ERROR in Comprehensive Type 4 query: {ex.Message}");
            }

            return reportData;
        }

        /// <summary>
        /// Extracts technology type from fault description and raw platform
        /// </summary>
        private string ExtractTechnologyFromFaultDescription(string faultDescription, string rawPlatform)
        {
            // First, try to extract from alias text in fault description (## ... ##)
            string aliasText = ExtractAliasText(faultDescription);
            if (!string.IsNullOrEmpty(aliasText))
            {
                // Look for technology indicators in alias text
                if (IsMatch(aliasText, @"MSAN")) return "MSAN";
                if (IsMatch(aliasText, @"LTE|4G")) return "LTE";
                if (IsMatch(aliasText, @"IPTV")) return "IPTV";
                if (IsMatch(aliasText, @"DATA")) return "DATA";
                if (IsMatch(aliasText, @"TX")) return "TX";
                if (IsMatch(aliasText, @"WIFI")) return "WIFI";
            }

            // Fallback to raw platform
            if (!string.IsNullOrEmpty(rawPlatform))
            {
                if (IsMatch(rawPlatform, @"MSAN")) return "MSAN";
                if (IsMatch(rawPlatform, @"LTE")) return "LTE";
                if (IsMatch(rawPlatform, @"IPTV")) return "IPTV";
                if (IsMatch(rawPlatform, @"DATA")) return "DATA";
                if (IsMatch(rawPlatform, @"TX")) return "TX";
                if (IsMatch(rawPlatform, @"WIFI")) return "WIFI";
                if (IsMatch(rawPlatform, @"MPLS")) return "MPLS";
                if (IsMatch(rawPlatform, @"ISP")) return "ISP";
            }

            // Default fallback
            return "UNKNOWN";
        }

        /// <summary>
        /// Extracts text between ## markers from fault description
        /// </summary>
        private string ExtractAliasText(string faultDescription)
        {
            if (string.IsNullOrEmpty(faultDescription)) return string.Empty;

            int startIndex = faultDescription.IndexOf("##");
            if (startIndex == -1) return string.Empty;

            int endIndex = faultDescription.IndexOf("##", startIndex + 2);
            if (endIndex == -1) return string.Empty;

            return faultDescription.Substring(startIndex + 2, endIndex - startIndex - 2).Trim();
        }

        /// <summary>
        /// Checks if text contains a pattern (case-insensitive)
        /// </summary>
        private bool IsMatch(string text, string pattern)
        {
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Maps raw platform to standard platform using PLATFORMS table
        /// </summary>
        private string GetStandardPlatform(string rawPlatform)
        {
            if (string.IsNullOrEmpty(rawPlatform)) return string.Empty;

            // This would ideally query the PLATFORMS table, but for simplicity
            // we'll use a direct mapping for known values
            var platformMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"MSAN", "MSAN"},
                {"LTE", "LTE"},
                {"GPON", "GPON"},
                {"CEA", "CEA"},
                {"RPB", "RPB"},
                {"Service Switch", "Service Switch"},
                {"SLBN", "SLBN"},
                {"PO", "PO"},
                {"TX", "TX"},
                {"ISP", "ISP"},
                {"IPTV", "IPTV"}
            };

            return platformMap.TryGetValue(rawPlatform, out var standard) ? standard : rawPlatform;
        }

        /// <summary>
        /// Calculates pending hours from fault time to now
        /// </summary>
        private int CalculatePendingHours(DateTime faultTime)
        {
            return (int)(DateTime.Now - faultTime).TotalHours;
        }

        /// <summary>
        /// Calculates number of days from fault time to now (for SLT 4G table)
        /// </summary>
        private int CalculateNoOfDays(DateTime faultTime)
        {
            return (int)(DateTime.Now - faultTime).TotalDays;
        }

        /// <summary>
        /// Gets MSAN-specific color code based on pending hours and fault status
        /// </summary>
        private string GetMsanColorCode(DateTime faultTime, string status)
        {
            if (status != null && (status.Equals("MONITORING", StringComparison.OrdinalIgnoreCase) || status.Equals("UP", StringComparison.OrdinalIgnoreCase)))
                return "#D1E2C4"; // Light Green

            int pendingHours = CalculatePendingHours(faultTime);
            if (pendingHours > 8)
                return "#E8B4B4"; // PINK
            else if (pendingHours >= 4)
                return "#FFFF00"; // YELLOW
            else
                return "#FFFFFF"; // WHITE
        }

        /// <summary>
        /// Cleans MSAN description per specification
        /// </summary>
        private string CleanMsanDescription(string description)
        {
            if (string.IsNullOrEmpty(description)) return "N/A";

            try
            {
                string trimmed = description.Trim();
                int colonIndex = trimmed.IndexOf(':');

                if (colonIndex > 0)
                {
                    string prefix = trimmed.Substring(0, colonIndex).Trim();
                    // Check if prefix is a date (simplified check)
                    if (DateTime.TryParse(prefix, out _))
                    {
                        return trimmed.Substring(colonIndex + 1).Trim();
                    }
                }

                return trimmed;
            }
            catch
            {
                return description.Trim();
            }
        }

        /// <summary>
        /// Helper methods for extracting specific information from fault description/alias
        /// </summary>
        private string ExtractVendorFromDescription(string description)
        {
            // Simple extraction - in reality would be more sophisticated
            if (description.Contains("HUAWEI")) return "HUAWEI";
            if (description.Contains("ZTE")) return "ZTE";
            if (description.Contains("NOKIA")) return "NOKIA";
            return string.Empty;
        }

        private string ExtractNeFromAlias(string description)
        {
            string alias = ExtractAliasText(description);
            if (string.IsNullOrEmpty(alias)) return string.Empty;

            // Look for patterns like "NE=" or common NE identifiers
            // This is simplified - would need more sophisticated parsing
            return string.Empty;
        }

        private string ExtractReasonFromAlias(string description)
        {
            string alias = ExtractAliasText(description);
            if (string.IsNullOrEmpty(alias)) return string.Empty;

            // Extract reason from alias - simplified
            return alias.Length > 20 ? alias.Substring(0, 20) : alias;
        }

        private bool IsTxTechnology(string tech) =>
            tech == "TX" || tech.Contains("TX") || tech.Equals("TRANSMISSION", StringComparison.OrdinalIgnoreCase);

        private bool IsTxTechnologyFromDescription(string description) =>
            string.IsNullOrEmpty(description) ? false :
            description.Contains("TX") || description.Contains("TRANSMISSION") ||
            description.Contains("MICROWAVE") || description.Contains("FIBER") ||
            description.Contains("ETHERNET");

        private bool IsMplsTechnology(string tech) =>
            tech == "MPLS";

        private bool IsMplsTechnologyFromDescription(string description) =>
            string.IsNullOrEmpty(description) ? false :
            description.Contains("MPLS");

        private bool IsDataTechnology(string tech) =>
            tech == "DATA" || tech.Contains("DATA") || tech.Equals("BROADBAND", StringComparison.OrdinalIgnoreCase) ||
            tech.Contains("GPON") || tech.Contains("PON") || tech.Contains("FTTH");

        private bool IsDataTechnologyFromDescription(string description) =>
            string.IsNullOrEmpty(description) ? false :
            description.Contains("DATA") || description.Contains("BROADBAND") ||
            description.Contains("GPON") || description.Contains("PON") ||
            description.Contains("FTTH") || description.Contains("ACCESS");

        private bool IsIspTechnology(string tech) =>
            tech == "ISP" || tech.Contains("ISP") || tech.Equals("INTERNET", StringComparison.OrdinalIgnoreCase) ||
            tech.Contains("BACKBONE");

        private bool IsIspTechnologyFromDescription(string description) =>
            string.IsNullOrEmpty(description) ? false :
            description.Contains("ISP") || description.Contains("INTERNET") ||
            description.Contains("BACKBONE");

        private bool IsTxIntlTechnology(string tech) =>
            tech.Contains("INTL") || tech.Contains("INTERNATIONAL") ||
            tech.Contains("GLOBAL") || tech.Contains("SAT") || tech.Contains("VSAT");

        private bool IsTxIntlTechnologyFromDescription(string description) =>
            string.IsNullOrEmpty(description) ? false :
            description.Contains("INTL") || description.Contains("INTERNATIONAL") ||
            description.Contains("GLOBAL") || description.Contains("SAT") ||
            description.Contains("VSAT") || description.Contains("UNDERSEA");

        private bool IsIptvTechnology(string tech) =>
            tech == "IPTV" || tech.Contains("IPTV") || tech.Contains("VIDEO") ||
            tech.Contains("TV");

        private bool IsIptvTechnologyFromDescription(string description) =>
            string.IsNullOrEmpty(description) ? false :
            description.Contains("IPTV") || description.Contains("VIDEO") ||
            description.Contains("TV");

        // ────────────────────────────────────────────────────────────────────────
        // TYPE 4 COMPREHENSIVE REPORT DATA CONTAINER
        // ────────────────────────────────────────────────────────────────────────
        public class Type4ReportData
        {
            public List<MsanNetworkFailureRow> MsanFailures { get; set; } = new();
            public List<Slt4GSitesDownRow> Slt4GSitesDown { get; set; } = new();
            public List<TxNetworkFailureRow> TxRows { get; set; } = new();

            public int TotalCount =>
                MsanFailures.Count +
                Slt4GSitesDown.Count +
                TxRows.Count;
        }

        // ────────────────────────────────────────────────────────────────────────
        // FAULT DATA INTERNAL CLASS
        // ────────────────────────────────────────────────────────────────────────
        private class FaultData
        {
            public long Id { get; set; }
            public string Node { get; set; } = string.Empty;
            public string FaultDescription { get; set; } = string.Empty;
            public string RawPlatform { get; set; } = string.Empty;
            public string StandardPlatform { get; set; } = string.Empty;
            public DateTime FaultTime { get; set; }
            public string Region { get; set; } = string.Empty;
            public string Province { get; set; } = string.Empty;
            public string NetworkEngineer { get; set; } = string.Empty;
            public string Technology { get; set; } = string.Empty;
            public string LatestComment { get; set; } = string.Empty;
        }

        // ────────────────────────────────────────────────────────────────────────
        // TYPE 4 COMPREHENSIVE EMAIL GENERATION
        // ────────────────────────────────────────────────────────────────────────
        private async Task<string> GenerateType4ComprehensiveEmailAsync(DateTime? targetDate)
        {
            var reportData = await GetType4ComprehensiveReportDataAsync(targetDate);
            string dateStr = (targetDate ?? DateTime.Today).ToString("dd MMM yyyy");

            var sb = new StringBuilder();

            // Table 1: MSAN NETWORK FAILURES
            sb.Append($@"<table style=""{TableStyle}"">");
            sb.Append(GenerateTableHeader("MSAN NETWORK FAILURES",
                new[] { "No", "Network", "Node", "Description", "Network Engineer", "Province", "Region", "Fault Detected" }));

            if (reportData.MsanFailures.Count == 0)
            {
                sb.Append(GetNoDataRow(8, "No pending MSAN faults found")); // 8 columns
            }
            else
            {
                foreach (var row in reportData.MsanFailures)
                {
                    sb.Append($@"
                    <tr style=""background-color:{row.BackgroundColor};"">
                        <td style=""{TdStyle}text-align:center;"">{row.No}</td>
                        <td style=""{TdStyle}font-weight:bold;"">{System.Net.WebUtility.HtmlEncode(row.Network)}</td>
                        <td style=""{TdStyle}font-weight:bold;"">{System.Net.WebUtility.HtmlEncode(row.Node)}</td>
                        <td style=""{TdStyle}text-align:left;"">{System.Net.WebUtility.HtmlEncode(row.Description)}</td>
                        <td style=""{TdStyle}font-weight:bold;"">{System.Net.WebUtility.HtmlEncode(row.NetworkEngineer)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Province)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Region)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.FaultDetected.ToString("yyyy-MM-dd HH:mm:ss"))}</td>
                    </tr>");
                }
            }
            sb.Append("</table>");

            // Table 2: SLT 4G SITES DOWN
            sb.Append($@"<table style=""{TableStyle}"">");
            sb.Append(GenerateTableHeader("SLT 4G SITES DOWN",
                new[] { "No", "Vendor", "Date", "Ne", "SubRegion", "Reason", "No Of Days", "AttendBy" }));

            if (reportData.Slt4GSitesDown.Count == 0)
            {
                sb.Append(GetNoDataRow(8, "No pending SLT 4G faults found")); // 8 columns
            }
            else
            {
                foreach (var row in reportData.Slt4GSitesDown)
                {
                    string bg = (reportData.Slt4GSitesDown.IndexOf(row) % 2 == 0) ? "#ffffff" : "#f5f5f5";
                    sb.Append($@"
                    <tr style=""background-color:{bg};"">
                        <td style=""{TdStyle}text-align:center;"">{row.No}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(row.Vendor)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Date)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Ne)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.SubRegion)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Reason)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.NoOfDays.ToString())}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.AttendBy)}</td>
                    </tr>");
                }
            }
            sb.Append("</table>");

            // Table 3: TRANSMISSION (TX) NETWORK FAILURES
            sb.Append($@"<table style=""{TableStyle}"">");
            sb.Append(GenerateTableHeader("TRANSMISSION (TX) NETWORK FAILURES",
                new[] { "No", "Network", "Description", "Engineer", "Province", "Region", "Responsible Group", "Remarks", "Fault Occurred Date", "Time", "Pending hours" }));

            if (reportData.TxRows.Count == 0)
            {
                sb.Append(GetNoDataRow(11, "No outage")); // 11 columns
            }
            else
            {
                foreach (var row in reportData.TxRows)
                {
                    string bg = (reportData.TxRows.IndexOf(row) % 2 == 0) ? "#ffffff" : "#f5f5f5";
                    sb.Append($@"
                    <tr style=""background-color:{bg};"">
                        <td style=""{TdStyle}text-align:center;"">{row.No}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(row.Network)}</td>
                        <td style=""{TdStyle}text-align:left;"">{System.Net.WebUtility.HtmlEncode(row.Description)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Engineer)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Province)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Region)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.ResponsibleGroup)}</td>
                        <td style=""{TdStyle}text-align:left;"">{System.Net.WebUtility.HtmlEncode(row.Remarks)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.FaultOccurredDate)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.FaultTime)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.PendingHours.ToString())}</td>
                    </tr>");
                }
            }
            sb.Append("</table>");

            string inner = $@"
            <p style=""font-size:14px;"">Dear All,</p>
            <p>Comprehensive Network Failures Report as of <b>{dateStr}</b> is as follows.</p>

            {sb}
            {StandardFooter}";

            return WrapDocument(inner);
        }

        /// <summary>
        /// Generates a table header row with the specified columns
        /// </summary>
        private string GenerateTableHeader(string tableName, string[] columns)
        {
            var sb = new StringBuilder();
            sb.Append("<tr>");
            sb.Append($"<th style=\"{ThStyle}\" colspan=\"{columns.Length}\">{tableName}</th>");
            sb.Append("</tr>");
            sb.Append("<tr>");

            foreach (var column in columns)
            {
                sb.Append($"<th style=\"{ThStyle}\">{column}</th>");
            }

            sb.Append("</tr>");

            return sb.ToString();
        }

        /// <summary>
        /// Generates a "no data" row spanning all columns
        /// </summary>
        private string GetNoDataRow(int columnCount, string message)
        {
            return $@"
            <tr>
                <td colspan=""{columnCount}"" class=""no-data"">{message}</td>
            </tr>";
        }

        private async Task<string> GenerateType4EmailCompatibility(List<PendingFaultReport> reports, string date)
        {
            // Ignore the reports parameter, fetch fresh comprehensive data
            var reportData = await GetType4ComprehensiveReportDataAsync();
            // Use the provided date string for the header (expected format: "dd MMM yyyy")
            string dateStr = date;

            var sb = new StringBuilder();

            // Table 1: MSAN NETWORK FAILURES
            sb.Append($@"<table style=""{TableStyle}"">");
            sb.Append(GenerateTableHeader("MSAN NETWORK FAILURES",
                new[] { "No", "Network", "Node", "Description", "Network Engineer", "Province", "Region", "Fault Detected" }));

            if (reportData.MsanFailures.Count == 0)
            {
                sb.Append(GetNoDataRow(8, "No pending MSAN faults found")); // 8 columns
            }
            else
            {
                foreach (var row in reportData.MsanFailures)
                {
                    sb.Append($@"
                    <tr style=""background-color:{row.BackgroundColor};"">
                        <td style=""{TdStyle}text-align:center;"">{row.No}</td>
                        <td style=""{TdStyle}font-weight:bold;"">{System.Net.WebUtility.HtmlEncode(row.Network)}</td>
                        <td style=""{TdStyle}font-weight:bold;"">{System.Net.WebUtility.HtmlEncode(row.Node)}</td>
                        <td style=""{TdStyle}text-align:left;"">{System.Net.WebUtility.HtmlEncode(row.Description)}</td>
                        <td style=""{TdStyle}font-weight:bold;"">{System.Net.WebUtility.HtmlEncode(row.NetworkEngineer)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Province)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Region)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.FaultDetected.ToString("yyyy-MM-dd HH:mm:ss"))}</td>
                    </tr>");
                }
            }
            sb.Append("</table>");

            // Table 2: SLT 4G SITES DOWN
            sb.Append($@"<table style=""{TableStyle}"">");
            sb.Append(GenerateTableHeader("SLT 4G SITES DOWN",
                new[] { "No", "Vendor", "Date", "Ne", "SubRegion", "Reason", "No Of Days", "AttendBy" }));

            if (reportData.Slt4GSitesDown.Count == 0)
            {
                sb.Append(GetNoDataRow(8, "No pending SLT 4G faults found")); // 8 columns
            }
            else
            {
                foreach (var row in reportData.Slt4GSitesDown)
                {
                    string bg = (reportData.Slt4GSitesDown.IndexOf(row) % 2 == 0) ? "#ffffff" : "#f5f5f5";
                    sb.Append($@"
                    <tr style=""background-color:{bg};"">
                        <td style=""{TdStyle}text-align:center;"">{row.No}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(row.Vendor)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Date)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Ne)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.SubRegion)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Reason)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.NoOfDays.ToString())}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.AttendBy)}</td>
                    </tr>");
                }
            }
            sb.Append("</table>");

            // Table 3: TRANSMISSION (TX) NETWORK FAILURES
            sb.Append($@"<table style=""{TableStyle}"">");
            sb.Append(GenerateTableHeader("TRANSMISSION (TX) NETWORK FAILURES",
                new[] { "No", "Network", "Description", "Engineer", "Province", "Region", "Responsible Group", "Remarks", "Fault Occurred Date", "Time", "Pending hours" }));

            if (reportData.TxRows.Count == 0)
            {
                sb.Append(GetNoDataRow(11, "No outage")); // 11 columns
            }
            else
            {
                foreach (var row in reportData.TxRows)
                {
                    string bg = (reportData.TxRows.IndexOf(row) % 2 == 0) ? "#ffffff" : "#f5f5f5";
                    sb.Append($@"
                    <tr style=""background-color:{bg};"">
                        <td style=""{TdStyle}text-align:center;"">{row.No}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(row.Network)}</td>
                        <td style=""{TdStyle}text-align:left;"">{System.Net.WebUtility.HtmlEncode(row.Description)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Engineer)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Province)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.Region)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.ResponsibleGroup)}</td>
                        <td style=""{TdStyle}text-align:left;"">{System.Net.WebUtility.HtmlEncode(row.Remarks)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.FaultOccurredDate)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.FaultTime)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(row.PendingHours.ToString())}</td>
                    </tr>");
                }
            }
            sb.Append("</table>");

            string inner = $@"
            <p style=""font-size:14px;"">Dear All,</p>
            <p>Comprehensive Network Failures Report as of <b>{dateStr}</b> is as follows.</p>

            {sb}
            {StandardFooter}";

            return WrapDocument(inner);
        }

        private static string CleanDescription(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "N/A";

            // Remove leading date/time stamps (e.g., "2026-09-30 10:15:00 - ")
            var cleaned = System.Text.RegularExpressions.Regex.Replace(raw, @"^\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\s*[-–]\s*", "");
            // Remove clarity prefixes like "CLARITY:", "UPDATE:", etc.
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"^(CLARITY|UPDATE|REMARK|NOTE):\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // Trim
            return cleaned.Trim();
        }

        // ── Compatibility Methods ──────────────────────────────────────────────
        public async Task<List<PendingFaultReport>> GetPendingFaultsAsync()
        {
            var dataRows = await GetType1RegionalFaultsAsync(null);
            var reports = new List<PendingFaultReport>();
            foreach (var r in dataRows)
            {
                reports.Add(new PendingFaultReport
                {
                    No                  = r.No,
                    DataCircuitId       = r.DataCircuitId,
                    DetailsOfCustomer   = r.DetailsOfCustomer,
                    Date                = r.FaultDate,
                    Time                = r.FaultTime,
                    WorkOrdersHeldUpAt  = r.WorkOrdersHeldUpAt,
                    Description         = r.Description,
                    DurationOfComplaint = r.DurationOfComplaint
                });
            }
            return reports;
        }

        public async Task<string> GenerateHtmlEmailBodyAsync(List<PendingFaultReport> reports, int type = 1)
        {
            string date = DateTime.Now.ToString("dd MMM yyyy");
            return type switch
            {
                1 => GenerateType1RegionalTeamsCompatibility(reports, date),
                2 => GenerateType2OpmcFaultsCompatibility(reports, date),
                3 => GenerateType3SltNocFaultsCompatibility(reports, date),
                4 => await GenerateType4EmailCompatibility(reports, date),
                _ => GenerateType1RegionalTeamsCompatibility(reports, date)
            };
        }

        private string GenerateType1RegionalTeamsCompatibility(List<PendingFaultReport> reports, string date)
        {
            string inner = $@"
            <p style=""font-size:14px;"">Dear All,</p>
            <p>Pending Data Faults related to Regional Network Teams on <b>{date}</b> are as follows.</p>

            <table style=""{TableStyle}"">
                {DataCircuitTableHeader()}
                {DataCircuitRows(reports)}
            </table>

            <p>Best Regards,<br>
               <b>Lalith Perera</b><br>
               <span style=""color:#1a6e2e;font-weight:bold;"">SLT NOC – Service Assurance Team</span>
            </p>
            <hr style=""border:none;border-top:1px solid #cccccc;margin:10px 0;"">
            <table cellpadding=""0"" cellspacing=""0"" style=""font-family:Arial,Helvetica,sans-serif;font-size:11px;color:#444;"">
                <tr>
                    <td>
                        <p style=""margin:2px 0;""><b>Sri Lanka Telecom PLC</b></p>
                        <p style=""margin:2px 0;"">6<sup>th</sup> Floor, OTS Building, Lotus Road, Colombo 01, Sri Lanka</p>
                        <p style=""margin:2px 0;"">Email: <a href=""mailto:sateam@slt.com.lk"" style=""color:#1a3c5e;"">sateam@slt.com.lk</a>&nbsp;&nbsp;|&nbsp;&nbsp;Tel: +94 11 202 9595</p>
                    </td>
                </tr>
            </table>";

            return WrapDocument(inner);
        }

        private string GenerateType2OpmcFaultsCompatibility(List<PendingFaultReport> reports, string date)
        {
            var sb = new StringBuilder();
            if (reports.Count == 0)
            {
                sb.Append(@"<tr><td colspan=""10"" class=""no-data"">No pending OPMC faults found.</td></tr>");
            }
            else
            {
                for (int i = 0; i < reports.Count; i++)
                {
                    var item = reports[i];
                    string bg = i % 2 == 0 ? "#ffffff" : "#f5f5f5";
                    sb.Append($@"
                    <tr style=""background-color:{bg};"">
                        <td style=""{TdStyle}text-align:center;"">{item.No}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.ReportedDate)}</td>
                        <td style=""{TdStyle}font-weight:bold;"">{System.Net.WebUtility.HtmlEncode(item.CircuitName)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.CustomerName)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.OutageTime)}</td>
                        <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.Lea)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.RelevantOpmc)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.Dgm)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.Gm)}</td>
                        <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.RecentUpdate)}</td>
                    </tr>");
                }
            }

            string inner = $@"
            <p style=""font-size:14px;"">Dear All,</p>
            <p>Pending Data fault related to OPMC <b>{date}</b> at 07:00 AM are as follows.</p>

            <table style=""{TableStyle}"">
                <tr>
                    <th style=""{ThStyle}"">No</th>
                    <th style=""{ThStyle}"">REPORTED DATE</th>
                    <th style=""{ThStyle}"">CIRCUIT NAME</th>
                    <th style=""{ThStyle}"">CUSTOMER</th>
                    <th style=""{ThStyle}"">OUTAGE TIME</th>
                    <th style=""{ThStyle}"">LEA</th>
                    <th style=""{ThStyle}"">RELEVENT OPMC</th>
                    <th style=""{ThStyle}"">DGM</th>
                    <th style=""{ThStyle}"">GM</th>
                    <th style=""{ThStyle}"">Recent Update</th>
                </tr>
                {sb}
            </table>
            {StandardFooter}";

            return WrapDocument(inner);
        }

        private string GenerateType3SltNocFaultsCompatibility(List<PendingFaultReport> reports, string date)
        {
            string inner = $@"
            <p style=""font-size:14px;"">Dear All,</p>
            <p>Pending Data Faults Related to SLT NOC - Fault Management Teams as at 00:00hrs on <b>{date}</b> are attached herewith please.</p>

            <table style=""{TableStyle}"">
                {DataCircuitTableHeader()}
                {DataCircuitRows(reports)}
            </table>
            {StandardFooter}";

            return WrapDocument(inner);
        }

        // ── HTML Document Helpers ──────────────────────────────────────────────
        private static string WrapDocument(string innerHtml)
        {
            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width,initial-scale=1.0"">
<style>
  body {{ margin:0; padding:0; background:#f0f2f5; }}
  .email-wrapper {{ max-width:960px; margin:24px auto; background:#ffffff;
                   padding:28px 32px; border-radius:6px;
                   box-shadow:0 2px 8px rgba(0,0,0,.10); }}
  table {{ border-collapse:collapse; width:100%; font-size:12px; margin-bottom:16px; }}
  th, td {{ padding:6px 8px; border:1px solid #cccccc; }}
  th {{ background-color:#1a3c5e; color:#ffffff; font-size:11px; text-align:center; }}
  td {{ vertical-align:top; }}
  p {{ margin:6px 0; line-height:1.5; }}
  a {{ color:#1a3c5e; }}
  .no-data {{ text-align:center; color:#777; font-style:italic; padding:12px; }}
</style>
</head>
<body>
<div class=""email-wrapper"" style=""{FontBase}"">
{innerHtml}
</div>
</body>
</html>";
        }

        private static string DataCircuitTableHeader()
        {
            return $@"
            <tr>
                <th rowspan=""2"" style=""{ThStyle}"">No</th>
                <th rowspan=""2"" style=""{ThStyle}"">Data Circuit ID</th>
                <th rowspan=""2"" style=""{ThStyle}"">Customer</th>
                <th colspan=""2"" style=""{ThStyle}"">Fault Reported<br><small>(Date | Time)</small></th>
                <th rowspan=""2"" style=""{ThStyle}"">Work Orders Presently<br>Held up at</th>
                <th rowspan=""2"" style=""{ThStyle}"">Description</th>
                <th rowspan=""2"" style=""{ThStyle}"">Duration of Complaint<br><small>(hh:mm:ss)</small></th>
            </tr>
            <tr>
                <th style=""{ThStyle}"">Date</th>
                <th style=""{ThStyle}"">Time</th>
            </tr>";
        }

        private static string DataCircuitRows(List<PendingFaultReport> reports)
        {
            if (reports.Count == 0)
            {
                return $@"<tr><td colspan=""8"" class=""no-data"">No pending faults found.</td></tr>";
            }

            var sb = new StringBuilder();
            for (int i = 0; i < reports.Count; i++)
            {
                var item = reports[i];
                string bg = i % 2 == 0 ? "#ffffff" : "#f5f5f5";
                sb.Append($@"
                <tr style=""background-color:{bg};"">
                    <td style=""{TdStyle}text-align:center;"">{item.No}</td>
                    <td style=""{TdStyle}font-weight:bold;"">{System.Net.WebUtility.HtmlEncode(item.DataCircuitId)}</td>
                    <td style=""{TdStyle}text-align:left;"">{System.Net.WebUtility.HtmlEncode(item.DetailsOfCustomer)}</td>
                    <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.Date)}</td>
                    <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.Time)}</td>
                    <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.WorkOrdersHeldUpAt)}</td>
                    <td style=""{TdStyle}"">{System.Net.WebUtility.HtmlEncode(item.Description)}</td>
                    <td style=""{TdStyle}text-align:center;"">{System.Net.WebUtility.HtmlEncode(item.DurationOfComplaint)}</td>
                </tr>");
            }
            return sb.ToString();
        }
    }
}