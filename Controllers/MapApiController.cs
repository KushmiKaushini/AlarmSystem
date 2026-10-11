using AlarmSystem.Models.Map;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text;

namespace AlarmSystem.Controllers
{
    [ApiController]
    [Route("api/map")]
    public class MapApiController : ControllerBase, System.IDisposable
    {
        private const int DefaultCustomerFaultLookbackDays = 7;
        private static readonly DateTime DefaultNetworkFaultFromDate = new(1900, 1, 1);

        private readonly SqlConnection _con;

        public MapApiController(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            var connStr = configuration.GetConnectionString("FaultDb")
                ?? throw new InvalidOperationException("Connection string 'FaultDb' is not configured.");
            _con = new SqlConnection(connStr);
        }

        public void Dispose()
        {
            _con?.Dispose();
        }

        // GET: /api/map/faults?from=2026-01-01&to=2026-03-01&carrier=Dialog&search=D12345&top=2000
        [HttpGet("faults")]
        public async Task<IActionResult> GetFaults(
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    [FromQuery] string? carrier,
    [FromQuery] string? search,
    [FromQuery] string? region,
    [FromQuery] string? province,
    [FromQuery] bool customerFault = false,
    [FromQuery] int top = 2000)
        {
            var list = new List<FaultMapPointDto>();

            if (top < 1) top = 100;
            if (top > 10000) top = 10000;

            var customerFaultMode = customerFault
                || IsCustomerFaultSelector(Request.Query["source"].ToString())
                || IsCustomerFaultSelector(Request.Query["mode"].ToString());

            var fromInput = ResolveDateQueryValue(from, "fromDate");
            var toInput = ResolveDateQueryValue(to, "toDate");

            var fromDate = (fromInput ?? (customerFaultMode
                ? DateTime.UtcNow.AddDays(-DefaultCustomerFaultLookbackDays)
                : DefaultNetworkFaultFromDate)).Date;
            var toDate = toInput.HasValue ? toInput.Value.Date.AddDays(1) : DateTime.UtcNow.Date.AddDays(1);

            if (fromDate >= toDate)
                return BadRequest(new { message = "From date must be on or before To date." });

            string carrierLike = "";
            if (!string.IsNullOrWhiteSpace(carrier))
            {
                carrierLike = carrier.Trim().ToLower() switch
                {
                    "dialog" => "%dialog%",
                    
                    "mobitel" => "%mobitel%",
                    "hutch" => "%hutch%",
                    "etisalat" => "%etisalat%",
                    "airtel" => "%airtel%",
                    _ => "%" + carrier.Trim().ToLower() + "%"
                };
            }

            // When searching, we only want exact CCT matches, so don't pass search to the stored procedure
            var searchLike = ""; // Always empty to let client-side handle exact CCT filtering
            var regionValue = string.IsNullOrWhiteSpace(region) ? "" : region.Trim().ToLower();
            var provinceValue = string.IsNullOrWhiteSpace(province) ? "" : province.Trim().ToLower();

            // Extract the original search term (without wildcards) for exact matching
            var originalSearchTerm = string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLower();

            if (customerFaultMode)
            {
                return await GetCustomerFaults(fromDate, toDate, carrierLike, "", regionValue, provinceValue, top, originalSearchTerm);
            }

            try
            {
                await _con.OpenAsync();
                await using var coordinateCon = new SqlConnection(_con.ConnectionString);
                await coordinateCon.OpenAsync();
                var coordinateColumns = await GetCoordinateColumnsAsync(coordinateCon);
                var coordinateCache = new Dictionary<string, CoordinateLookupDto?>(StringComparer.OrdinalIgnoreCase);

                using var cmd = new SqlCommand("dbo.usp_Map_GetFaultsNetwork", _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Top", top);
                cmd.Parameters.AddWithValue("@From", fromDate);
                cmd.Parameters.AddWithValue("@To", toDate);
                cmd.Parameters.AddWithValue("@CarrierLike", carrierLike);
                cmd.Parameters.AddWithValue("@SearchLike", searchLike);
                cmd.Parameters.AddWithValue("@RegionValue", regionValue);
                cmd.Parameters.AddWithValue("@ProvinceValue", provinceValue);

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var lat = GetNullableDouble(r, "Lat");
                    var lng = GetNullableDouble(r, "Lng");
                    if (!lat.HasValue || !lng.HasValue)
                    {
                        // Ignore rows without valid coordinates so one bad record does not fail the entire response.
                        continue;
                    }

                    var cct = r["CCT"]?.ToString();
                    var faultKey = r["FaultKey"]?.ToString();
                    var faultText = r["FAULT"]?.ToString() ?? string.Empty;
                    
                    // When searching for CCT, only include exact matches (case-insensitive)
                    if (!string.IsNullOrWhiteSpace(originalSearchTerm))
                    {
                        var isCctMatch = !string.IsNullOrWhiteSpace(cct) && 
                                         cct.Equals(originalSearchTerm, StringComparison.OrdinalIgnoreCase);
                        
                        // If search term looks like a CCT (not empty) and current row's CCT doesn't match exactly, skip it
                        if (!isCctMatch)
                        {
                            continue;
                        }
                    }

                    var coordinateDetails =
                        await GetCoordinateDetailsByCctAsync(coordinateCon, coordinateColumns, coordinateCache, cct)
                        ?? await GetCoordinateDetailsAsync(coordinateCon, coordinateColumns, coordinateCache, lat.Value, lng.Value);
                    var account = GetNullableString(r, "ACCOUNT")
                        ?? GetNullableString(r, "ACCOUNT_NO")
                        ?? GetNullableString(r, "ACCOUNT_ID")
                        ?? GetNullableString(r, "ACCOUNTNUMBER")
                        ?? GetNullableString(r, "AC_NO")
                        ?? GetNullableString(r, "ACCT_NO")
                        ?? GetNullableString(r, "CUST_ACCOUNT")
                        ?? GetNullableString(r, "CUSTOMER_ACCOUNT");

                    var circuitStatus = GetNullableString(r, "CIRCUIT_STATUS")
                        ?? GetNullableString(r, "CIRCUIT STATUS")
                        ?? GetNullableString(r, "SERVICE_STATUS")
                        ?? GetNullableString(r, "STATUS")
                        ?? GetNullableString(r, "CCT_STATUS")
                        ?? GetNullableString(r, "CIRT_STATUS");
                    if (string.IsNullOrWhiteSpace(circuitStatus))
                    {
                        if (faultText.Contains("down", StringComparison.OrdinalIgnoreCase))
                            circuitStatus = "Down";
                        else if (faultText.Contains("up", StringComparison.OrdinalIgnoreCase))
                            circuitStatus = "Up";
                        else if (faultText.Contains("loss", StringComparison.OrdinalIgnoreCase) || faultText.Contains("error", StringComparison.OrdinalIgnoreCase))
                            circuitStatus = "Alarm";
                        else
                            circuitStatus = "Open";
                    }

                    var customerName = GetNullableString(r, "CUSTOMER_NAME")
                        ?? GetNullableString(r, "CUSTOMER")
                        ?? GetNullableString(r, "CARRIER")
                        ?? GetNullableString(r, "PROVIDER")
                        ?? GetNullableString(r, "OPERATOR")
                        ?? GetNullableString(r, "NodeName");

                    var regionText = GetNullableString(r, "REGION");
                    var provinceText = GetNullableString(r, "PROVINCE");
                    var customerAddress = GetNullableString(r, "CUSTOMER_ADDRESS")
                        ?? GetNullableString(r, "ADDRESS")
                        ?? GetNullableString(r, "CUST_ADDRESS")
                        ?? GetNullableString(r, "SERVICE_ADDRESS")
                        ?? GetNullableString(r, "LOCATION_ADDRESS");
                    if (string.IsNullOrWhiteSpace(customerAddress))
                    {
                        customerAddress = !string.IsNullOrWhiteSpace(provinceText) && !string.IsNullOrWhiteSpace(regionText)
                            ? $"{provinceText}, {regionText}"
                            : provinceText ?? regionText;
                    }

                    var rGm = GetNullableString(r, "R_GM")
                        ?? GetNullableString(r, "RGM")
                        ?? regionText;
                    var pDgm = GetNullableString(r, "P_DGM")
                        ?? GetNullableString(r, "PDGM")
                        ?? provinceText;
                    var ipAddress = GetNullableString(r, "IP_ADDRESS")
                        ?? GetNullableString(r, "IPADDRESS")
                        ?? GetNullableString(r, "IP");

                    list.Add(new FaultMapPointDto
                    {
                        ID = Convert.ToInt32(r["ID"]),
                        NodeName = r["NodeName"]?.ToString(),
                        FAULT = faultText,
                        FAULT_TIME = r["FAULT_TIME"] as DateTime?,
                        FaultKey = faultKey,
                        FaultType = r["FaultType"]?.ToString(),

                        CCT = cct,
                        EQ_LOCATION_NODE = r["EQ_LOCATION_NODE"]?.ToString(),
                        R_GM = coordinateDetails?.R_GM ?? rGm,
                        P_DGM = coordinateDetails?.P_DGM ?? pDgm,
                        REGION = regionText,
                        PROVINCE = provinceText,

                        CR = coordinateDetails?.CR ?? GetNullableString(r, "CR") ?? GetNullableString(r, "CX_CR"),
                        ACCOUNT = coordinateDetails?.ACCOUNT ?? account,
                        CIRCUIT_NAME = coordinateDetails?.CIRCUIT_NAME ?? GetNullableString(r, "CIRCUIT_NAME") ?? GetNullableString(r, "CX_CIRCUIT_NAME") ?? GetNullableString(r, "CIRCUIT_ID") ?? GetNullableString(r, "SERVICE") ?? GetNullableString(r, "SERVICE_NAME") ?? GetNullableString(r, "SERVICE_TYPE") ?? GetNullableString(r, "ALIAS_NAME"),
                        CIRCUIT_STATUS = coordinateDetails?.CIRCUIT_STATUS ?? circuitStatus,
                        CUSTOMER_NAME = coordinateDetails?.CUSTOMER_NAME ?? customerName,
                        CUSTOMER_ADDRESS = coordinateDetails?.CUSTOMER_ADDRESS ?? customerAddress,
                        IP_ADDRESS = coordinateDetails?.IP_ADDRESS ?? ipAddress,

                        Lat = lat.Value,
                        Lng = lng.Value,

                        CeaLat = GetNullableDouble(r, "CeaLat"),
                        CeaLng = GetNullableDouble(r, "CeaLng"),

                        Carrier = r["Carrier"]?.ToString(),
                        IsUrgent = r["IsUrgent"] != DBNull.Value && Convert.ToBoolean(r["IsUrgent"]),
                        IsPriority = r["IsPriority"] != DBNull.Value && Convert.ToBoolean(r["IsPriority"])
                    });
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error reading faults", error = ex.Message });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }



        // GET: /api/map/faults?from=2026-01-01&to=2026-03-01&carrier=Dialog&search=D12345&top=2000
        [HttpGet("faults_all")]
        public async Task<IActionResult> GetFaultsAll(
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    [FromQuery] string? carrier,
    [FromQuery] string? search,
    [FromQuery] string? region,
    [FromQuery] string? province,
    [FromQuery] bool customerFault = false,
    [FromQuery] int top = 2000)
        {
            var list = new List<FaultMapPointDto>();

            if (top < 1) top = 100;
            if (top > 10000) top = 10000;

            var customerFaultMode = customerFault
                || IsCustomerFaultSelector(Request.Query["source"].ToString())
                || IsCustomerFaultSelector(Request.Query["mode"].ToString());

            var fromInput = ResolveDateQueryValue(from, "fromDate");
            var toInput = ResolveDateQueryValue(to, "toDate");

            var fromDate = (fromInput ?? (customerFaultMode
                ? DateTime.UtcNow.AddDays(-DefaultCustomerFaultLookbackDays)
                : DefaultNetworkFaultFromDate)).Date;
            var toDate = toInput.HasValue ? toInput.Value.Date.AddDays(1) : DateTime.UtcNow.Date.AddDays(1);

            if (fromDate >= toDate)
                return BadRequest(new { message = "From date must be on or before To date." });

            string carrierLike = "";
            if (!string.IsNullOrWhiteSpace(carrier))
            {
                carrierLike = carrier.Trim().ToLower() switch
                {
                    "dialog" => "%dialog%",

                    "mobitel" => "%mobitel%",
                    "hutch" => "%hutch%",
                    "etisalat" => "%etisalat%",
                    "airtel" => "%airtel%",
                    _ => "%" + carrier.Trim().ToLower() + "%"
                };
            }

            // When searching, we only want exact CCT matches, so don't pass search to the stored procedure
            var searchLike = ""; // Always empty to let client-side handle exact CCT filtering
            var regionValue = string.IsNullOrWhiteSpace(region) ? "" : region.Trim().ToLower();
            var provinceValue = string.IsNullOrWhiteSpace(province) ? "" : province.Trim().ToLower();

            // Extract the original search term (without wildcards) for exact matching
            var originalSearchTerm = string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLower();

            if (customerFaultMode)
            {
                return await GetCustomerFaults(fromDate, toDate, carrierLike, "", regionValue, provinceValue, top, originalSearchTerm);
            }

            try
            {
                await _con.OpenAsync();
                await using var coordinateCon = new SqlConnection(_con.ConnectionString);
                await coordinateCon.OpenAsync();
                var coordinateColumns = await GetCoordinateColumnsAsync(coordinateCon);
                var coordinateCache = new Dictionary<string, CoordinateLookupDto?>(StringComparer.OrdinalIgnoreCase);

                using var cmd = new SqlCommand("dbo.usp_Map_GetFaultsNetwork_all", _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Top", top);
                cmd.Parameters.AddWithValue("@From", fromDate);
                cmd.Parameters.AddWithValue("@To", toDate);
                cmd.Parameters.AddWithValue("@CarrierLike", carrierLike);
                cmd.Parameters.AddWithValue("@SearchLike", searchLike);
                cmd.Parameters.AddWithValue("@RegionValue", regionValue);
                cmd.Parameters.AddWithValue("@ProvinceValue", provinceValue);

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var lat = GetNullableDouble(r, "Lat");
                    var lng = GetNullableDouble(r, "Lng");


                    var cct = r["CCT"]?.ToString();
                    var faultKey = r["FaultKey"]?.ToString();
                    var faultText = r["FAULT"]?.ToString() ?? string.Empty;

                    // When searching for CCT, only include exact matches (case-insensitive)
                    if (!string.IsNullOrWhiteSpace(originalSearchTerm))
                    {
                        var isCctMatch = !string.IsNullOrWhiteSpace(cct) &&
                                         cct.Equals(originalSearchTerm, StringComparison.OrdinalIgnoreCase);

                        // If search term looks like a CCT (not empty) and current row's CCT doesn't match exactly, skip it
                        if (!isCctMatch)
                        {
                            continue;
                        }
                    }

                    var coordinateDetails =
                        await GetCoordinateDetailsByCctAsync(coordinateCon, coordinateColumns, coordinateCache, cct)
                        ?? (lat.HasValue && lng.HasValue ? await GetCoordinateDetailsAsync(coordinateCon, coordinateColumns, coordinateCache, lat.Value, lng.Value) : null);
                    var account = GetNullableString(r, "ACCOUNT")
                        ?? GetNullableString(r, "ACCOUNT_NO")
                        ?? GetNullableString(r, "ACCOUNT_ID")
                        ?? GetNullableString(r, "ACCOUNTNUMBER")
                        ?? GetNullableString(r, "AC_NO")
                        ?? GetNullableString(r, "ACCT_NO")
                        ?? GetNullableString(r, "CUST_ACCOUNT")
                        ?? GetNullableString(r, "CUSTOMER_ACCOUNT");

                    var circuitStatus = GetNullableString(r, "CIRCUIT_STATUS")
                        ?? GetNullableString(r, "CIRCUIT STATUS")
                        ?? GetNullableString(r, "SERVICE_STATUS")
                        ?? GetNullableString(r, "STATUS")
                        ?? GetNullableString(r, "CCT_STATUS")
                        ?? GetNullableString(r, "CIRT_STATUS");
                    if (string.IsNullOrWhiteSpace(circuitStatus))
                    {
                        if (faultText.Contains("down", StringComparison.OrdinalIgnoreCase))
                            circuitStatus = "Down";
                        else if (faultText.Contains("up", StringComparison.OrdinalIgnoreCase))
                            circuitStatus = "Up";
                        else if (faultText.Contains("loss", StringComparison.OrdinalIgnoreCase) || faultText.Contains("error", StringComparison.OrdinalIgnoreCase))
                            circuitStatus = "Alarm";
                        else
                            circuitStatus = "Open";
                    }

                    var customerName = GetNullableString(r, "CUSTOMER_NAME")
                        ?? GetNullableString(r, "CUSTOMER")
                        ?? GetNullableString(r, "CARRIER")
                        ?? GetNullableString(r, "PROVIDER")
                        ?? GetNullableString(r, "OPERATOR")
                        ?? GetNullableString(r, "NodeName");

                    var regionText = GetNullableString(r, "REGION");
                    var provinceText = GetNullableString(r, "PROVINCE");
                    var customerAddress = GetNullableString(r, "CUSTOMER_ADDRESS")
                        ?? GetNullableString(r, "ADDRESS")
                        ?? GetNullableString(r, "CUST_ADDRESS")
                        ?? GetNullableString(r, "SERVICE_ADDRESS")
                        ?? GetNullableString(r, "LOCATION_ADDRESS");
                    if (string.IsNullOrWhiteSpace(customerAddress))
                    {
                        customerAddress = !string.IsNullOrWhiteSpace(provinceText) && !string.IsNullOrWhiteSpace(regionText)
                            ? $"{provinceText}, {regionText}"
                            : provinceText ?? regionText;
                    }

                    var rGm = GetNullableString(r, "R_GM")
                        ?? GetNullableString(r, "RGM")
                        ?? regionText;
                    var pDgm = GetNullableString(r, "P_DGM")
                        ?? GetNullableString(r, "PDGM")
                        ?? provinceText;
                    var ipAddress = GetNullableString(r, "IP_ADDRESS")
                        ?? GetNullableString(r, "IPADDRESS")
                        ?? GetNullableString(r, "IP");

                    list.Add(new FaultMapPointDto
                    {
                        ID = Convert.ToInt32(r["ID"]),
                        NodeName = r["NodeName"]?.ToString(),
                        FAULT = faultText,
                        FAULT_TIME = r["FAULT_TIME"] as DateTime?,
                        FaultKey = faultKey,
                        FaultType = r["FaultType"]?.ToString(),

                        CCT = cct,
                        EQ_LOCATION_NODE = r["EQ_LOCATION_NODE"]?.ToString(),
                        R_GM = coordinateDetails?.R_GM ?? rGm,
                        P_DGM = coordinateDetails?.P_DGM ?? pDgm,
                        REGION = regionText,
                        PROVINCE = provinceText,

                        CR = coordinateDetails?.CR ?? GetNullableString(r, "CR") ?? GetNullableString(r, "CX_CR"),
                        ACCOUNT = coordinateDetails?.ACCOUNT ?? account,
                        CIRCUIT_NAME = coordinateDetails?.CIRCUIT_NAME ?? GetNullableString(r, "CIRCUIT_NAME") ?? GetNullableString(r, "CX_CIRCUIT_NAME") ?? GetNullableString(r, "CIRCUIT_ID") ?? GetNullableString(r, "SERVICE") ?? GetNullableString(r, "SERVICE_NAME") ?? GetNullableString(r, "SERVICE_TYPE") ?? GetNullableString(r, "ALIAS_NAME"),
                        CIRCUIT_STATUS = coordinateDetails?.CIRCUIT_STATUS ?? circuitStatus,
                        CUSTOMER_NAME = coordinateDetails?.CUSTOMER_NAME ?? customerName,
                        CUSTOMER_ADDRESS = coordinateDetails?.CUSTOMER_ADDRESS ?? customerAddress,
                        IP_ADDRESS = coordinateDetails?.IP_ADDRESS ?? ipAddress,

                        Lat = lat.GetValueOrDefault(),
                        Lng = lng.GetValueOrDefault(),

                        CeaLat = GetNullableDouble(r, "CeaLat"),
                        CeaLng = GetNullableDouble(r, "CeaLng"),

                        Carrier = r["Carrier"]?.ToString(),
                        IsUrgent = r["IsUrgent"] != DBNull.Value && Convert.ToBoolean(r["IsUrgent"]),
                        IsPriority = r["IsPriority"] != DBNull.Value && Convert.ToBoolean(r["IsPriority"])
                    });
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error reading faults", error = ex.Message });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }


        // GET: /api/map/carrier-counts?from=2026-01-01&to=2026-03-01&carrier=Dialog&search=D12345
        [HttpGet("carrier-counts")]
        public async Task<IActionResult> GetCarrierCounts(
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    [FromQuery] string? carrier,
    [FromQuery] string? search,
    [FromQuery] string? region,
    [FromQuery] string? province,
    [FromQuery] bool customerFault = false)
        {
            var customerFaultMode = customerFault
                || IsCustomerFaultSelector(Request.Query["source"].ToString())
                || IsCustomerFaultSelector(Request.Query["mode"].ToString());

            var fromInput = ResolveDateQueryValue(from, "fromDate");
            var toInput = ResolveDateQueryValue(to, "toDate");

            var fromDate = (fromInput ?? (customerFaultMode
                ? DateTime.UtcNow.AddDays(-DefaultCustomerFaultLookbackDays)
                : DefaultNetworkFaultFromDate)).Date;
            var toDate = toInput.HasValue ? toInput.Value.Date.AddDays(1) : DateTime.UtcNow.Date.AddDays(1);

            if (fromDate >= toDate)
                return BadRequest(new { message = "From date must be on or before To date." });

            string carrierLike = "";
            if (!string.IsNullOrWhiteSpace(carrier))
            {
                carrierLike = carrier.Trim().ToLower() switch
                {
                    "dialog" => "%dialog%",
                    "slt" => "%slt%",
                    "mobitel" => "%slt%",
                    "hutch" => "%hutch%",
                    "etisalat" => "%etisalat%",
                    "airtel" => "%airtel%",
                    _ => "%" + carrier.Trim().ToLower() + "%"
                };
            }

            var searchLike = string.IsNullOrWhiteSpace(search) ? "" : "%" + search.Trim().ToLower() + "%";
            var regionValue = string.IsNullOrWhiteSpace(region) ? "" : region.Trim().ToLower();
            var provinceValue = string.IsNullOrWhiteSpace(province) ? "" : province.Trim().ToLower();

            if (customerFaultMode)
            {
                return await GetCustomerFaultCarrierCounts(fromDate, toDate, carrierLike, searchLike, regionValue, provinceValue);
            }

            try
            {
                await _con.OpenAsync();

                using var cmd = new SqlCommand("dbo.usp_Map_GetCarrierCountsNetwork", _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@From", fromDate);
                cmd.Parameters.AddWithValue("@To", toDate);
                cmd.Parameters.AddWithValue("@CarrierLike", carrierLike);
                cmd.Parameters.AddWithValue("@SearchLike", searchLike);
                cmd.Parameters.AddWithValue("@RegionValue", regionValue);
                cmd.Parameters.AddWithValue("@ProvinceValue", provinceValue);

                var counts = CreateEmptyCarrierCounts();

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var key = r["Carrier"]?.ToString();
                    var alarmCount = r["AlarmCount"] == DBNull.Value ? 0 : Convert.ToInt32(r["AlarmCount"]);
                    var customerFaultCount = r["CustomerFaultCount"] == DBNull.Value ? 0 : Convert.ToInt32(r["CustomerFaultCount"]);

                    if (!string.IsNullOrWhiteSpace(key) && counts.TryGetValue(key, out var item))
                    {
                        item.AlarmCount = alarmCount;
                        item.CustomerFaultCount = customerFaultCount;
                    }
                }

                return Ok(counts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error reading carrier counts", error = ex.Message });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        // GET: /api/map/region-province-options
        [HttpGet("region-province-options")]
        public async Task<IActionResult> GetRegionProvinceOptions()
        {
            try
            {
                await _con.OpenAsync();

                using var cmd = new SqlCommand("dbo.usp_Map_GetRegionProvinceOptions", _con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var rows = new List<object>();
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    rows.Add(new
                    {
                        Region = r["REGION"] == DBNull.Value ? null : r["REGION"]?.ToString(),
                        Province = r["PROVINCE"] == DBNull.Value ? null : r["PROVINCE"]?.ToString()
                    });
                }

                return Ok(rows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error reading region/province options", error = ex.Message });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        private async Task<IActionResult> GetCustomerFaults(
    DateTime fromDate,
    DateTime toDate,
    string carrierLike,
    string searchLike,
    string regionValue,
    string provinceValue,
    int top,
    string originalSearchTerm = "")
        {
            var list = new List<FaultMapPointDto>();
            
            // If originalSearchTerm not provided, extract from searchLike
            if (string.IsNullOrWhiteSpace(originalSearchTerm))
            {
                originalSearchTerm = searchLike.StartsWith("%") && searchLike.EndsWith("%")
                    ? searchLike.Substring(1, searchLike.Length - 2)
                    : searchLike;
                originalSearchTerm = string.IsNullOrWhiteSpace(originalSearchTerm) ? "" : originalSearchTerm;
            }

            try
            {
                await _con.OpenAsync();
                await using var coordinateCon = new SqlConnection(_con.ConnectionString);
                await coordinateCon.OpenAsync();
                var coordinateColumns = await GetCoordinateColumnsAsync(coordinateCon);
                var coordinateCache = new Dictionary<string, CoordinateLookupDto?>(StringComparer.OrdinalIgnoreCase);

                using var cmd = new SqlCommand("dbo.usp_Map_GetCustomerFaults", _con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Top", top);
                cmd.Parameters.AddWithValue("@From", fromDate);
                cmd.Parameters.AddWithValue("@To", toDate);
                cmd.Parameters.AddWithValue("@CarrierLike", carrierLike);
                cmd.Parameters.AddWithValue("@SearchLike", searchLike);
                cmd.Parameters.AddWithValue("@RegionValue", regionValue);
                cmd.Parameters.AddWithValue("@ProvinceValue", provinceValue);

                using var r = await cmd.ExecuteReaderAsync();
                var seenCcts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                
                while (await r.ReadAsync())
                {
                    var lat = GetNullableDouble(r, "Lat");
                    var lng = GetNullableDouble(r, "Lng");
                    if (!lat.HasValue || !lng.HasValue)
                    {
                        // Ignore rows without valid coordinates so one bad record does not fail the entire response.
                        continue;
                    }

                    var cct = r["CCT"]?.ToString();
                    var faultKey = r["FaultKey"]?.ToString();
                    var faultText = r["FAULT"]?.ToString() ?? string.Empty;
                    
                    // When searching for CCT, only include exact matches (case-insensitive) and skip duplicates
                    if (!string.IsNullOrWhiteSpace(originalSearchTerm))
                    {
                        var isCctMatch = !string.IsNullOrWhiteSpace(cct) && 
                                         cct.Equals(originalSearchTerm, StringComparison.OrdinalIgnoreCase);
                        
                        // If search term is provided and current row's CCT doesn't match exactly, skip it
                        if (!isCctMatch)
                        {
                            continue;
                        }
                        
                        // Skip duplicate CCTs when searching - only show first occurrence
                        if (!string.IsNullOrWhiteSpace(cct) && !seenCcts.Add(cct))
                        {
                            continue;
                        }
                    }

                    var coordinateDetails =
                        await GetCoordinateDetailsByCctAsync(coordinateCon, coordinateColumns, coordinateCache, cct)
                        ?? await GetCoordinateDetailsAsync(coordinateCon, coordinateColumns, coordinateCache, lat.Value, lng.Value);

                    var account = GetNullableString(r, "ACCOUNT")
                        ?? GetNullableString(r, "ACCOUNT_NO")
                        ?? GetNullableString(r, "ACCOUNT_ID")
                        ?? GetNullableString(r, "ACCOUNTNUMBER")
                        ?? GetNullableString(r, "AC_NO")
                        ?? GetNullableString(r, "ACCT_NO")
                        ?? GetNullableString(r, "CUST_ACCOUNT")
                        ?? GetNullableString(r, "CUSTOMER_ACCOUNT");

                    var circuitStatus = GetNullableString(r, "CIRCUIT_STATUS")
                        ?? GetNullableString(r, "CIRCUIT STATUS")
                        ?? GetNullableString(r, "SERVICE_STATUS")
                        ?? GetNullableString(r, "STATUS")
                        ?? GetNullableString(r, "CCT_STATUS")
                        ?? GetNullableString(r, "CIRT_STATUS");
                    if (string.IsNullOrWhiteSpace(circuitStatus))
                    {
                        if (faultText.Contains("down", StringComparison.OrdinalIgnoreCase))
                            circuitStatus = "Down";
                        else if (faultText.Contains("up", StringComparison.OrdinalIgnoreCase))
                            circuitStatus = "Up";
                        else if (faultText.Contains("loss", StringComparison.OrdinalIgnoreCase) || faultText.Contains("error", StringComparison.OrdinalIgnoreCase))
                            circuitStatus = "Alarm";
                        else
                            circuitStatus = "Open";
                    }

                    var customerName = GetNullableString(r, "CUSTOMER_NAME")
                        ?? GetNullableString(r, "CUSTOMER")
                        ?? GetNullableString(r, "CARRIER")
                        ?? GetNullableString(r, "PROVIDER")
                        ?? GetNullableString(r, "OPERATOR")
                        ?? GetNullableString(r, "NodeName");

                    var regionText = GetNullableString(r, "REGION");
                    var provinceText = GetNullableString(r, "PROVINCE");
                    var customerAddress = GetNullableString(r, "CUSTOMER_ADDRESS")
                        ?? GetNullableString(r, "ADDRESS")
                        ?? GetNullableString(r, "CUST_ADDRESS")
                        ?? GetNullableString(r, "SERVICE_ADDRESS")
                        ?? GetNullableString(r, "LOCATION_ADDRESS");
                    if (string.IsNullOrWhiteSpace(customerAddress))
                    {
                        customerAddress = !string.IsNullOrWhiteSpace(provinceText) && !string.IsNullOrWhiteSpace(regionText)
                            ? $"{provinceText}, {regionText}"
                            : provinceText ?? regionText;
                    }

                    var rGm = GetNullableString(r, "R_GM")
                        ?? GetNullableString(r, "RGM")
                        ?? regionText;
                    var pDgm = GetNullableString(r, "P_DGM")
                        ?? GetNullableString(r, "PDGM")
                        ?? provinceText;
                    var ipAddress = GetNullableString(r, "IP_ADDRESS")
                        ?? GetNullableString(r, "IPADDRESS")
                        ?? GetNullableString(r, "IP");

                    list.Add(new FaultMapPointDto
                    {
                        ID = r["ID"] == DBNull.Value ? 0 : Convert.ToInt32(r["ID"]),
                        NodeName = r["NodeName"]?.ToString(),
                        FAULT = faultText,
                        FAULT_TIME = r["FAULT_TIME"] as DateTime?,
                        FaultKey = faultKey,
                        FaultType = r["FaultType"]?.ToString(),

                        CCT = cct,
                        EQ_LOCATION_NODE = r["EQ_LOCATION_NODE"]?.ToString(),
                        R_GM = coordinateDetails?.R_GM ?? rGm,
                        P_DGM = coordinateDetails?.P_DGM ?? pDgm,
                        REGION = regionText,
                        PROVINCE = provinceText,

                        CR = coordinateDetails?.CR ?? GetNullableString(r, "CR") ?? GetNullableString(r, "CX_CR"),
                        ACCOUNT = coordinateDetails?.ACCOUNT ?? account,
                        CIRCUIT_NAME = coordinateDetails?.CIRCUIT_NAME ?? GetNullableString(r, "CIRCUIT_NAME") ?? GetNullableString(r, "CX_CIRCUIT_NAME") ?? GetNullableString(r, "CIRCUIT_ID") ?? GetNullableString(r, "SERVICE") ?? GetNullableString(r, "SERVICE_NAME") ?? GetNullableString(r, "SERVICE_TYPE") ?? GetNullableString(r, "ALIAS_NAME"),
                        CIRCUIT_STATUS = coordinateDetails?.CIRCUIT_STATUS ?? circuitStatus,
                        CUSTOMER_NAME = coordinateDetails?.CUSTOMER_NAME ?? customerName,
                        CUSTOMER_ADDRESS = coordinateDetails?.CUSTOMER_ADDRESS ?? customerAddress,
                        IP_ADDRESS = coordinateDetails?.IP_ADDRESS ?? ipAddress,

                        Lat = lat.Value,
                        Lng = lng.Value,

                        CeaLat = GetNullableDouble(r, "CeaLat"),
                        CeaLng = GetNullableDouble(r, "CeaLng"),

                        Carrier = r["Carrier"]?.ToString(),
                        IsUrgent = r["IsUrgent"] != DBNull.Value && Convert.ToBoolean(r["IsUrgent"]),
                        IsPriority = r["IsPriority"] != DBNull.Value && Convert.ToBoolean(r["IsPriority"])
                    });
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error reading customer faults", error = ex.Message });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        private async Task<IActionResult> GetCustomerFaultCarrierCounts(
    DateTime fromDate,
    DateTime toDate,
    string carrierLike,
    string searchLike,
    string regionValue,
    string provinceValue)
        {
            try
            {
                await _con.OpenAsync();

                using var cmd = new SqlCommand("dbo.usp_Map_GetCustomerFaultCarrierCounts", _con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@From", fromDate);
                cmd.Parameters.AddWithValue("@To", toDate);
                cmd.Parameters.AddWithValue("@CarrierLike", carrierLike);
                cmd.Parameters.AddWithValue("@SearchLike", searchLike);
                cmd.Parameters.AddWithValue("@RegionValue", regionValue);
                cmd.Parameters.AddWithValue("@ProvinceValue", provinceValue);

                var counts = CreateEmptyCarrierCounts();

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var key = r["Carrier"]?.ToString();
                    // When querying customer faults, AlarmCount should always be 0
                    // Only CustomerFaultCount should be populated
                    var customerFaultCount = r["CustomerFaultCount"] == DBNull.Value ? 0 : Convert.ToInt32(r["CustomerFaultCount"]);

                    if (!string.IsNullOrWhiteSpace(key) && counts.TryGetValue(key, out var item))
                    {
                        item.AlarmCount = 0; // Always 0 for customer fault queries
                        item.CustomerFaultCount = customerFaultCount;
                    }
                }

                return Ok(counts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error reading customer carrier counts", error = ex.Message });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        private async Task<IActionResult> GetCustomerFaultRegionProvinceOptions(DateTime fromDate, DateTime toDate)
        {
            try
            {
                await _con.OpenAsync();

                using var cmd = new SqlCommand("dbo.usp_Map_GetCustomerRegionProvinceOptions", _con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@From", fromDate);
                cmd.Parameters.AddWithValue("@To", toDate);

                var rows = new List<object>();
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    rows.Add(new
                    {
                        Region = r["REGION"] == DBNull.Value ? null : r["REGION"]?.ToString(),
                        Province = r["PROVINCE"] == DBNull.Value ? null : r["PROVINCE"]?.ToString()
                    });
                }

                return Ok(rows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error reading customer region/province options", error = ex.Message });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
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
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var name = r["COLUMN_NAME"]?.ToString();
                if (!string.IsNullOrWhiteSpace(name) && !map.ContainsKey(name))
                    map[name] = name;
            }

            return map;
        }

        private static string BuildCoordinateSelectList(Dictionary<string, string> columns)
        {
            var parts = new List<string>
            {
                BuildCoordinateColumn(columns, new[] { "CR" }, "CR"),
                BuildCoordinateColumn(columns, new[] { "ACCOUNT", "ACCOUNT_NO", "ACCOUNT NUMBER", "ACCOUNT_ID", "AC_NO", "AC NUMBER" }, "ACCOUNT"),
                BuildCoordinateColumn(columns, new[] { "CCT", "CIRCUIT_ID", "SERVICE_ID" }, "CCT"),
                BuildCoordinateColumn(columns, new[] { "CIRCUIT_NAME", "CIRCUIT NAME", "SERVICE", "SERVICE_NAME", "CIRCUIT", "SERVICE TYPE" }, "CIRCUIT_NAME"),
                BuildCoordinateColumn(columns, new[] { "CIRCUIT_STATUS", "CIRCUIT STATUS", "SERVICE_STATUS", "STATUS", "CCT_STATUS", "CIRT_STATUS" }, "CIRCUIT_STATUS"),
                BuildCoordinateColumn(columns, new[] { "CUSR_NAME", "CUSTOMER_NAME", "CUSTOMER NAME", "CUST_NAME", "CUSTOMER", "OPERATOR", "CARRIER", "PROVIDER" }, "CUSTOMER_NAME"),
                BuildCoordinateColumn(columns, new[] { "CUSR_ADDRESS", "CUSTOMER_ADDRESS", "CUSTOMER ADDRESS", "ADDRESS", "CUST_ADDRESS", "SERVICE_ADDRESS", "LOCATION_ADDRESS", "BENDADDRESS" }, "CUSTOMER_ADDRESS"),
                BuildCoordinateColumn(columns, new[] { "EQ_LOCATION_NODE", "EQ LOCATION NODE", "EQ_NODE", "EQ NODE", "NODE" }, "EQ_LOCATION_NODE"),
                BuildCoordinateColumn(columns, new[] { "R-GM", "R_GM", "R GM" }, "R_GM"),
                BuildCoordinateColumn(columns, new[] { "P-DGM", "P_DGM", "P DGM" }, "P_DGM"),
                BuildCoordinateColumn(columns, new[] { "IP_ADDRESS", "IP ADDRESS", "IP", "IPADDRESS", "IP-ADDRESS" }, "IP_ADDRESS")
            };

            var sb = new StringBuilder();
            for (var i = 0; i < parts.Count; i++)
            {
                if (i > 0) sb.AppendLine(",");
                sb.Append("    ").Append(parts[i]);
            }

            return sb.ToString();
        }

        private static string BuildCoordinateColumn(
            Dictionary<string, string> columns,
            IEnumerable<string> candidates,
            string alias)
        {
            foreach (var candidate in candidates)
            {
                if (columns.TryGetValue(candidate, out var actualColumn))
                {
                    return $"TRY_CONVERT(nvarchar(4000), s.{BracketIdentifier(actualColumn)}) AS {BracketIdentifier(alias)}";
                }

                var normalizedCandidate = NormalizeIdentifier(candidate);
                foreach (var kv in columns)
                {
                    if (NormalizeIdentifier(kv.Key) != normalizedCandidate) continue;
                    return $"TRY_CONVERT(nvarchar(4000), s.{BracketIdentifier(kv.Value)}) AS {BracketIdentifier(alias)}";
                }
            }

            return $"CAST(NULL AS nvarchar(4000)) AS {BracketIdentifier(alias)}";
        }

        private static string BuildCoordinateValueExpression(
            Dictionary<string, string> columns,
            IEnumerable<string> candidates,
            string tableAlias,
            string fallbackExpression)
        {
            foreach (var candidate in candidates)
            {
                if (columns.TryGetValue(candidate, out var actualColumn))
                {
                    return $"TRY_CONVERT(nvarchar(4000), {tableAlias}.{BracketIdentifier(actualColumn)})";
                }

                var normalizedCandidate = NormalizeIdentifier(candidate);
                foreach (var kv in columns)
                {
                    if (NormalizeIdentifier(kv.Key) != normalizedCandidate) continue;
                    return $"TRY_CONVERT(nvarchar(4000), {tableAlias}.{BracketIdentifier(kv.Value)})";
                }
            }

            return fallbackExpression;
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

        private DateTime? ResolveDateQueryValue(DateTime? directValue, string alternateQueryKey)
        {
            if (directValue.HasValue) return directValue.Value;
            if (!Request.Query.TryGetValue(alternateQueryKey, out var raw)) return null;

            var text = raw.ToString();
            if (string.IsNullOrWhiteSpace(text)) return null;

            return DateTime.TryParse(text, out var parsed) ? parsed : null;
        }

        private static bool IsCustomerFaultSelector(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            var normalized = new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
            return normalized is "customerfault" or "customerfaults";
        }

        private static string BracketIdentifier(string value) => $"[{value.Replace("]", "]]", StringComparison.Ordinal)}]";

        private static string NormalizeIdentifier(string value)
        {
            var chars = value.Where(char.IsLetterOrDigit).ToArray();
            return new string(chars).ToUpperInvariant();
        }

        private static string? GetNullableString(IDataRecord record, string columnName)
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

        private static async Task<CoordinateLookupDto?> GetCoordinateDetailsByCctAsync(
            SqlConnection coordinateCon,
            Dictionary<string, string> coordinateColumns,
            Dictionary<string, CoordinateLookupDto?> cache,
            string? cct)
        {
            if (string.IsNullOrWhiteSpace(cct))
                return null;

            var cctNormalized = cct.Trim().ToUpperInvariant();
            if (cache.TryGetValue(cctNormalized, out var cached))
                return cached;

            var cctColumn = ResolveCoordinateColumn(coordinateColumns, new[] { "CCT", "CIRCUIT_ID", "SERVICE_ID" });
            if (string.IsNullOrWhiteSpace(cctColumn))
            {
                cache[cctNormalized] = null;
                return null;
            }

            var sql = $@"
SELECT TOP (1)
{BuildCoordinateSelectList(coordinateColumns)}
FROM dbo.coordinates s
WHERE UPPER(LTRIM(RTRIM(s.{BracketIdentifier(cctColumn)}))) = @CCT;";

            using var cmd = new SqlCommand(sql, coordinateCon);
            cmd.Parameters.Add("@CCT", SqlDbType.NVarChar).Value = cctNormalized;

            using var reader = await cmd.ExecuteReaderAsync();
            CoordinateLookupDto? coordinate = null;

            if (await reader.ReadAsync())
            {
                coordinate = new CoordinateLookupDto
                {
                    CR = GetNullableString(reader, "CR"),
                    ACCOUNT = GetNullableString(reader, "ACCOUNT"),
                    CCT = GetNullableString(reader, "CCT"),
                    CIRCUIT_NAME = GetNullableString(reader, "CIRCUIT_NAME"),
                    CIRCUIT_STATUS = GetNullableString(reader, "CIRCUIT_STATUS"),
                    CUSTOMER_NAME = GetNullableString(reader, "CUSTOMER_NAME"),
                    CUSTOMER_ADDRESS = GetNullableString(reader, "CUSTOMER_ADDRESS"),
                    EQ_LOCATION_NODE = GetNullableString(reader, "EQ_LOCATION_NODE"),
                    R_GM = GetNullableString(reader, "R_GM"),
                    P_DGM = GetNullableString(reader, "P_DGM"),
                    IP_ADDRESS = GetNullableString(reader, "IP_ADDRESS")
                };
            }

            cache[cctNormalized] = coordinate;
            return coordinate;
        }

        private static async Task<CoordinateLookupDto?> GetCoordinateDetailsAsync(
            SqlConnection coordinateCon,
            Dictionary<string, string> coordinateColumns,
            Dictionary<string, CoordinateLookupDto?> cache,
            double latitude,
            double longitude)
        {
            var cacheKey = string.Create(CultureInfo.InvariantCulture, $"{latitude:G17}|{longitude:G17}") ;
            if (cache.TryGetValue(cacheKey, out var cached))
                return cached;

            var latitudeColumn = ResolveCoordinateColumn(coordinateColumns, new[] { "latitude", "lat", "Latitude", "Lat" });
            var longitudeColumn = ResolveCoordinateColumn(coordinateColumns, new[] { "longitude", "lng", "Longitude", "Lng" });

            if (string.IsNullOrWhiteSpace(latitudeColumn) || string.IsNullOrWhiteSpace(longitudeColumn))
            {
                cache[cacheKey] = null;
                return null;
            }

            var sql = $@"
SELECT TOP (1)
{BuildCoordinateSelectList(coordinateColumns)}
FROM dbo.coordinates s
WHERE ABS(COALESCE(TRY_CONVERT(float, s.{BracketIdentifier(latitudeColumn)}), 0) - @Lat) < 0.0005
  AND ABS(COALESCE(TRY_CONVERT(float, s.{BracketIdentifier(longitudeColumn)}), 0) - @Lng) < 0.0005;";

            using var cmd = new SqlCommand(sql, coordinateCon);
            cmd.Parameters.Add("@Lat", SqlDbType.Float).Value = latitude;
            cmd.Parameters.Add("@Lng", SqlDbType.Float).Value = longitude;

            using var reader = await cmd.ExecuteReaderAsync();
            CoordinateLookupDto? coordinate = null;

            if (await reader.ReadAsync())
            {
                coordinate = new CoordinateLookupDto
                {
                    CR = GetNullableString(reader, "CR"),
                    ACCOUNT = GetNullableString(reader, "ACCOUNT"),
                    CCT = GetNullableString(reader, "CCT"),
                    CIRCUIT_NAME = GetNullableString(reader, "CIRCUIT_NAME"),
                    CIRCUIT_STATUS = GetNullableString(reader, "CIRCUIT_STATUS"),
                    CUSTOMER_NAME = GetNullableString(reader, "CUSTOMER_NAME"),
                    CUSTOMER_ADDRESS = GetNullableString(reader, "CUSTOMER_ADDRESS"),
                    EQ_LOCATION_NODE = GetNullableString(reader, "EQ_LOCATION_NODE"),
                    R_GM = GetNullableString(reader, "R_GM"),
                    P_DGM = GetNullableString(reader, "P_DGM"),
                    IP_ADDRESS = GetNullableString(reader, "IP_ADDRESS")
                };
            }

            cache[cacheKey] = coordinate;
            return coordinate;
        }

        private static double? GetNullableDouble(IDataRecord record, string columnName)
        {
            for (var i = 0; i < record.FieldCount; i++)
            {
                if (!string.Equals(record.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (record.IsDBNull(i)) return null;

                try
                {
                    return Convert.ToDouble(record.GetValue(i));
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private static Dictionary<string, CarrierCountsDto> CreateEmptyCarrierCounts()
        {
            return new Dictionary<string, CarrierCountsDto>(StringComparer.OrdinalIgnoreCase)
            {
                ["Dialog"] = new CarrierCountsDto(),
                ["Mobitel"] = new CarrierCountsDto(),
                ["Hutch"] = new CarrierCountsDto(),
                ["Etisalat"] = new CarrierCountsDto(),
                ["Airtel"] = new CarrierCountsDto()
            };
        }

        private sealed class CarrierCountsDto
        {
            public int AlarmCount { get; set; }
            public int CustomerFaultCount { get; set; }
        }

        private sealed class CoordinateLookupDto
        {
            public string? CR { get; set; }
            public string? ACCOUNT { get; set; }
            public string? CCT { get; set; }
            public string? CIRCUIT_NAME { get; set; }
            public string? CIRCUIT_STATUS { get; set; }
            public string? CUSTOMER_NAME { get; set; }
            public string? CUSTOMER_ADDRESS { get; set; }
            public string? EQ_LOCATION_NODE { get; set; }
            public string? R_GM { get; set; }
            public string? P_DGM { get; set; }
            public string? IP_ADDRESS { get; set; }
        }
    }
}
