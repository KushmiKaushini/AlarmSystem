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
        // LIVE DATABASE STATUS & LATEST DATA SNAPSHOT DISCOVERY
        // ────────────────────────────────────────────────────────────────────────
        public async Task<DatabaseConnectionStatus> GetDatabaseConnectionStatusAsync()
        {
            var status = new DatabaseConnectionStatus();
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                status.IsConnected = true;

                using (var cmd = new SqlCommand("SELECT @@SERVERNAME, DB_NAME(), @@VERSION", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        status.ServerName = reader.GetString(0);
                        status.DatabaseName = reader.GetString(1);
                        var ver = reader.GetString(2);
                        status.SqlVersion = ver.Split('\n')[0].Trim();
                    }
                }

                const string statsSql = @"
                    SELECT 
                        (SELECT COUNT(*) FROM dbo.FAULTS) AS FaultsCount,
                        (SELECT COUNT(*) FROM dbo.CLARITY_WO_STATUS) AS WorkOrdersCount,
                        (SELECT COUNT(*) FROM dbo.CLARITY_COMMENTS) AS ClarityCommentsCount,
                        (SELECT COUNT(*) FROM dbo.COMMENTS) AS CommentsCount,
                        (SELECT COUNT(*) FROM dbo.CLARITY_WO_STATUS w
                         WHERE (w.WO_STATUS IN ('OPEN', 'PENDING', 'IN PROGRESS', 'INPROGRESS', 'ASSIGNED', 'ACKNOWLEDGED', 'Acknowledged', 'Open', 'Pending', 'In Progress')
                                OR (UPPER(ISNULL(w.WO_STATUS, '')) NOT IN ('CLOSED', 'RESOLVED', 'CANCELLED') AND ISNULL(w.WO_STATUS, '') != '')
                                OR w.WO_STATUS IS NULL)
                           AND ISNULL(w.AREA, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS')
                           AND ISNULL(w.REGION, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS', 'SOC', 'Transport Networks', 'Network Services', 'DC & Cloud', 'IPTV', 'SME')
                           AND ISNULL(w.PROVINCE, '') NOT LIKE '%HQ%'
                           AND ISNULL(w.WORK_GROUP, '') NOT LIKE '%HQ%') AS Type1Count,
                        (SELECT COUNT(*) FROM dbo.CLARITY_WO_STATUS w
                         WHERE (w.WO_STATUS IN ('OPEN', 'PENDING', 'IN PROGRESS', 'INPROGRESS', 'ASSIGNED', 'ACKNOWLEDGED', 'Acknowledged', 'Open', 'Pending', 'In Progress')
                                OR (UPPER(ISNULL(w.WO_STATUS, '')) NOT IN ('CLOSED', 'RESOLVED', 'CANCELLED') AND ISNULL(w.WO_STATUS, '') != '')
                                OR w.WO_STATUS IS NULL)
                           AND ISNULL(w.AREA, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS')
                           AND ISNULL(w.REGION, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS', 'SOC', 'Transport Networks', 'Network Services', 'DC & Cloud', 'IPTV', 'SME')
                           AND ISNULL(w.PROVINCE, '') NOT LIKE '%HQ%'
                           AND ISNULL(w.WORK_GROUP, '') NOT LIKE '%HQ%'
                           AND (w.OPMC_NW = 'OPMC' OR w.WORK_GROUP LIKE '%-MDF' OR w.WORK_GROUP LIKE '%-CPE%' OR w.WORK_GROUP LIKE '%-CPE-MTCE')) AS Type2Count,
                        (SELECT COUNT(*) FROM dbo.CLARITY_DATA_NW_FAULTS f
                         LEFT JOIN dbo.CLARITY_WO_STATUS w ON w.CLARITY_REF = f.problem_no
                         WHERE (f.problem_status IN ('OPEN', 'PENDING', 'IN PROGRESS', 'INPROGRESS', 'ASSIGNED', 'ACKNOWLEDGED', 'Acknowledged', 'Open', 'Pending', 'In Progress')
                                OR (UPPER(ISNULL(f.problem_status, '')) NOT IN ('CLOSED', 'RESOLVED', 'CANCELLED') AND ISNULL(f.problem_status, '') != '')
                                OR f.problem_status IS NULL)
                           AND (w.AREA IS NULL OR (
                               ISNULL(w.AREA, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS')
                               AND ISNULL(w.REGION, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS', 'SOC', 'Transport Networks', 'Network Services', 'DC & Cloud', 'IPTV', 'SME')
                               AND ISNULL(w.PROVINCE, '') NOT LIKE '%HQ%'
                               AND ISNULL(w.WORK_GROUP, '') NOT LIKE '%HQ%'
                           ))) AS Type3Count,
                        (SELECT COUNT(*) FROM dbo.FAULTS f WHERE f.FAULT_STATUS = 'OPEN' AND f.SOURCEIP NOT LIKE '77%' AND f.PLATFORM = 'MSAN' AND (f.ALARM_TYPE = 'Node Down' OR f.ALARM_TYPE = 'Node Name' OR f.ALARM_TYPE LIKE '%Node%') AND (ISNULL(f.REGION, '') NOT IN ('HQ','CENTRAL','HEADQUARTERS','HQ-NOC','IDC HQ NOC') AND ISNULL(f.PROVINCE, '') NOT LIKE '%HQ%' AND ISNULL(f.NW_ENG, '') NOT LIKE '%HQ%')) AS Type4MsanCount,
                        (SELECT COUNT(*) FROM dbo.FAULTS f WHERE f.FAULT_STATUS = 'OPEN' AND f.SOURCEIP NOT LIKE '77%' AND f.PLATFORM IN ('LTE', '4G', '5G', 'MOBILE', 'RAN', 'SLT 4G') AND (ISNULL(f.REGION, '') NOT IN ('HQ','CENTRAL','HEADQUARTERS','HQ-NOC','IDC HQ NOC') AND ISNULL(f.PROVINCE, '') NOT LIKE '%HQ%' AND ISNULL(f.NW_ENG, '') NOT LIKE '%HQ%')) AS Type44GCount,
                        (SELECT COUNT(*) FROM dbo.FAULTS f WHERE f.FAULT_STATUS = 'OPEN' AND f.SOURCEIP NOT LIKE '77%' AND f.PLATFORM IN ('TX', 'TRANSMISSION', 'DWDM', 'SDH', 'SLBN') AND (ISNULL(f.REGION, '') NOT IN ('HQ','CENTRAL','HEADQUARTERS','HQ-NOC','IDC HQ NOC') AND ISNULL(f.PROVINCE, '') NOT LIKE '%HQ%' AND ISNULL(f.NW_ENG, '') NOT LIKE '%HQ%')) AS Type4TxCount,
                        (SELECT MAX(MaxTime) FROM (
                            SELECT MAX(FAULT_TIME) AS MaxTime FROM dbo.FAULTS
                            UNION ALL
                            SELECT MAX(STARTED_TIME) FROM dbo.CLARITY_WO_STATUS
                            UNION ALL
                            SELECT MAX(reportedtime) FROM dbo.CLARITY_DATA_NW_FAULTS
                        ) t) AS LatestDate;
                ";

                using (var cmd = new SqlCommand(statsSql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        status.FaultsCount          = reader.GetInt32(0);
                        status.WorkOrdersCount      = reader.GetInt32(1);
                        status.ClarityCommentsCount = reader.GetInt32(2);
                        status.CommentsCount        = reader.GetInt32(3);
                        status.Type1Count           = reader.GetInt32(4);
                        status.Type2Count           = reader.GetInt32(5);
                        status.Type3Count           = reader.GetInt32(6);
                        status.Type4MsanCount       = reader.GetInt32(7);
                        status.Type44GCount         = reader.GetInt32(8);
                        status.Type4TxCount         = reader.GetInt32(9);
                        status.Type4Count           = status.Type4MsanCount + status.Type44GCount + status.Type4TxCount;
                        if (!reader.IsDBNull(10))
                        {
                            status.LatestRecordDate = reader.GetDateTime(10);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status.IsConnected = false;
                status.ErrorMessage = ex.Message;
            }

            return status;
        }

        public async Task<DateTime?> GetLatestDataDateAsync()
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(@"
                    SELECT CAST(MAX(MaxTime) AS DATE) FROM (
                        SELECT MAX(FAULT_TIME) AS MaxTime FROM dbo.FAULTS
                        UNION ALL
                        SELECT MAX(STARTED_TIME) FROM dbo.CLARITY_WO_STATUS
                        UNION ALL
                        SELECT MAX(reportedtime) FROM dbo.CLARITY_DATA_NW_FAULTS
                    ) t", conn);
                var res = await cmd.ExecuteScalarAsync();
                if (res != null && res != DBNull.Value)
                    return Convert.ToDateTime(res);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FaultReportService] Warning in GetLatestDataDateAsync: {ex.Message}");
            }
            return null;
        }

        // ────────────────────────────────────────────────────────────────────────
        // MASTER PREVIEW ENTRY POINT (Parameterized by Type & Optional Target Date)
        // ────────────────────────────────────────────────────────────────────────
        public async Task<string> GetEmailPreviewHtmlAsync(int type, DateTime? targetDate = null)
        {
            var effectiveDate = targetDate ?? await GetLatestDataDateAsync() ?? DateTime.Today;

            return type switch
            {
                1 => await GenerateType1EmailAsync(effectiveDate),
                2 => await GenerateType2EmailAsync(effectiveDate),
                3 => await GenerateType3EmailAsync(effectiveDate),
                4 => await GenerateType4ComprehensiveEmailAsync(effectiveDate),
                _ => await GenerateType1EmailAsync(effectiveDate)
            };
        }

        // ────────────────────────────────────────────────────────────────────────
        // TYPE 1: Regional Network Teams (DataFaultRow Mapping)
        // ────────────────────────────────────────────────────────────────────────
        public async Task<List<DataFaultRow>> GetType1RegionalFaultsAsync(DateTime? targetDate = null)
        {
            var rows = new List<DataFaultRow>();
            var effectiveDate = targetDate ?? await GetLatestDataDateAsync() ?? DateTime.Today;
            DateTime cutOffTime = effectiveDate.Date.AddHours(7);

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
                        WHEN DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), @CutOffTime) < 0 THEN '00:00:00'
                        ELSE
                            CAST(DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), @CutOffTime) / 3600 AS VARCHAR) + ':' +
                            RIGHT('00' + CAST((DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), @CutOffTime) % 3600) / 60 AS VARCHAR), 2) + ':' +
                            RIGHT('00' + CAST(DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), @CutOffTime) % 60 AS VARCHAR), 2)
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
                AND ISNULL(w.AREA, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS')
                AND ISNULL(w.REGION, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS', 'SOC', 'Transport Networks', 'Network Services', 'DC & Cloud', 'IPTV', 'SME')
                AND ISNULL(w.PROVINCE, '') NOT LIKE '%HQ%'
                AND ISNULL(w.WORK_GROUP, '') NOT LIKE '%HQ%'
                AND ISNULL(w.STARTED_TIME, f.reportedtime) <= @CutOffTime
                ORDER BY ISNULL(w.STARTED_TIME, f.reportedtime) DESC";

            Console.WriteLine($"[FaultReportService] Executing Type 1 SQL Query (TargetDate: {effectiveDate:yyyy-MM-dd}, CutOffTime: {cutOffTime:yyyy-MM-dd HH:mm:ss})...");

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd  = new SqlCommand(query, conn);
                cmd.Parameters.Add("@CutOffTime", SqlDbType.DateTime).Value = cutOffTime;

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
            <p>Pending Data Faults related to Regional Network Teams as at 07:00 AM on <b>{dateStr}</b> are as follows.</p>

            <table style=""{TableStyle}"">
                {DataCircuitTableHeader()}
                {sb}
            </table>
            {StandardFooter}";

            return WrapDocument(inner);
        }

        // ────────────────────────────────────────────────────────────────────────
        // TYPE 2: OPMC Faults (OpmcFaultRow Mapping)
        // ────────────────────────────────────────────────────────────────────────
        public async Task<List<OpmcFaultRow>> GetType2OpmcFaultsAsync(DateTime? targetDate = null)
        {
            var rows = new List<OpmcFaultRow>();
            var effectiveDate = targetDate ?? await GetLatestDataDateAsync() ?? DateTime.Today;
            DateTime cutOffTime = effectiveDate.Date.AddHours(7);

            const string query = @"
                SELECT TOP 100
                    ROW_NUMBER() OVER(ORDER BY ISNULL(w.STARTED_TIME, f.reportedtime) DESC) AS No,
                    CONVERT(varchar(20), ISNULL(w.STARTED_TIME, f.reportedtime), 120) AS ReportedDate,
                    ISNULL(NULLIF(f.circuit_name, ''), ISNULL(NULLIF(w.CIRCUIT, ''), 'N/A')) AS CircuitName,
                    ISNULL(NULLIF(f.customer_name, ''), ISNULL(NULLIF(w.CUSTOMER, ''), 'N/A')) AS CustomerName,
                    CASE
                        WHEN ISNULL(w.STARTED_TIME, f.reportedtime) IS NULL THEN '00:00'
                        WHEN DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), @CutOffTime) < 0 THEN '00:00'
                        ELSE
                            CAST(DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), @CutOffTime) / 3600 AS VARCHAR) + ':' +
                            RIGHT('00' + CAST((DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), @CutOffTime) % 3600) / 60 AS VARCHAR), 2)
                    END AS OutageTime,
                    COALESCE(
                        opmcData.ResolvedLea,
                        ISNULL(NULLIF(w.AREA, ''), 'N/A')
                    ) AS Lea,
                    ISNULL(opmcData.ResolvedOpmcName, 'N/A') AS RelevantOpmc,
                    ISNULL(NULLIF(w.PROVINCE, ''), ISNULL(NULLIF(opmcData.ResolvedProvince, ''), ISNULL(NULLIF(w.OPMC_NW, ''), 'N/A'))) AS Dgm,
                    ISNULL(NULLIF(w.REGION, ''), ISNULL(NULLIF(opmcData.ResolvedRegion, ''), 'N/A')) AS Gm,
                    ISNULL((SELECT TOP 1 cc.description
                            FROM dbo.CLARITY_COMMENTS cc
                            WHERE cc.docket_id = CAST(w.CLARITY_REF AS VARCHAR)
                            ORDER BY 
                                CASE WHEN cc.description NOT LIKE '%NEW DOCKET CREATED%' 
                                          AND cc.description NOT LIKE '%NEW WORK ORDER CREATED%' 
                                     THEN 1 ELSE 0 END DESC,
                                cc.event_time DESC), 'No update') AS RecentUpdate
                FROM dbo.CLARITY_WO_STATUS w
                LEFT JOIN dbo.CLARITY_DATA_NW_FAULTS f ON f.problem_no = w.CLARITY_REF
                OUTER APPLY (
                    SELECT
                        COALESCE(
                            (SELECT TOP 1 lm.LEA FROM dbo.LEA_MAPPING lm WHERE lm.LEA = 
                                CASE WHEN CHARINDEX('-', w.WORK_GROUP) > 0 AND CHARINDEX('-', w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1) > 0
                                     THEN SUBSTRING(w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1, CHARINDEX('-', w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1) - CHARINDEX('-', w.WORK_GROUP) - 1)
                                     ELSE NULL END),
                            (SELECT TOP 1 lm.LEA FROM dbo.LEA_MAPPING lm WHERE lm.LEA = 
                                CASE WHEN CHARINDEX('-', w.WORK_GROUP) > 0 
                                     THEN SUBSTRING(w.WORK_GROUP, 1, CHARINDEX('-', w.WORK_GROUP) - 1)
                                     ELSE w.WORK_GROUP END),
                            CASE WHEN CHARINDEX('-', w.WORK_GROUP) > 0 AND CHARINDEX('-', w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1) > 0
                                 THEN SUBSTRING(w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1, CHARINDEX('-', w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1) - CHARINDEX('-', w.WORK_GROUP) - 1)
                                 WHEN CHARINDEX('-', w.WORK_GROUP) > 0 
                                 THEN SUBSTRING(w.WORK_GROUP, 1, CHARINDEX('-', w.WORK_GROUP) - 1)
                                 ELSE w.WORK_GROUP END
                        ) AS ResolvedLea,
                        COALESCE(
                            (SELECT TOP 1 lm.OPMC_NAME FROM dbo.LEA_MAPPING lm WHERE lm.LEA = 
                                CASE WHEN CHARINDEX('-', w.WORK_GROUP) > 0 AND CHARINDEX('-', w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1) > 0
                                     THEN SUBSTRING(w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1, CHARINDEX('-', w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1) - CHARINDEX('-', w.WORK_GROUP) - 1)
                                     ELSE NULL END AND lm.OPMC_NAME IS NOT NULL AND lm.OPMC_NAME != '' AND lm.OPMC_NAME != '#N/A'),
                            (SELECT TOP 1 lm.OPMC_NAME FROM dbo.LEA_MAPPING lm WHERE lm.LEA = 
                                CASE WHEN CHARINDEX('-', w.WORK_GROUP) > 0 
                                     THEN SUBSTRING(w.WORK_GROUP, 1, CHARINDEX('-', w.WORK_GROUP) - 1)
                                     ELSE w.WORK_GROUP END AND lm.OPMC_NAME IS NOT NULL AND lm.OPMC_NAME != '' AND lm.OPMC_NAME != '#N/A'),
                            (SELECT TOP 1 lm.OPMC_NAME FROM dbo.LEA_MAPPING lm WHERE lm.OPMC = 
                                CASE WHEN CHARINDEX('-', w.WORK_GROUP) > 0 
                                     THEN SUBSTRING(w.WORK_GROUP, 1, CHARINDEX('-', w.WORK_GROUP) - 1)
                                     ELSE w.WORK_GROUP END AND lm.OPMC_NAME IS NOT NULL AND lm.OPMC_NAME != '' AND lm.OPMC_NAME != '#N/A'),
                            (SELECT TOP 1 lm.OPMC_NAME FROM dbo.LEA_MAPPING lm WHERE lm.RTOM = 
                                'R-' + CASE WHEN CHARINDEX('-', w.WORK_GROUP) > 0 
                                     THEN SUBSTRING(w.WORK_GROUP, 1, CHARINDEX('-', w.WORK_GROUP) - 1)
                                     ELSE w.WORK_GROUP END AND lm.OPMC_NAME IS NOT NULL AND lm.OPMC_NAME != '' AND lm.OPMC_NAME != '#N/A'),
                            (SELECT TOP 1 lm.OPMC FROM dbo.LEA_MAPPING lm WHERE lm.LEA = 
                                CASE WHEN CHARINDEX('-', w.WORK_GROUP) > 0 AND CHARINDEX('-', w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1) > 0
                                     THEN SUBSTRING(w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1, CHARINDEX('-', w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1) - CHARINDEX('-', w.WORK_GROUP) - 1)
                                     ELSE NULL END AND lm.OPMC IS NOT NULL AND lm.OPMC != '')
                        ) AS ResolvedOpmcName,
                        COALESCE(
                            (SELECT TOP 1 lm.PROVINCE FROM dbo.LEA_MAPPING lm WHERE lm.LEA = 
                                CASE WHEN CHARINDEX('-', w.WORK_GROUP) > 0 AND CHARINDEX('-', w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1) > 0
                                     THEN SUBSTRING(w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1, CHARINDEX('-', w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1) - CHARINDEX('-', w.WORK_GROUP) - 1)
                                     ELSE NULL END AND lm.PROVINCE IS NOT NULL),
                            (SELECT TOP 1 lm.PROVINCE FROM dbo.LEA_MAPPING lm WHERE lm.LEA = 
                                CASE WHEN CHARINDEX('-', w.WORK_GROUP) > 0 
                                     THEN SUBSTRING(w.WORK_GROUP, 1, CHARINDEX('-', w.WORK_GROUP) - 1)
                                     ELSE w.WORK_GROUP END AND lm.PROVINCE IS NOT NULL)
                        ) AS ResolvedProvince,
                        COALESCE(
                            (SELECT TOP 1 lm.REGION FROM dbo.LEA_MAPPING lm WHERE lm.LEA = 
                                CASE WHEN CHARINDEX('-', w.WORK_GROUP) > 0 AND CHARINDEX('-', w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1) > 0
                                     THEN SUBSTRING(w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1, CHARINDEX('-', w.WORK_GROUP, CHARINDEX('-', w.WORK_GROUP) + 1) - CHARINDEX('-', w.WORK_GROUP) - 1)
                                     ELSE NULL END AND lm.REGION IS NOT NULL),
                            (SELECT TOP 1 lm.REGION FROM dbo.LEA_MAPPING lm WHERE lm.LEA = 
                                CASE WHEN CHARINDEX('-', w.WORK_GROUP) > 0 
                                     THEN SUBSTRING(w.WORK_GROUP, 1, CHARINDEX('-', w.WORK_GROUP) - 1)
                                     ELSE w.WORK_GROUP END AND lm.REGION IS NOT NULL)
                        ) AS ResolvedRegion
                ) opmcData
                WHERE (
                    w.WO_STATUS IN ('OPEN', 'PENDING', 'IN PROGRESS', 'INPROGRESS', 'ASSIGNED', 'ACKNOWLEDGED', 'Acknowledged', 'Open', 'Pending', 'In Progress')
                    OR (UPPER(ISNULL(w.WO_STATUS, '')) NOT IN ('CLOSED', 'RESOLVED', 'CANCELLED') AND ISNULL(w.WO_STATUS, '') != '')
                    OR w.WO_STATUS IS NULL
                )
                AND ISNULL(w.AREA, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS')
                AND ISNULL(w.REGION, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS', 'SOC', 'Transport Networks', 'Network Services', 'DC & Cloud', 'IPTV', 'SME')
                AND ISNULL(w.PROVINCE, '') NOT LIKE '%HQ%'
                AND ISNULL(w.WORK_GROUP, '') NOT LIKE '%HQ%'
                AND (w.OPMC_NW = 'OPMC' OR w.WORK_GROUP LIKE '%-MDF' OR w.WORK_GROUP LIKE '%-CPE%' OR w.WORK_GROUP LIKE '%-CPE-MTCE')
                AND ISNULL(w.STARTED_TIME, f.reportedtime) <= @CutOffTime
                ORDER BY ISNULL(w.STARTED_TIME, f.reportedtime) DESC";

            Console.WriteLine($"[FaultReportService] Executing Type 2 SQL Query (TargetDate: {effectiveDate:yyyy-MM-dd}, CutOffTime: {cutOffTime:yyyy-MM-dd HH:mm:ss})...");

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd  = new SqlCommand(query, conn);
                cmd.Parameters.Add("@CutOffTime", SqlDbType.DateTime).Value = cutOffTime;

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
                        RecentUpdate = CleanCommentOrDescription(GetString(reader, "RecentUpdate", "No update"))
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
                    <th style=""{ThStyle}"">RELEVANT OPMC</th>
                    <th style=""{ThStyle}"">DGM</th>
                    <th style=""{ThStyle}"">GM</th>
                    <th style=""{ThStyle}"">Description</th>
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
            var effectiveDate = targetDate ?? await GetLatestDataDateAsync() ?? DateTime.Today;
            DateTime cutOffTime = effectiveDate.Date.AddHours(7);

            const string query = @"
                SELECT TOP 100
                    ROW_NUMBER() OVER(ORDER BY f.reportedtime DESC) AS No,
                    ISNULL(NULLIF(f.circuit_name, ''), 'N/A') AS DataCircuitId,
                    ISNULL(NULLIF(f.customer_name, ''), 'N/A') AS DetailsOfCustomer,
                    CONVERT(varchar(12), f.reportedtime, 101) AS FaultDate,
                    CONVERT(varchar(8),  f.reportedtime, 108) AS FaultTime,
                    ISNULL(NULLIF(f.workgroup, ''), ISNULL(NULLIF(w.WORK_GROUP, ''), 'N/A')) AS WorkOrdersHeldUpAt,
                    ISNULL(NULLIF(f.description, ''), 'N/A') AS Description,
                    CASE
                        WHEN f.reportedtime IS NULL THEN '00:00:00'
                        WHEN DATEDIFF(SECOND, f.reportedtime, @CutOffTime) < 0 THEN '00:00:00'
                        ELSE
                            CAST(DATEDIFF(SECOND, f.reportedtime, @CutOffTime) / 3600 AS VARCHAR) + ':' +
                            RIGHT('00' + CAST((DATEDIFF(SECOND, f.reportedtime, @CutOffTime) % 3600) / 60 AS VARCHAR), 2) + ':' +
                            RIGHT('00' + CAST(DATEDIFF(SECOND, f.reportedtime, @CutOffTime) % 60 AS VARCHAR), 2)
                    END AS DurationOfComplaint,
                    ISNULL(NULLIF(w.REGION, ''), 'N/A') AS Region,
                    ISNULL(NULLIF(w.PROVINCE, ''), 'N/A') AS Province
                FROM dbo.CLARITY_DATA_NW_FAULTS f
                LEFT JOIN dbo.CLARITY_WO_STATUS w ON w.CLARITY_REF = f.problem_no
                WHERE (
                    f.problem_status IN ('OPEN', 'PENDING', 'IN PROGRESS', 'INPROGRESS', 'ASSIGNED', 'ACKNOWLEDGED', 'Acknowledged', 'Open', 'Pending', 'In Progress')
                    OR (UPPER(ISNULL(f.problem_status, '')) NOT IN ('CLOSED', 'RESOLVED', 'CANCELLED') AND ISNULL(f.problem_status, '') != '')
                    OR f.problem_status IS NULL
                )
                AND (w.AREA IS NULL OR (
                    ISNULL(w.AREA, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS')
                    AND ISNULL(w.REGION, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS', 'SOC', 'Transport Networks', 'Network Services', 'DC & Cloud', 'IPTV', 'SME')
                    AND ISNULL(w.PROVINCE, '') NOT LIKE '%HQ%'
                    AND ISNULL(w.WORK_GROUP, '') NOT LIKE '%HQ%'
                ))
                AND f.reportedtime <= @CutOffTime
                ORDER BY f.reportedtime DESC";

            Console.WriteLine($"[FaultReportService] Executing Type 3 SQL Query (TargetDate: {effectiveDate:yyyy-MM-dd}, CutOffTime: {cutOffTime:yyyy-MM-dd HH:mm:ss})...");

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd  = new SqlCommand(query, conn);
                cmd.Parameters.Add("@CutOffTime", SqlDbType.DateTime).Value = cutOffTime;

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
            <p>Pending Data Faults Related to SLT NOC - Fault Management Teams as at 07:00 AM on <b>{dateStr}</b> are attached herewith please.</p>

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
            var cutOffTime = (targetDate ?? DateTime.Today).Date.AddHours(7);
            var windowStartTime = cutOffTime.AddDays(-1);
            var reportData = new Type4ReportData();

            // ─── TABLE 1: MSAN NETWORK FAILURES ──────────────────────────────────
            const string msanQuery = @"
                SELECT
                    ROW_NUMBER() OVER(ORDER BY f.FAULT_TIME DESC) AS [No],
                    f.PLATFORM AS [Network],
                    f.NODE AS [Node],
                    COALESCE(
                        (SELECT TOP 1 c.COMMENT_TEXT 
                         FROM dbo.COMMENTS c 
                         INNER JOIN dbo.T_TICKETS t ON c.TICKET_REF = t.ID 
                         WHERE t.NODE = f.NODE 
                         ORDER BY c.ID DESC),
                        (SELECT TOP 1 cc.description
                         FROM dbo.CLARITY_COMMENTS cc
                         WHERE cc.docket_id = CAST(f.REF_ID AS VARCHAR)
                         ORDER BY cc.event_time DESC),
                        LTRIM(RTRIM(CASE
                            WHEN CHARINDEX('If Memo=--', f.FAULT) > 0
                                THEN SUBSTRING(f.FAULT, CHARINDEX('If Memo=--', f.FAULT) + 11, LEN(f.FAULT))
                            WHEN CHARINDEX(':', f.FAULT) > 0 AND ISDATE(SUBSTRING(f.FAULT, 1, CHARINDEX(':', f.FAULT) - 1)) = 1
                                THEN SUBSTRING(f.FAULT, CHARINDEX(':', f.FAULT) + 1, LEN(f.FAULT))
                            ELSE f.FAULT
                        END))
                    ) AS [Description],
                    f.NW_ENG AS [NetworkEngineer],
                    f.PROVINCE AS [Province],
                    f.REGION AS [Region],
                    f.FAULT_TIME AS [FaultDetected],
                    f.FAULT_STATUS AS [FaultStatus]
                FROM FAULTS f
                WHERE f.FAULT_STATUS = 'OPEN'
                  AND f.FAULT_TIME >= @WindowStartTime AND f.FAULT_TIME <= @CutOffTime
                  AND f.SOURCEIP NOT LIKE '77%'
                  AND f.PLATFORM = 'MSAN'
                  AND (f.ALARM_TYPE = 'Node Down' OR f.ALARM_TYPE = 'Node Name' OR f.ALARM_TYPE LIKE '%Node%')
                  AND (ISNULL(f.REGION, '') NOT IN ('HQ','CENTRAL','HEADQUARTERS','HQ-NOC','IDC HQ NOC')
                       AND ISNULL(f.PROVINCE, '') NOT LIKE '%HQ%'
                       AND ISNULL(f.NW_ENG, '') NOT LIKE '%HQ%')
                ORDER BY f.FAULT_TIME DESC";

            // ─── TABLE 2: SLT 4G SITES DOWN ─────────────────────────────────────
            const string fourGQuery = @"
                SELECT
                    ROW_NUMBER() OVER(ORDER BY f.FAULT_TIME DESC) AS [No],
                    ISNULL(NULLIF(f.VENDOR, ''),
                        CASE
                            WHEN f.NODE LIKE '%HUAWEI%' OR f.PLATFORM LIKE '%HUAWEI%' THEN 'HUAWEI'
                            WHEN f.NODE LIKE '%ZTE%' OR f.PLATFORM LIKE '%ZTE%' THEN 'ZTE'
                            WHEN f.NODE LIKE '%NOKIA%' OR f.PLATFORM LIKE '%NOKIA%' THEN 'NOKIA'
                            WHEN f.NODE LIKE '%ERICSSON%' OR f.PLATFORM LIKE '%ERICSSON%' THEN 'ERICSSON'
                            ELSE 'UNKNOWN'
                        END) AS [Vendor],
                    CONVERT(VARCHAR(10), f.FAULT_TIME, 120) AS [Date],
                    f.NODE AS [Ne],
                    ISNULL(NULLIF(f.REGION, ''), 'N/A') AS [SubRegion],
                    COALESCE(
                        (SELECT TOP 1 c.COMMENT_TEXT 
                         FROM dbo.COMMENTS c 
                         INNER JOIN dbo.T_TICKETS t ON c.TICKET_REF = t.ID 
                         WHERE t.NODE = f.NODE 
                         ORDER BY c.ID DESC),
                        (SELECT TOP 1 cc.description
                         FROM dbo.CLARITY_COMMENTS cc
                         WHERE cc.docket_id = CAST(f.REF_ID AS VARCHAR)
                         ORDER BY cc.event_time DESC),
                        LTRIM(RTRIM(CASE
                            WHEN CHARINDEX('If Memo=--', f.FAULT) > 0
                                THEN SUBSTRING(f.FAULT, CHARINDEX('If Memo=--', f.FAULT) + 11, LEN(f.FAULT))
                            WHEN CHARINDEX(':', f.FAULT) > 0 AND ISDATE(SUBSTRING(f.FAULT, 1, CHARINDEX(':', f.FAULT) - 1)) = 1
                                THEN SUBSTRING(f.FAULT, CHARINDEX(':', f.FAULT) + 1, LEN(f.FAULT))
                            ELSE f.FAULT
                        END))
                    ) AS [Reason],
                    CASE 
                        WHEN DATEDIFF(DAY, f.FAULT_TIME, @CutOffTime) < 0 THEN 0 
                        ELSE DATEDIFF(DAY, f.FAULT_TIME, @CutOffTime) 
                    END AS [NoOfDays],
                    ISNULL(NULLIF(f.NW_ENG, ''), 'N/A') AS [AttendBy],
                    f.FAULT_TIME AS [FaultTime]
                FROM FAULTS f
                WHERE f.FAULT_STATUS = 'OPEN'
                  AND f.FAULT_TIME >= @WindowStartTime AND f.FAULT_TIME <= @CutOffTime
                  AND f.SOURCEIP NOT LIKE '77%'
                  AND f.PLATFORM IN ('LTE', '4G', '5G', 'MOBILE', 'RAN', 'SLT 4G')
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
                        WHEN CHARINDEX('If Memo=--', f.FAULT) > 0
                            THEN SUBSTRING(f.FAULT, CHARINDEX('If Memo=--', f.FAULT) + 11, LEN(f.FAULT))
                        WHEN CHARINDEX(':', f.FAULT) > 0 AND ISDATE(SUBSTRING(f.FAULT, 1, CHARINDEX(':', f.FAULT) - 1)) = 1
                            THEN SUBSTRING(f.FAULT, CHARINDEX(':', f.FAULT) + 1, LEN(f.FAULT))
                        ELSE f.FAULT
                    END)) AS [Description],
                    f.NW_ENG AS [Engineer],
                    f.PROVINCE AS [Province],
                    f.REGION AS [Region],
                    'DCRO' AS [ResponsibleGroup],
                    COALESCE(
                        (SELECT TOP 1 c.COMMENT_TEXT 
                         FROM dbo.COMMENTS c 
                         INNER JOIN dbo.T_TICKETS t ON c.TICKET_REF = t.ID 
                         WHERE t.NODE = f.NODE 
                         ORDER BY c.ID DESC),
                        (SELECT TOP 1 cc.description
                         FROM dbo.CLARITY_COMMENTS cc
                         WHERE cc.docket_id = CAST(f.REF_ID AS VARCHAR)
                         ORDER BY cc.event_time DESC),
                        LTRIM(RTRIM(CASE
                            WHEN CHARINDEX('If Memo=--', f.FAULT) > 0
                                THEN SUBSTRING(f.FAULT, CHARINDEX('If Memo=--', f.FAULT) + 11, LEN(f.FAULT))
                            WHEN CHARINDEX(':', f.FAULT) > 0 AND ISDATE(SUBSTRING(f.FAULT, 1, CHARINDEX(':', f.FAULT) - 1)) = 1
                                THEN SUBSTRING(f.FAULT, CHARINDEX(':', f.FAULT) + 1, LEN(f.FAULT))
                            ELSE f.FAULT
                        END))
                    ) AS [Remarks],
                    CONVERT(VARCHAR(10), f.FAULT_TIME, 120) AS [FaultOccurredDate],
                    CONVERT(VARCHAR(8), f.FAULT_TIME, 108) AS [FaultTime],
                    CASE 
                        WHEN DATEDIFF(HOUR, f.FAULT_TIME, @CutOffTime) < 0 THEN 0 
                        ELSE DATEDIFF(HOUR, f.FAULT_TIME, @CutOffTime) 
                    END AS [PendingHours],
                    f.FAULT_TIME AS [RawFaultTime]
                FROM FAULTS f
                WHERE f.FAULT_STATUS = 'OPEN'
                  AND f.FAULT_TIME >= @WindowStartTime AND f.FAULT_TIME <= @CutOffTime
                  AND f.SOURCEIP NOT LIKE '77%'
                  AND f.PLATFORM IN ('TX', 'TRANSMISSION', 'DWDM', 'SDH', 'SLBN')
                  AND (ISNULL(f.REGION, '') NOT IN ('HQ','CENTRAL','HEADQUARTERS','HQ-NOC','IDC HQ NOC')
                       AND ISNULL(f.PROVINCE, '') NOT LIKE '%HQ%'
                       AND ISNULL(f.NW_ENG, '') NOT LIKE '%HQ%')
                ORDER BY f.FAULT_TIME DESC";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // Execute Table 1: MSAN
                try
                {
                    using var cmd = new SqlCommand(msanQuery, conn);
                    cmd.Parameters.Add("@WindowStartTime", SqlDbType.DateTime).Value = windowStartTime;
                    cmd.Parameters.Add("@CutOffTime", SqlDbType.DateTime).Value = cutOffTime;
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var faultTime = GetDateTime(reader, "FaultDetected");
                        var status = GetString(reader, "FaultStatus");
                        int pendingHours = Math.Max(0, (int)(cutOffTime - faultTime).TotalHours);

                        // Calculate MSAN row color (aligned to legend palette)
                        string backgroundColor;
                        if (status != null && (status.Equals("MONITORING", StringComparison.OrdinalIgnoreCase) || status.Equals("UP", StringComparison.OrdinalIgnoreCase)))
                            backgroundColor = "#D9EAD3"; // Soft Green  → Up / Monitoring
                        else if (pendingHours > 8)
                            backgroundColor = "#E8C2C2"; // Soft Pink   → More than 8 hrs
                        else if (pendingHours >= 4)
                            backgroundColor = "#FFFF00"; // Yellow       → 4–8 Hours
                        else
                            backgroundColor = "#FFFFFF"; // White        → 0–4 Hours

                        string colorCode;
                        if (status != null && (status.Equals("MONITORING", StringComparison.OrdinalIgnoreCase) || status.Equals("UP", StringComparison.OrdinalIgnoreCase)))
                            colorCode = "GREEN";
                        else if (pendingHours > 8)
                            colorCode = "PINK";
                        else if (pendingHours >= 4)
                            colorCode = "YELLOW";
                        else
                            colorCode = "WHITE";

                        reportData.MsanFailures.Add(new MsanNetworkFailureRow
                        {
                            No = (int)GetInt64(reader, "No"),
                            Network = GetString(reader, "Network"),
                            Node = GetString(reader, "Node"),
                            Description = CleanCommentOrDescription(GetString(reader, "Description")),
                            NetworkEngineer = GetString(reader, "NetworkEngineer"),
                            Province = GetString(reader, "Province"),
                            Region = GetString(reader, "Region"),
                            FaultDetected = faultTime,
                            PendingHours = pendingHours,
                            ColorCode = colorCode,
                            BackgroundColor = backgroundColor
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FaultReportService] ERROR in Type 4 Table 1 (MSAN) query: {ex.Message}");
                }

                // Execute Table 2: SLT 4G
                try
                {
                    using var cmd = new SqlCommand(fourGQuery, conn);
                    cmd.Parameters.Add("@WindowStartTime", SqlDbType.DateTime).Value = windowStartTime;
                    cmd.Parameters.Add("@CutOffTime", SqlDbType.DateTime).Value = cutOffTime;
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var rawFaultTime = GetDateTime(reader, "FaultTime");
                        int noOfDays = rawFaultTime != DateTime.MinValue
                            ? Math.Max(0, (int)(cutOffTime.Date - rawFaultTime.Date).TotalDays)
                            : Math.Max(0, GetInt32(reader, "NoOfDays"));

                        reportData.Slt4GSitesDown.Add(new Slt4GSitesDownRow
                        {
                            No = (int)GetInt64(reader, "No"),
                            Vendor = GetString(reader, "Vendor"),
                            Date = GetString(reader, "Date"),
                            Ne = GetString(reader, "Ne"),
                            SubRegion = GetString(reader, "SubRegion"),
                            Reason = CleanCommentOrDescription(GetString(reader, "Reason")),
                            NoOfDays = noOfDays,
                            AttendBy = GetString(reader, "AttendBy")
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FaultReportService] ERROR in Type 4 Table 2 (4G/LTE) query: {ex.Message}");
                }

                // Execute Table 3: TX
                try
                {
                    using var cmd = new SqlCommand(txQuery, conn);
                    cmd.Parameters.Add("@WindowStartTime", SqlDbType.DateTime).Value = windowStartTime;
                    cmd.Parameters.Add("@CutOffTime", SqlDbType.DateTime).Value = cutOffTime;
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var rawFaultTime = GetDateTime(reader, "RawFaultTime");
                        int pendingHours = rawFaultTime != DateTime.MinValue
                            ? Math.Max(0, (int)(cutOffTime - rawFaultTime).TotalHours)
                            : Math.Max(0, GetInt32(reader, "PendingHours"));

                        reportData.TxRows.Add(new TxNetworkFailureRow
                        {
                            No = (int)GetInt64(reader, "No"),
                            Network = GetString(reader, "Network"),
                            Description = CleanCommentOrDescription(GetString(reader, "Description")),
                            Engineer = GetString(reader, "Engineer"),
                            Province = GetString(reader, "Province"),
                            Region = GetString(reader, "Region"),
                            ResponsibleGroup = GetString(reader, "ResponsibleGroup"),
                            Remarks = CleanCommentOrDescription(GetString(reader, "Remarks")),
                            FaultOccurredDate = GetString(reader, "FaultOccurredDate"),
                            FaultTime = GetString(reader, "FaultTime"),
                            PendingHours = pendingHours
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FaultReportService] ERROR in Type 4 Table 3 (TX) query: {ex.Message}");
                }

                Console.WriteLine($"[FaultReportService] Comprehensive Type 4 successfully processed {reportData.TotalCount} rows (MSAN: {reportData.MsanFailures.Count}, 4G: {reportData.Slt4GSitesDown.Count}, TX: {reportData.TxRows.Count}).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FaultReportService] ERROR in Comprehensive Type 4 connection: {ex.Message}");
            }

            return reportData;
        }

        // ────────────────────────────────────────────────────────────────────────
        // COMPREHENSIVE TYPE 4: Query TMS DB for MSAN, 4G Sites, and TX Failures to populate Type4EmailViewModel
        // ────────────────────────────────────────────────────────────────────────
        public async Task<Type4EmailViewModel> GetType4ReportDataAsync(DateTime? date = null)
        {
            var cutOffTime = (date ?? DateTime.Today).Date.AddHours(7);
            var windowStartTime = cutOffTime.AddDays(-1);
            var reportData = await GetType4ComprehensiveReportDataAsync(date);

            var viewModel = new Type4EmailViewModel
            {
                ReportDate = (date ?? DateTime.Today).ToString("dd MMM yyyy"),
                MsanFaults = reportData.MsanFailures.Select(m => new Type4MsanFaultItem
                {
                    No = m.No,
                    Network = m.Network,
                    Node = m.Node,
                    Description = m.Description,
                    Engineer = m.NetworkEngineer,
                    Province = m.Province,
                    Region = m.Region,
                    FaultDetectedDate = m.FaultDetected.ToString("yyyy-MM-dd HH:mm:ss"),
                    FaultDetected = m.FaultDetected,
                    PendingHours = m.PendingHours,
                    BackgroundColor = m.BackgroundColor
                }).ToList(),
                Slt4GFaults = reportData.Slt4GSitesDown.Select(s => new Type4Slt4GFaultItem
                {
                    No = s.No,
                    Vendor = s.Vendor,
                    Date = s.Date,
                    Ne = s.Ne,
                    SubRegion = s.SubRegion,
                    Reason = s.Reason,
                    NoOfDays = s.NoOfDays,
                    AttendBy = s.AttendBy
                }).ToList(),
                TxFaults = reportData.TxRows.Select(t => new Type4TxFaultItem
                {
                    No = t.No,
                    Network = t.Network,
                    Description = t.Description,
                    Engineer = t.Engineer,
                    Province = t.Province,
                    Region = t.Region,
                    ResponsibleGroup = t.ResponsibleGroup,
                    Remarks = t.Remarks,
                    FaultOccurredDate = t.FaultOccurredDate,
                    Time = t.FaultTime,
                    PendingHours = t.PendingHours
                }).ToList(),
                TotalMsanCount = reportData.MsanFailures.Count,
                TotalCeaNodeCount = reportData.TxRows.Count,
                TotalSlt4GCount = reportData.Slt4GSitesDown.Count
            };

            return viewModel;
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
        /// Calculates pending hours from fault time to cut-off time (or now)
        /// </summary>
        private int CalculatePendingHours(DateTime faultTime, DateTime? cutOffTime = null)
        {
            var refTime = cutOffTime ?? DateTime.Now;
            return Math.Max(0, (int)(refTime - faultTime).TotalHours);
        }

        /// <summary>
        /// Calculates number of days from fault time to cut-off time (for SLT 4G table)
        /// </summary>
        private int CalculateNoOfDays(DateTime faultTime, DateTime? cutOffTime = null)
        {
            var refTime = cutOffTime ?? DateTime.Now;
            return Math.Max(0, (int)(refTime.Date - faultTime.Date).TotalDays);
        }

        /// <summary>
        /// Gets MSAN-specific color code based on pending hours and fault status
        /// </summary>
        private string GetMsanColorCode(DateTime faultTime, string status, DateTime? cutOffTime = null)
        {
            if (status != null && (status.Equals("MONITORING", StringComparison.OrdinalIgnoreCase) || status.Equals("UP", StringComparison.OrdinalIgnoreCase)))
                return "#D9EAD3"; // Soft Green

            int pendingHours = CalculatePendingHours(faultTime, cutOffTime);
            if (pendingHours > 8)
                return "#E8C2C2"; // Soft Pink
            else if (pendingHours >= 4)
                return "#FFFF00"; // Yellow
            else
                return "#FFFFFF"; // White
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

            public int TotalMsanCount => MsanFailures.Count;
            public int TotalCeaNodeCount => TxRows.Count;
            public int TotalSlt4GCount => Slt4GSitesDown.Count;

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
                    int hours4G = row.NoOfDays * 24;
                    string bg = hours4G > 8 ? "#E8C2C2" : (hours4G >= 4 ? "#FFFF00" : "#FFFFFF");
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
                    string bg = row.PendingHours > 8 ? "#E8C2C2" : (row.PendingHours >= 4 ? "#FFFF00" : "#FFFFFF");
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

            string legendAndSummary = $@"
            <table style=""width:100%;margin-bottom:20px;border-collapse:collapse;"">
                <tr>
                    <td style=""vertical-align:top;width:50%;padding-right:20px;"">
                        <table style=""{TableStyle}margin-bottom:0;font-size:12px;"">
                            <tr><td style=""{TdStyle}padding:5px 10px;""><span style=""display:inline-block;width:30px;height:18px;background-color:#E8C2C2;border:1px solid #999;vertical-align:middle;margin-right:6px;""></span>More than 8hrs</td></tr>
                            <tr><td style=""{TdStyle}padding:5px 10px;""><span style=""display:inline-block;width:30px;height:18px;background-color:#FFFF00;border:1px solid #999;vertical-align:middle;margin-right:6px;""></span>4–8 Hours</td></tr>
                            <tr><td style=""{TdStyle}padding:5px 10px;""><span style=""display:inline-block;width:30px;height:18px;background-color:#FFFFFF;border:1px solid #999;vertical-align:middle;margin-right:6px;""></span>0–4 Hours</td></tr>
                            <tr><td style=""{TdStyle}padding:5px 10px;""><span style=""display:inline-block;width:30px;height:18px;background-color:#D9EAD3;border:1px solid #999;vertical-align:middle;margin-right:6px;""></span>Up / Monitoring</td></tr>
                        </table>
                    </td>
                    <td style=""vertical-align:top;width:50%;"">
                        <table style=""{TableStyle}margin-bottom:0;font-size:12px;"">
                            <thead>
                                <tr>
                                    <th colspan=""2"" style=""{ThStyle}background-color:#1a3c5e;color:#ffffff;text-align:center;"">Summary</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td style=""{TdStyle}text-align:left;padding:6px 10px;"">Number of MSAN</td>
                                    <td style=""{TdStyle}text-align:center;font-weight:bold;padding:6px 10px;"">{reportData.MsanFailures.Count}</td>
                                </tr>
                                <tr>
                                    <td style=""{TdStyle}text-align:left;padding:6px 10px;"">Number of CEA node</td>
                                    <td style=""{TdStyle}text-align:center;font-weight:bold;padding:6px 10px;"">{reportData.TxRows.Count}</td>
                                </tr>
                                <tr>
                                    <td style=""{TdStyle}text-align:left;padding:6px 10px;"">SLT 4G sites</td>
                                    <td style=""{TdStyle}text-align:center;font-weight:bold;padding:6px 10px;"">{reportData.Slt4GSitesDown.Count}</td>
                                </tr>
                            </tbody>
                        </table>
                    </td>
                </tr>
            </table>";

            string inner = $@"
            <p style=""font-size:14px;"">Dear All,</p>
            <p>Pending Network Failures as at 07:00 AM on <b>{dateStr}</b> are as follows.</p>

            {legendAndSummary}

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
            DateTime? parsedDate = null;
            if (DateTime.TryParse(date, out var pd))
            {
                parsedDate = pd;
            }
            var reportData = await GetType4ComprehensiveReportDataAsync(parsedDate);
            // Use the provided date string for the header (expected format: "dd MMM yyyy")
            string dateStr = parsedDate?.ToString("dd MMM yyyy") ?? date;

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
        /// Cleans raw clarity comments and fault messages:
        /// - Strips Clarity ticket / Fault ID prefixes (e.g., "46769114 : ", "FAULT2026091869809: ")
        /// - Strips user / workgroup bracket tags (e.g., "[SLT018399] ", "[INT-NW-SYSADMIN]: ")
        /// - Strips trailing Clarity / Fault reference tags (e.g., ": FAULT2026091870424")
        /// - Converts Clarity system messages (e.g., "NEW DOCKET CREATED: 46769167") to clean summaries
        /// - Strips technical alarm headers (e.g., "MUT_LOS:... : ", "RELAY_ALARM_CRITICAL:... : ")
        /// - Strips leading timestamps and standard memo indicators ("If Memo=--")
        /// Returns only the pure comment or actionable description text.
        /// </summary>
        private static string CleanCommentOrDescription(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "No update";

            string t = raw.Trim();

            // 1. Remove date/timestamp prefixes (e.g. "2026-09-30 10:15:00 - " or "09/18/2026 14:35:47 : ")
            t = Regex.Replace(t, @"^\d{1,4}[-/]\d{1,2}[-/]\d{1,4}\s+\d{1,2}:\d{2}(?::\d{2})?\s*[-–:]\s*", "");

            // 2. Handle Clarity docket/work-order creation system messages
            var docketMatch = Regex.Match(t, @"^NEW DOCKET CREATED(?:\s*:\s*\d+)?$", RegexOptions.IgnoreCase);
            if (docketMatch.Success)
                return "Docket created";

            var workOrderMatch = Regex.Match(t, @"^NEW WORK ORDER CREATED\s*\[([^\]]+)\](?:\s*:\s*(?:FAULT\s*\d+|\d+))?$", RegexOptions.IgnoreCase);
            if (workOrderMatch.Success)
                return $"Work order created ({workOrderMatch.Groups[1].Value.Trim()})";

            var workOrderClosedMatch = Regex.Match(t, @"^\[([^\]]+)\]\s*WORK ORDER CLOSED(?:\s*:\s*(?:FAULT\s*\d+|\d+))?$", RegexOptions.IgnoreCase);
            if (workOrderClosedMatch.Success)
                return $"Work order closed ({workOrderClosedMatch.Groups[1].Value.Trim()})";

            // 3. Remove leading Clarity ticket / Fault ID: e.g. '46769114 : ' or 'FAULT2026091869809: '
            t = Regex.Replace(t, @"^(?:FAULT\s*\d+|\d{7,10})\s*[:\-\s]\s*", "", RegexOptions.IgnoreCase);

            // 4. Remove leading officer/workgroup bracket: e.g. '[SLT018399] ' or '[INT-NW-SYSADMIN]: '
            t = Regex.Replace(t, @"^\[[^\]]+\]\s*:?\s*", "");

            // 5. Check if another ticket prefix follows the bracket: e.g. '[INT-NW-SYSADMIN]: FAULT...: '
            t = Regex.Replace(t, @"^(?:FAULT\s*\d+|\d{7,10})\s*[:\-\s]\s*", "", RegexOptions.IgnoreCase);

            // 6. Remove trailing Clarity/Fault references: e.g. ': FAULT2026091870424' or ': 46769167'
            t = Regex.Replace(t, @"\s*[:\-\s]\s*(?:FAULT\s*\d+|\d{7,10})\s*$", "", RegexOptions.IgnoreCase);

            // 7. Technical alarm string with multiple colons (for Type 4 alarms like MUT_LOS, RELAY_ALARM, NE_NOT_LOGIN, etc.)
            if (t.Contains(" : ") && (t.Contains("MUT_LOS") || t.Contains("RELAY_ALARM") || t.Contains("NE_NOT_LOGIN") || t.Contains("POWER_FAIL") || t.Contains("source=") || t.Contains("If Memo=--")))
            {
                var parts = t.Split(new[] { " : " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                    t = parts[^1].Trim();
            }

            // 8. Clean any remaining 'If Memo=--' markers
            if (t.Contains("If Memo=--"))
            {
                int idx = t.IndexOf("If Memo=--") + 11;
                if (idx < t.Length)
                    t = t.Substring(idx).Trim();
            }

            // 9. Remove standard prefixes like "CLARITY:", "UPDATE:", "REMARK:", "NOTE:"
            t = Regex.Replace(t, @"^(CLARITY|UPDATE|REMARK|NOTE):\s*", "", RegexOptions.IgnoreCase);

            // 10. Clean any leading colon, dash or space leftover
            t = Regex.Replace(t, @"^[:\-\s]+", "");

            return string.IsNullOrWhiteSpace(t) ? "No update" : t.Trim();
        }

        private static string CleanDescription(string raw)
        {
            return CleanCommentOrDescription(raw);
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
                    <th style=""{ThStyle}"">RELEVANT OPMC</th>
                    <th style=""{ThStyle}"">DGM</th>
                    <th style=""{ThStyle}"">GM</th>
                    <th style=""{ThStyle}"">Description</th>
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