using AlarmSystem.Models.Reports;
using AlarmSystem.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly SqlConnection _connection;

        public ReportService(SqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<NodeDownReportModel>> GetNodeDownReportAsync(
            DateTime from,
            DateTime to,
            string customer)
        {
            var reports = new List<NodeDownReportModel>();

            using (SqlCommand cmd = new SqlCommand("REPORT_New_NW_MAPPING_REGION_CEA_PORT_OPEN", _connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@from", from);
                cmd.Parameters.AddWithValue("@to", to);
                cmd.Parameters.AddWithValue("@customer", customer);

                if (_connection.State != ConnectionState.Open)
                    await _connection.OpenAsync();

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        reports.Add(new NodeDownReportModel
                        {
                            ID = reader["ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ID"]),
                            NODE = reader["NODE"]?.ToString(),
                            FAULT = reader["FAULT"]?.ToString(),
                            FAULT_TIME = reader["FAULT_TIME"] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(reader["FAULT_TIME"]),
                            PLATFORM = reader["PLATFORM"]?.ToString(),
                            FAULT_STATUS = reader["FAULT_STATUS"]?.ToString(),
                            REGION = reader["REGION"]?.ToString(),
                            PROVINCE = reader["PROVINCE"]?.ToString(),
                            NW_ENG = reader["NW_ENG"]?.ToString(),
                            ALARM_TYPE = reader["ALARM_TYPE"]?.ToString(),
                            SOURCEIP = reader["sourceip"]?.ToString()
                        });
                    }
                }

                await _connection.CloseAsync();
            }

            return reports.Where(r =>
                !string.Equals(r.REGION, "HQ", StringComparison.OrdinalIgnoreCase) &&
                !(r.REGION ?? "").Contains("HQ", StringComparison.OrdinalIgnoreCase) &&
                !(r.PROVINCE ?? "").Contains("HQ", StringComparison.OrdinalIgnoreCase) &&
                !(r.NW_ENG ?? "").Contains("HQ", StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
    }
}