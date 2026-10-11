using AlarmSystem.Models.Customer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Controllers
{
    [ApiController]
    [Route("api/customerfault")]
    public class CustomerFaultApiController : ControllerBase
    {
        private readonly SqlConnection _con;

        public CustomerFaultApiController(SqlConnection con)
        {
            _con = con;
        }

        // ✅ GET: /api/customerfault/summary?from=2026-01-01&region={region}&province={province}&top=8
        [HttpGet("summary")]
        public async Task<IActionResult> Summary(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? region,
            [FromQuery] string? province,
            [FromQuery] int top = 8)
        {
            // limits
            if (top < 1) top = 8;
            if (top > 50) top = 50;

            if (!TryResolveDateRange(from, to, out var fromDate, out var toDate))
                return BadRequest(new { message = "From date must be on or before To date." });

            var regionValue = string.IsNullOrWhiteSpace(region) ? "" : region.Trim().ToLower();
            var provinceValue = string.IsNullOrWhiteSpace(province) ? "" : province.Trim().ToLower();

            var result = new CustomerFaultSummaryDto();

            try
            {
                await _con.OpenAsync();

                using (var cmd = new SqlCommand("dbo.usp_CustomerFault_Summary", _con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@From", SqlDbType.DateTime).Value = fromDate;
                    cmd.Parameters.Add("@To", SqlDbType.DateTime).Value = toDate;
                    cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
                    cmd.Parameters.Add("@RegionValue", SqlDbType.NVarChar, 200).Value = regionValue;
                    cmd.Parameters.Add("@ProvinceValue", SqlDbType.NVarChar, 200).Value = provinceValue;

                    using var r = await cmd.ExecuteReaderAsync();
                    if (await r.ReadAsync())
                    {
                        result.Total = Convert.ToInt32(r["Total"]);
                        result.Today = Convert.ToInt32(r["Today"]);
                        result.Last7Days = Convert.ToInt32(r["Last7Days"]);
                    }

                    if (await r.NextResultAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            var timeStr = r["reportedtime"]?.ToString();
                            DateTime? faultTime = null;
                            if (!string.IsNullOrWhiteSpace(timeStr) && DateTime.TryParse(timeStr, out var dt))
                                faultTime = dt;

                            result.Recent.Add(new CustomerFaultItemDto
                            {
                                Id = r["problem_no"] == DBNull.Value ? 0 : Convert.ToInt32(r["problem_no"]),
                                Fault = r["description"]?.ToString(),
                                FaultTime = faultTime,
                                Source = r["circuit_name"]?.ToString()
                            });
                        }
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CustomerFaultApi] Summary Error: {ex}");
                return StatusCode(500, new { message = "Error reading customer faults", error = ex.Message, innerError = ex.InnerException?.Message });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }


        [HttpGet("mapdata-all")]
        public async Task<IActionResult> MapDataAll(
            DateTime? from,
            DateTime? to,
            string? search,
            string? region,
            string? province,
            int top = 5000)
        {
            //var fromDate = (from ?? DateTime.UtcNow.AddDays(-7)).Date;
            var fromDate = from ?? new DateTime(1900, 1, 1);
            var toDate = to.HasValue ? to.Value.Date.AddDays(1) : DateTime.UtcNow.Date.AddDays(1);

            var list = new List<dynamic>();

            await _con.OpenAsync();

            using var cmd = new SqlCommand("dbo.usp_CustomerFault_MapData_All", _con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Top", top);
            cmd.Parameters.AddWithValue("@From", fromDate);
            cmd.Parameters.AddWithValue("@To", toDate);
            cmd.Parameters.AddWithValue("@SearchLike", string.IsNullOrEmpty(search) ? "" : "%" + search.ToLower() + "%");
            cmd.Parameters.AddWithValue("@RegionValue", region ?? "");
            cmd.Parameters.AddWithValue("@ProvinceValue", province ?? "");

            using var r = await cmd.ExecuteReaderAsync();

            while (await r.ReadAsync())
            {
                list.Add(new
                {
                    cr = r["cr"],
                    fault = r["description"],
                    circuit_name = r["circuit_name"],
                    circuitType = r["circuit_type"],
                    latitude = r["latitude"],
                    longitude = r["longitude"],
                    customer_name = r["Customer_Name"],
                    BENDADDRESS = r["BENDADDRESS"],
                    Account_Number = r["Account_Number"],
                    reportedtime = r["reportedtime"],
                    ceaLat = r["cealat"],
                    ceaLng = r["cealong"],
                    NW_NODE = r["NW_NODE"],
                    Region = r["Region"],
                    Province = r["Province"]

                });
            }

            return Ok(list);
        }


        // ✅ GET: /api/customerfault/list?from=2026-01-01&search=los&region={region}&province={province}&top=200
        // For the CustomerFaults page table
        [HttpGet("list")]
        public async Task<IActionResult> List(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? search,
            [FromQuery] string? region,
            [FromQuery] string? province,
            [FromQuery] int top = 200)
        {
            // limits
            if (top < 1) top = 200;
            if (top > 5000) top = 5000;

            if (!TryResolveDateRange(from, to, out var fromDate, out var toDate))
                return BadRequest(new { message = "From date must be on or before To date." });

            var searchLike = string.IsNullOrWhiteSpace(search) ? "" : $"%{search.Trim().ToLower()}%";
            var regionValue = string.IsNullOrWhiteSpace(region) ? "" : region.Trim().ToLower();
            var provinceValue = string.IsNullOrWhiteSpace(province) ? "" : province.Trim().ToLower();

            var list = new List<dynamic>();

            try
            {
                await _con.OpenAsync();

                using var cmd = new SqlCommand("dbo.usp_CustomerFault_List", _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
                cmd.Parameters.Add("@From", SqlDbType.DateTime).Value = fromDate;
                cmd.Parameters.Add("@To", SqlDbType.DateTime).Value = toDate;
                cmd.Parameters.Add("@RegionValue", SqlDbType.NVarChar, 200).Value = regionValue;
                cmd.Parameters.Add("@ProvinceValue", SqlDbType.NVarChar, 200).Value = provinceValue;

                cmd.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 2000).Value = searchLike;

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var timeStr = r["reportedtime"]?.ToString();
                    DateTime? faultTime = null;
                    if (!string.IsNullOrWhiteSpace(timeStr) && DateTime.TryParse(timeStr, out var dt))
                        faultTime = dt;

                    list.Add(new
                    {
                        id = r["problem_no"] == DBNull.Value ? 0 : Convert.ToInt32(r["problem_no"]),
                        fault = r["description"]?.ToString(),
                        faultTime,
                        cr = r["cr"]?.ToString(),
                        source = r["circuit_name"]?.ToString()
                    });
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CustomerFaultApi] List Error: {ex}");
                return StatusCode(500, new { message = "Error reading customer faults list", error = ex.Message, innerError = ex.InnerException?.Message });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        // ✅ GET: /api/customerfault/mapdata?from=2026-01-01&to=2026-03-01&search={text}&region={region}&province={province}&top=5000
        // For displaying customer faults on map by matching cx_fault.circuit_name with NW_fault.DeviceCode
        [HttpGet("mapdata")]
        public async Task<IActionResult> MapData(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? search,
            [FromQuery] string? carrier,
            [FromQuery] string? region,
            [FromQuery] string? province,
            [FromQuery] int top = 5000)
        {
            if (top < 1) top = 500;
            if (top > 10000) top = 10000;
            var fromDate = from ?? new DateTime(1900, 1, 1);
            //var fromDate = (from ?? DateTime.UtcNow.AddDays(-7)).Date;
            var toDate = to.HasValue ? to.Value.Date.AddDays(1) : DateTime.UtcNow.Date.AddDays(1);
            var searchText = string.IsNullOrWhiteSpace(search) ? "" : search.Trim();
            var regionValue = string.IsNullOrWhiteSpace(region) ? "" : region.Trim().ToLower();
            var provinceValue = string.IsNullOrWhiteSpace(province) ? "" : province.Trim().ToLower();
            var carrierLike = string.IsNullOrWhiteSpace(carrier) ? "" : carrier.Trim().ToLower() switch
            {
                "dialog" => "%dialog%",
                "mobitel" => "%mobitel%",
                "hutch" => "%hutch%",
                "etisalat" => "%etisalat%",
                "airtel" => "%airtel%",
                _ => "%" + carrier.Trim().ToLower() + "%"
            };

            var mapPoints = new List<dynamic>();

            try
            {
                await _con.OpenAsync();

                using var cmd = new SqlCommand("dbo.usp_CustomerFault_MapData", _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
                cmd.Parameters.Add("@From", SqlDbType.DateTime).Value = fromDate;
                cmd.Parameters.Add("@To", SqlDbType.DateTime).Value = toDate;
                cmd.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 2000).Value = string.IsNullOrWhiteSpace(searchText) ? "" : $"%{searchText.ToLower()}%";
                cmd.Parameters.Add("@RegionValue", SqlDbType.NVarChar, 200).Value = regionValue;
                cmd.Parameters.Add("@ProvinceValue", SqlDbType.NVarChar, 200).Value = provinceValue;

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var faultTime = r["reportedtime"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(r["reportedtime"]);

                    var latitude = GetNullableDouble(r, "latitude", "lat", "Latitude", "Lat");
                    var longitude = GetNullableDouble(r, "longitude", "lng", "Longitude", "Lng");
                    if (!latitude.HasValue || !longitude.HasValue)
                        continue;

                    var regionText = r["REGION"] == DBNull.Value ? null : r["REGION"]?.ToString();
                    var provinceText = r["PROVINCE"] == DBNull.Value ? null : r["PROVINCE"]?.ToString();

                    var eqLocationNode = GetNullableString(r, "EQ_LOCATION_NODE", "EQ LOCATION NODE", "EQ_NODE", "EQ NODE", "NODE")
                        ?? GetNullableString(r, "NW_NODE");
                    var ceaLat = r["ceaLat"]?.ToString();
                    var ceaLng = r["ceaLng"]?.ToString();
                    
                    var account = r["Account_Number"] == DBNull.Value ? null : r["Account_Number"]?.ToString();
                    var customerName = r["Customer_Name"]?.ToString();
                    var descriptionText = r["description"]?.ToString() ?? string.Empty;
                    var circuitStatus = GetNullableString(r, "CIRCUIT_STATUS", "CIRCUIT STATUS", "SERVICE_STATUS", "STATUS", "CCT_STATUS", "CIRT_STATUS", "VW_CIRCUIT_TYPE");

                    if (string.IsNullOrWhiteSpace(circuitStatus))
                    {
                        if (descriptionText.Contains("down", StringComparison.OrdinalIgnoreCase))
                            circuitStatus = "Down";
                        else if (descriptionText.Contains("up", StringComparison.OrdinalIgnoreCase))
                            circuitStatus = "Up";
                        else if (descriptionText.Contains("loss", StringComparison.OrdinalIgnoreCase) || descriptionText.Contains("error", StringComparison.OrdinalIgnoreCase))
                            circuitStatus = "Alarm";
                        else
                            circuitStatus = "Open";
                    }

               

                    var customerAddress = GetNullableString(r, "CUSTOMER_ADDRESS", "CUSTOMER ADDRESS", "CUSR_ADDRESS", "ADDRESS", "CUST_ADDRESS", "SERVICE_ADDRESS", "LOCATION_ADDRESS", "BENDADDRESS");

                    var circuitName = GetNullableString(r, "CIRCUIT_NAME", "CIRCUIT NAME", "CIRCUIT_ID", "SERVICE", "SERVICE_NAME", "SERVICE_TYPE", "CIRCUIT", "CX_CIRCUIT_NAME", "ALIAS_NAME")
                        ?? r["circuit_name"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(searchText) &&
                        (string.IsNullOrWhiteSpace(circuitName) || circuitName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(customerAddress))
                    {
                        customerAddress = !string.IsNullOrWhiteSpace(provinceText) && !string.IsNullOrWhiteSpace(regionText)
                            ? $"{provinceText}, {regionText}"
                            : provinceText ?? regionText;
                    }

                    var rGm = GetNullableString(r, "R_GM", "RGM", "RGMGM")
                        ?? regionText;

                    var pDgm = GetNullableString(r, "P_DGM", "PDGM", "P_DGM_NAME")
                        ?? provinceText;

                    var ipAddress = GetNullableString(r, "IP_ADDRESS", "IP-ADDRESS", "IPADDRESS", "IP", "VW_IP");

                    var alarmTime = GetNullableString(r, "Alarm_time");
                    var alarmText = GetNullableString(r, "Alarm");

                    mapPoints.Add(new
                    {
                        id = r["problem_no"] == DBNull.Value ? 0 : Convert.ToInt32(r["problem_no"]),
                        fault = r["description"]?.ToString(),
                        faultTime,
                        cr = GetNullableString(r, "CR") ?? r["cr"]?.ToString(),
                       // CR = GetNullableString(r, "CR") ?? r["cr"]?.ToString(),
                        CCT = GetNullableString(r, "CCT") ?? r["circuit_name"]?.ToString(),
                       // cr = r["cr"]?.ToString(),
                        //circuitName = circuitName,
                        faultType = "Customer",
                        latitude = latitude.Value,
                        longitude = longitude.Value,
                        EQ_LOCATION_NODE = eqLocationNode,
                        ACCOUNT = account,
                        CIRCUIT_NAME = circuitName,
                        CIRCUIT_STATUS = circuitStatus,
                        CUSTOMER_NAME = customerName,
                        CUSTOMER_ADDRESS = customerAddress,
                        R_GM = rGm,
                        P_DGM = pDgm,
                        IP_ADDRESS = ipAddress,
                        nodeName = GetNullableString(r, "NW_NODE"),
                        region = regionText,
                        province = provinceText,
                        cealat = ceaLat,
                        cealng = ceaLng,
                        Alarm_Time=alarmTime,
                        AlarmDes=alarmText

                });
                }

                return Ok(mapPoints);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine($"[CustomerFaultApi] MapData Error: {ex}");
                return StatusCode(500, new { message = "Error reading map data", error = ex.Message, innerError = ex.InnerException?.Message });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        private static string NormalizeColumnKey(string value)
        {
            var chars = value.Where(char.IsLetterOrDigit).ToArray();
            return new string(chars).ToUpperInvariant();
        }

        private static bool TryResolveDateRange(DateTime? from, DateTime? to, out DateTime fromDate, out DateTime toDate)
        {
            var effectiveTo = (to ?? DateTime.UtcNow.Date).Date;
            fromDate = (from ?? effectiveTo.AddDays(-30)).Date;

            if (fromDate > effectiveTo)
            {
                toDate = default;
                return false;
            }

            toDate = effectiveTo.AddDays(1);
            return true;
        }

        private static string? GetNullableString(IDataRecord record, params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                var normalizedTarget = NormalizeColumnKey(columnName);
                for (var i = 0; i < record.FieldCount; i++)
                {
                    var normalizedField = NormalizeColumnKey(record.GetName(i));
                    if (!string.Equals(normalizedField, normalizedTarget, StringComparison.Ordinal))
                        continue;

                    if (record.IsDBNull(i)) return null;
                    return record.GetValue(i)?.ToString();
                }
            }

            return null;
        }

        private static double? GetNullableDouble(IDataRecord record, params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                var normalizedTarget = NormalizeColumnKey(columnName);
                for (var i = 0; i < record.FieldCount; i++)
                {
                    var normalizedField = NormalizeColumnKey(record.GetName(i));
                    if (!string.Equals(normalizedField, normalizedTarget, StringComparison.Ordinal))
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
            }

            return null;
        }
    }
}