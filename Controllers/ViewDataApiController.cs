using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using AlarmSystem.Services;
using System.Data;

namespace AlarmSystem.Controllers
{
    [ApiController]
    [Route("api/viewdata")]
    public class ViewDataApiController : ControllerBase
    {
        private readonly SqlConnection _con;

        public class UpdateSectionRowRequest
        {
            public Dictionary<string, string?>? OriginalRow { get; set; }
            public Dictionary<string, string?>? UpdatedRow { get; set; }
        }

        public class DeleteSectionRowRequest
        {
            public Dictionary<string, string?>? Row { get; set; }
        }

        public class AddSectionRowRequest
        {
            public Dictionary<string, string?>? Row { get; set; }
        }

        public ViewDataApiController(SqlConnection con)
        {
            _con = con;
        }

        [HttpGet("section/{sectionKey}")]
        public async Task<IActionResult> GetSection([FromRoute] string sectionKey, [FromQuery] string? search, [FromQuery] int top = 10000)
        {
            if (top < 1) top = 50;
            if (top > 20000) top = 20000;

            var normalizedSectionKey = NormalizeSectionKey(sectionKey);
            var searchLike = string.IsNullOrWhiteSpace(search)
                ? string.Empty
                : $"%{search.Trim().ToLowerInvariant()}%";

            try
            {
                await _con.OpenAsync();

                var coordinateColumns = await GetCoordinateColumnsAsync(_con);

                object? payload = normalizedSectionKey switch
                {
                    "realtime-alarms" => await BuildRealtimeAlarmsSectionAsync(searchLike, top),
                    "customer-fault-oss" => await BuildCustomerFaultSectionAsync(searchLike, top, coordinateColumns),
                    "customer-details" => await BuildCustomerDetailsSectionAsync(searchLike, top, coordinateColumns),
                    "network-element-details" => await BuildNetworkElementSectionAsync(searchLike, top, coordinateColumns),
                    _ => null
                };

                if (payload is null)
                    return NotFound(new { message = $"Unknown view data section '{sectionKey}'." });

                return Ok(payload);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error reading view data section",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        [HttpPost("section/{sectionKey}")]
        public async Task<IActionResult> AddSectionRow([FromRoute] string sectionKey, [FromBody] AddSectionRowRequest? request)
        {
            if (request?.Row is null || request.Row.Count == 0)
                return BadRequest(new { message = "Row data is required." });

            var normalizedSectionKey = NormalizeSectionKey(sectionKey);
            if (normalizedSectionKey != "customer-details" && normalizedSectionKey != "network-element-details")
                return BadRequest(new { message = "Add is only supported for Customer Details and Network Element Details." });

            try
            {
                await _con.OpenAsync();
                var coordinateColumns = await GetCoordinateColumnsAsync(_con);

                var mapping = normalizedSectionKey == "customer-details"
                    ? BuildCustomerDetailsUpdateMappings(coordinateColumns)
                    : BuildNetworkElementUpdateMappings(coordinateColumns);

                var columns = new List<string>();
                var values = new List<string>();
                using var cmd = new SqlCommand();
                var paramIndex = 0;

                foreach (var mapping2 in mapping)
                {
                    if (!request.Row.TryGetValue(mapping2.Key, out var val)) continue;
                    var normalized = NormalizeInputValue(val);
                    if (string.IsNullOrWhiteSpace(normalized)) continue;

                    var paramName = $"@p{paramIndex++}";
                    columns.Add(BracketIdentifier(mapping2.Value));
                    values.Add(paramName);
                    cmd.Parameters.Add(paramName, SqlDbType.NVarChar, 4000).Value = normalized;
                }

                if (columns.Count == 0)
                    return BadRequest(new { message = "No recognizable fields provided." });

                cmd.Connection = _con;
                cmd.CommandText = $"INSERT INTO dbo.coordinates ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)});";
                await cmd.ExecuteNonQueryAsync();

                return Ok(new { message = "Record added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding record.", error = ex.Message });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        [HttpPut("section/{sectionKey}")]
        public async Task<IActionResult> UpdateSectionRow([FromRoute] string sectionKey, [FromBody] UpdateSectionRowRequest? request)
        {
            if (request?.OriginalRow is null || request.UpdatedRow is null)
                return BadRequest(new { message = "Original and updated row data are required." });

            var normalizedSectionKey = NormalizeSectionKey(sectionKey);
            if (normalizedSectionKey != "customer-details" && normalizedSectionKey != "network-element-details")
                return BadRequest(new { message = "Editing is only supported for Customer Details and Network Element Details." });

            try
            {
                await _con.OpenAsync();
                var coordinateColumns = await GetCoordinateColumnsAsync(_con);

                var mapping = normalizedSectionKey == "customer-details"
                    ? BuildCustomerDetailsUpdateMappings(coordinateColumns)
                    : BuildNetworkElementUpdateMappings(coordinateColumns);

                var affectedRows = await UpdateCoordinatesRowAsync(_con, request.OriginalRow, request.UpdatedRow, mapping);
                if (affectedRows < 1)
                {
                    var fallbackWhereKeys = normalizedSectionKey == "customer-details"
                        ? new[] { "cr", "circuitId", "account" }
                        : new[] { "circuitId", "cr", "eqNode", "ipAddress" };

                    affectedRows = await UpdateCoordinatesRowWithKeyFallbackAsync(
                        _con,
                        request.OriginalRow,
                        request.UpdatedRow,
                        mapping,
                        fallbackWhereKeys);
                }

                if (affectedRows < 1)
                    return NotFound(new { message = "No matching record was found to update." });

                return Ok(new
                {
                    message = "Record updated successfully.",
                    affectedRows
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error updating record.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        [HttpDelete("section/{sectionKey}")]
        public async Task<IActionResult> DeleteSectionRow([FromRoute] string sectionKey, [FromBody] DeleteSectionRowRequest? request)
        {
            if (request?.Row is null)
                return BadRequest(new { message = "Row data is required." });

            var normalizedSectionKey = NormalizeSectionKey(sectionKey);
            if (normalizedSectionKey != "customer-details" && normalizedSectionKey != "network-element-details")
                return BadRequest(new { message = "Delete is only supported for Customer Details and Network Element Details." });

            try
            {
                await _con.OpenAsync();
                var coordinateColumns = await GetCoordinateColumnsAsync(_con);

                var mapping = normalizedSectionKey == "customer-details"
                    ? BuildCustomerDetailsUpdateMappings(coordinateColumns)
                    : BuildNetworkElementUpdateMappings(coordinateColumns);

                var whereKeys = normalizedSectionKey == "customer-details"
                    ? new[] { "cr", "circuitId", "account" }
                    : new[] { "circuitId", "cr", "eqNode", "ipAddress" };

                var affectedRows = await DeleteCoordinatesRowAsync(_con, request.Row, mapping, whereKeys);

                if (affectedRows < 1)
                    return NotFound(new { message = "No matching record was found to delete." });

                return Ok(new { message = "Record deleted successfully.", affectedRows });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting record.", error = ex.Message });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        private async Task<object> BuildRealtimeAlarmsSectionAsync(string searchLike, int top)
        {
            using var cmd = new SqlCommand("dbo.usp_ViewData_RealtimeAlarms", _con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
            cmd.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 2000).Value = searchLike;

            using var reader = await cmd.ExecuteReaderAsync();

            var rows = new List<Dictionary<string, string?>>();
            var totalCount = 0;
            string? lastUpdated = null;

            while (await reader.ReadAsync())
            {
                var alarmTime = ReadString(reader, "AlarmTime");
                lastUpdated ??= alarmTime;

                rows.Add(CreateRow(
                    ("severity", ReadString(reader, "Severity") ?? "General"),
                    ("alarmTime", alarmTime),
                    ("faultKey", ReadString(reader, "FaultKey")),
                    ("nodeName", ReadString(reader, "NodeName")),
                    ("region", ReadString(reader, "Region")),
                    ("province", ReadString(reader, "Province")),
                    ("alarmText", ReadString(reader, "AlarmText")),
                    ("alarmTextFull", ReadString(reader, "AlarmTextFull"))));
            }

            totalCount = rows.Count;

            return CreateSectionResponse(
                title: "Realtime Alarm",
                subtitle: "Current live network alarms ordered by most recent event time.",
                sourceLabel: "Source: NW_fault",
                totalCount: totalCount,
                lastUpdated: lastUpdated ?? "No alarm timestamp available",
                emptyMessage: "No realtime alarms match the current search.",
                columns: CreateColumns(
                    ("severity", "Severity", false, true, false),
                    ("alarmTime", "Alarm Time", true, false, false),
                    ("faultKey", "Circuit ID", true, false, false),
                    ("nodeName", "Node", false, false, false),
                    ("region", "Region", false, false, false),
                    ("province", "Province", false, false, false),
                    ("alarmText", "Alarm Details", false, false, false)),
                rows: rows);
        }

        private async Task<object> BuildCustomerFaultSectionAsync(string searchLike, int top, Dictionary<string, string> coordinateColumns)
        {
            using var cmd = new SqlCommand("dbo.usp_ViewData_CustomerFaultOss", _con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
            cmd.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 2000).Value = searchLike;

            using var reader = await cmd.ExecuteReaderAsync();

            var rows = new List<Dictionary<string, string?>>();
            var totalCount = 0;
            string? lastUpdated = null;

            while (await reader.ReadAsync())
            {
                var reportedTime = ReadString(reader, "ReportedTime");
                lastUpdated ??= reportedTime;

                rows.Add(CreateRow(
                    ("reportedTime", reportedTime),
                    ("problemNo", ReadString(reader, "ProblemNo")),
                    ("cr", ReadString(reader, "CR")),
                    ("circuitName", ReadString(reader, "CircuitName")),
                    ("customerName", ReadString(reader, "CustomerName")),
                    ("account", ReadString(reader, "Account")),
                    ("circuitStatus", ReadString(reader, "CircuitStatus")),
                    ("customerAddress", ReadString(reader, "CustomerAddress")),
                    ("faultDescription", ReadString(reader, "FaultDescription"))));
            }

            totalCount = rows.Count;

            return CreateSectionResponse(
                title: "Customer Fault (OSS)",
                subtitle: "Customer-fault records loaded directly from cx_fault, with optional enrichment when available.",
                sourceLabel: "Source: cx_fault (enriched by vw_cx_fault_full, coordinates)",
                totalCount: totalCount,
                lastUpdated: lastUpdated ?? "No customer fault timestamp available",
                emptyMessage: "No customer OSS faults match the current search.",
                columns: CreateColumns(
                    ("reportedTime", "Reported Time", true, false, false),
                    ("problemNo", "Problem No", true, false, false),
                    ("cr", "CR", true, false, false),
                    ("circuitName", "Circuit", true, false, false),
                    ("customerName", "Customer Name", false, false, false),
                    ("account", "Account", true, false, false),
                    ("circuitStatus", "Status", false, true, false),
                    ("customerAddress", "Customer Address", false, false, false),
                    ("faultDescription", "Fault Description", false, false, false)),
                rows: rows);
        }

        private async Task<object> BuildCustomerDetailsSectionAsync(string searchLike, int top, Dictionary<string, string> coordinateColumns)
        {
            using var cmd = new SqlCommand("dbo.usp_ViewData_CustomerDetails", _con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
            cmd.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 2000).Value = searchLike;

            using var reader = await cmd.ExecuteReaderAsync();

            var rows = new List<Dictionary<string, string?>>();
            var totalCount = 0;

            while (await reader.ReadAsync())
            {
                rows.Add(CreateRow(
                    ("customerName", ReadString(reader, "CustomerName")),
                    ("account", ReadString(reader, "Account")),
                    ("customerAddress", ReadString(reader, "CustomerAddress")),
                    ("cr", ReadString(reader, "CR")),
                    ("circuitId", ReadString(reader, "CircuitId")),
                    ("serviceName", ReadString(reader, "ServiceName")),
                    ("circuitStatus", ReadString(reader, "CircuitStatus")),
                    ("rGm", ReadString(reader, "RGm")),
                    ("pDgm", ReadString(reader, "PDgm"))));
            }

            totalCount = rows.Count;
            var generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return CreateSectionResponse(
                title: "Customer Details",
                subtitle: "Customer-related coordinate records kept separate from network element metadata.",
                sourceLabel: "Source: coordinates",
                totalCount: totalCount,
                lastUpdated: generatedAt,
                emptyMessage: "No customer detail records match the current search.",
                columns: CreateColumns(
                    ("customerName", "Customer Name", false, false, false),
                    ("account", "Account", true, false, false),
                    ("customerAddress", "Customer Address", false, false, false),
                    ("cr", "CR", true, false, false),
                    ("circuitId", "Circuit", true, false, false),
                    ("serviceName", "Service", false, false, false),
                    ("circuitStatus", "Status", false, true, false),
                    ("rGm", "R-GM", false, false, false),
                    ("pDgm", "P-DGM", false, false, false)),
                rows: rows);
        }

        private async Task<object> BuildNetworkElementSectionAsync(string searchLike, int top, Dictionary<string, string> coordinateColumns)
        {
            using var cmd = new SqlCommand("dbo.usp_ViewData_NetworkElementDetails", _con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
            cmd.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 2000).Value = searchLike;

            using var reader = await cmd.ExecuteReaderAsync();

            var rows = new List<Dictionary<string, string?>>();
            var totalCount = 0;

            while (await reader.ReadAsync())
            {
                rows.Add(CreateRow(
                    ("eqNode", ReadString(reader, "EqNode")),
                    ("ipAddress", ReadString(reader, "IpAddress")),
                    ("circuitId", ReadString(reader, "CircuitId")),
                    ("cr", ReadString(reader, "CR")),
                    ("rGm", ReadString(reader, "RGm")),
                    ("pDgm", ReadString(reader, "PDgm")),
                    ("ceaLatitude", ReadString(reader, "CeaLatitude")),
                    ("ceaLongitude", ReadString(reader, "CeaLongitude"))));
            }

            totalCount = rows.Count;
            var generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return CreateSectionResponse(
                title: "Network Element Details",
                subtitle: "Network element metadata separated from customer records using the coordinate table.",
                sourceLabel: "Source: coordinates",
                totalCount: totalCount,
                lastUpdated: generatedAt,
                emptyMessage: "No network element detail records match the current search.",
                columns: CreateColumns(
                    ("eqNode", "EQ Node", true, false, false),
                    ("ipAddress", "IP Address", true, false, false),
                    ("circuitId", "Circuit", true, false, false),
                    ("cr", "CR", true, false, false),
                    ("rGm", "R-GM", false, false, false),
                    ("pDgm", "P-DGM", false, false, false),
                    ("ceaLatitude", "CEA Lat", true, false, false),
                    ("ceaLongitude", "CEA Lng", true, false, false)),
                rows: rows);
        }

        private static object CreateSectionResponse(
            string title,
            string subtitle,
            string sourceLabel,
            int totalCount,
            string lastUpdated,
            string emptyMessage,
            List<object> columns,
            List<Dictionary<string, string?>> rows)
        {
            return new
            {
                title,
                subtitle,
                sourceLabel,
                totalCount,
                showingCount = rows.Count,
                lastUpdated,
                emptyMessage,
                columns,
                rows
            };
        }

        private static List<object> CreateColumns(params (string Key, string Label, bool Mono, bool Pill, bool Truncate)[] columns)
        {
            return columns
                .Select(column => (object)new
                {
                    key = column.Key,
                    label = column.Label,
                    mono = column.Mono,
                    pill = column.Pill,
                    truncate = column.Truncate
                })
                .ToList();
        }

        private static Dictionary<string, string?> CreateRow(params (string Key, string? Value)[] values)
        {
            return values.ToDictionary(value => value.Key, value => value.Value);
        }

        private static async Task<Dictionary<string, string>> GetCoordinateColumnsAsync(SqlConnection con)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            const string sql = @"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'coordinates';";

            using var cmd = new SqlCommand(sql, con);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var name = reader["COLUMN_NAME"]?.ToString();
                if (!string.IsNullOrWhiteSpace(name) && !map.ContainsKey(name))
                    map[name] = name;
            }

            return map;
        }

        private static string BuildCoordinateValueExpression(
            Dictionary<string, string> columns,
            IEnumerable<string> candidates,
            string tableAlias,
            string fallbackExpression = "CAST(NULL AS nvarchar(4000))")
        {
            foreach (var candidate in candidates)
            {
                if (columns.TryGetValue(candidate, out var actualColumn))
                    return $"TRY_CONVERT(nvarchar(4000), {tableAlias}.{BracketIdentifier(actualColumn)})";

                var normalizedCandidate = NormalizeIdentifier(candidate);
                foreach (var kv in columns)
                {
                    if (NormalizeIdentifier(kv.Key) != normalizedCandidate) continue;
                    return $"TRY_CONVERT(nvarchar(4000), {tableAlias}.{BracketIdentifier(kv.Value)})";
                }
            }

            return fallbackExpression;
        }

        private static Dictionary<string, string> BuildCustomerDetailsUpdateMappings(Dictionary<string, string> coordinateColumns)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            AddResolvedCoordinateColumn(map, "customerName", coordinateColumns, new[] { "CUSR_NAME", "CUSTOMER_NAME", "CUSTOMER NAME", "CUST_NAME", "CUSTOMER", "OPERATOR", "CARRIER", "PROVIDER" });
            AddResolvedCoordinateColumn(map, "account", coordinateColumns, new[] { "ACCOUNT", "ACCOUNT_NO", "ACCOUNT NUMBER", "ACCOUNT_ID", "AC_NO", "AC NUMBER" });
            AddResolvedCoordinateColumn(map, "customerAddress", coordinateColumns, new[] { "CUSR_ADDRESS", "CUSTOMER_ADDRESS", "CUSTOMER ADDRESS", "ADDRESS", "CUST_ADDRESS", "SERVICE_ADDRESS", "LOCATION_ADDRESS", "BENDADDRESS" });
            AddResolvedCoordinateColumn(map, "cr", coordinateColumns, new[] { "CR" });
            AddResolvedCoordinateColumn(map, "circuitId", coordinateColumns, new[] { "CCT" });
            AddResolvedCoordinateColumn(map, "circuitStatus", coordinateColumns, new[] { "CIRCUIT_STATUS", "CIRCUIT STATUS", "SERVICE_STATUS", "STATUS", "CCT_STATUS", "CIRT_STATUS" });
            AddResolvedCoordinateColumn(map, "rGm", coordinateColumns, new[] { "R-GM", "R_GM", "R GM" });
            AddResolvedCoordinateColumn(map, "pDgm", coordinateColumns, new[] { "P-DGM", "P_DGM", "P DGM" });
            return map;
        }

        private static Dictionary<string, string> BuildNetworkElementUpdateMappings(Dictionary<string, string> coordinateColumns)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            AddResolvedCoordinateColumn(map, "eqNode", coordinateColumns, new[] { "EQ_LOCATION_NODE", "EQ LOCATION NODE", "EQ_NODE", "EQ NODE", "NODE" });
            AddResolvedCoordinateColumn(map, "ipAddress", coordinateColumns, new[] { "IP_ADDRESS", "IP ADDRESS", "IP", "IPADDRESS", "IP-ADDRESS" });
            AddResolvedCoordinateColumn(map, "circuitId", coordinateColumns, new[] { "CCT" });
            AddResolvedCoordinateColumn(map, "cr", coordinateColumns, new[] { "CR" });
            AddResolvedCoordinateColumn(map, "rGm", coordinateColumns, new[] { "R-GM", "R_GM", "R GM" });
            AddResolvedCoordinateColumn(map, "pDgm", coordinateColumns, new[] { "P-DGM", "P_DGM", "P DGM" });
            AddResolvedCoordinateColumn(map, "ceaLatitude", coordinateColumns, new[] { "CEA Node- latitude", "CEA NODE LATITUDE", "CEA_LATITUDE", "CEA LATITUDE" });
            AddResolvedCoordinateColumn(map, "ceaLongitude", coordinateColumns, new[] { "CEA Node- longitude", "CEA NODE LONGITUDE", "CEA_LONGITUDE", "CEA LONGITUDE" });
            return map;
        }

        private static void AddResolvedCoordinateColumn(Dictionary<string, string> target, string key, Dictionary<string, string> coordinateColumns, IEnumerable<string> candidates)
        {
            var actualColumn = ResolveCoordinateColumn(coordinateColumns, candidates);
            if (!string.IsNullOrWhiteSpace(actualColumn))
                target[key] = actualColumn;
        }

        private static string? ResolveCoordinateColumn(Dictionary<string, string> columns, IEnumerable<string> candidates)
        {
            foreach (var candidate in candidates)
            {
                if (columns.TryGetValue(candidate, out var actualColumn))
                    return actualColumn;

                var normalizedCandidate = NormalizeIdentifier(candidate);
                foreach (var kv in columns)
                {
                    if (NormalizeIdentifier(kv.Key) == normalizedCandidate)
                        return kv.Value;
                }
            }

            return null;
        }

        private static async Task<int> UpdateCoordinatesRowAsync(
            SqlConnection con,
            Dictionary<string, string?> originalRow,
            Dictionary<string, string?> updatedRow,
            Dictionary<string, string> columnMap)
        {
            var setClauses = new List<string>();
            var whereClauses = new List<string>();
            var handledColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var cmd = new SqlCommand();
            var parameterIndex = 0;

            foreach (var mapping in columnMap)
            {
                if (!updatedRow.TryGetValue(mapping.Key, out var updatedValue))
                    continue;

                if (!handledColumns.Add(mapping.Value))
                    continue;

                var parameterName = $"@set{parameterIndex++}";
                setClauses.Add($"{BracketIdentifier(mapping.Value)} = {parameterName}");
                cmd.Parameters.Add(parameterName, SqlDbType.NVarChar, 4000).Value = (object?)NormalizeInputValue(updatedValue) ?? DBNull.Value;
            }

            if (setClauses.Count == 0)
                return 0;

            foreach (var mapping in columnMap)
            {
                if (!originalRow.TryGetValue(mapping.Key, out var originalValue))
                    continue;

                var parameterName = $"@where{parameterIndex++}";
                whereClauses.Add($"ISNULL(LTRIM(RTRIM(TRY_CONVERT(nvarchar(4000), {BracketIdentifier(mapping.Value)}))), '') = ISNULL(LTRIM(RTRIM({parameterName})), '')");
                cmd.Parameters.Add(parameterName, SqlDbType.NVarChar, 4000).Value = (object?)NormalizeInputValue(originalValue) ?? string.Empty;
            }

            if (whereClauses.Count == 0)
                return 0;

            cmd.Connection = con;
            cmd.CommandText = $@"
UPDATE dbo.coordinates
SET {string.Join(", ", setClauses)}
WHERE {string.Join(" AND ", whereClauses)};";

            return await cmd.ExecuteNonQueryAsync();
        }

        private static async Task<int> UpdateCoordinatesRowWithKeyFallbackAsync(
            SqlConnection con,
            Dictionary<string, string?> originalRow,
            Dictionary<string, string?> updatedRow,
            Dictionary<string, string> columnMap,
            IEnumerable<string> preferredWhereKeys)
        {
            var setClauses = new List<string>();
            var whereClauses = new List<string>();
            var handledSetColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var cmd = new SqlCommand();
            var parameterIndex = 0;

            foreach (var mapping in columnMap)
            {
                if (!updatedRow.TryGetValue(mapping.Key, out var updatedValue))
                    continue;

                if (!handledSetColumns.Add(mapping.Value))
                    continue;

                var parameterName = $"@set{parameterIndex++}";
                setClauses.Add($"{BracketIdentifier(mapping.Value)} = {parameterName}");
                cmd.Parameters.Add(parameterName, SqlDbType.NVarChar, 4000).Value = (object?)NormalizeInputValue(updatedValue) ?? DBNull.Value;
            }

            if (setClauses.Count == 0)
                return 0;

            foreach (var key in preferredWhereKeys)
            {
                if (!columnMap.TryGetValue(key, out var columnName))
                    continue;

                if (!originalRow.TryGetValue(key, out var originalValue))
                    continue;

                var normalizedOriginal = NormalizeInputValue(originalValue);
                if (string.IsNullOrWhiteSpace(normalizedOriginal))
                    continue;

                var parameterName = $"@where{parameterIndex++}";
                whereClauses.Add($"ISNULL(LTRIM(RTRIM(TRY_CONVERT(nvarchar(4000), {BracketIdentifier(columnName)}))), '') = ISNULL(LTRIM(RTRIM({parameterName})), '')");
                cmd.Parameters.Add(parameterName, SqlDbType.NVarChar, 4000).Value = normalizedOriginal;
            }

            if (whereClauses.Count == 0)
                return 0;

            cmd.Connection = con;
            cmd.CommandText = $@"
UPDATE dbo.coordinates
SET {string.Join(", ", setClauses)}
WHERE {string.Join(" AND ", whereClauses)};";

            return await cmd.ExecuteNonQueryAsync();
        }

        private static async Task<int> DeleteCoordinatesRowAsync(
            SqlConnection con,
            Dictionary<string, string?> row,
            Dictionary<string, string> columnMap,
            IEnumerable<string> whereKeys)
        {
            var whereClauses = new List<string>();
            using var cmd = new SqlCommand();
            var parameterIndex = 0;

            foreach (var key in whereKeys)
            {
                if (!columnMap.TryGetValue(key, out var columnName)) continue;
                if (!row.TryGetValue(key, out var value)) continue;
                var normalized = NormalizeInputValue(value);
                if (string.IsNullOrWhiteSpace(normalized)) continue;

                var paramName = $"@where{parameterIndex++}";
                whereClauses.Add($"ISNULL(LTRIM(RTRIM(TRY_CONVERT(nvarchar(4000), {BracketIdentifier(columnName)}))), '') = ISNULL(LTRIM(RTRIM({paramName})), '')");
                cmd.Parameters.Add(paramName, SqlDbType.NVarChar, 4000).Value = normalized;
            }

            if (whereClauses.Count == 0) return 0;

            cmd.Connection = con;
            cmd.CommandText = $"DELETE FROM dbo.coordinates WHERE {string.Join(" AND ", whereClauses)};";
            return await cmd.ExecuteNonQueryAsync();
        }

        private static string? NormalizeInputValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return value.Trim();
        }

        private static string NullIfTrim(string expression)
        {
            return $"NULLIF(LTRIM(RTRIM({expression})), '')";
        }

        private static string BracketIdentifier(string value) => $"[{value.Replace("]", "]]", StringComparison.Ordinal)}]";

        private static string NormalizeIdentifier(string value)
        {
            var chars = value.Where(char.IsLetterOrDigit).ToArray();
            return new string(chars).ToUpperInvariant();
        }

        private static string NormalizeSectionKey(string value)
        {
            return value.Trim().ToLowerInvariant();
        }

        private static string? ReadString(IDataRecord record, string columnName)
        {
            for (var i = 0; i < record.FieldCount; i++)
            {
                if (!string.Equals(record.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (record.IsDBNull(i)) return null;
                return record.GetValue(i)?.ToString();
            }

            return null;
        }
    }
}