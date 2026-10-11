using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class ReportCeaPortDownService : IReportCeaPortDownService
    {
        private readonly IConfiguration _configuration;

        public ReportCeaPortDownService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection Connection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("TMS"));
        }

        public async Task<List<string>> GetCustomersAsync()
        {
            return await Task.FromResult(new List<string>
            {
                "~ALL~",
                "Dialog",
                "LANKA_BELL",
                "MOBITEL",
                "HUTCH",
                "ETISALAT"
            });
        }

        public async Task<List<ReportCeaPortDown>> GetReportAsync(
            DateTime? from,
            DateTime? to,
            string customer)
        {
            List<ReportCeaPortDown> list = new();

            using SqlConnection con = Connection();

            await con.OpenAsync();

            SqlCommand cmd = new SqlCommand(
                "REPORT_New_NW_MAPPING_REGION_CEA_PORT_OPEN",
                con);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@from",
                from ?? DateTime.Now.Date);

            cmd.Parameters.AddWithValue(
                "@to",
                to ?? DateTime.Now);

            cmd.Parameters.AddWithValue(
                "@customer",
                string.IsNullOrEmpty(customer)
                    ? "~ALL~"
                    : customer);

            SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new ReportCeaPortDown
                {
                    ID = Convert.ToInt32(dr["ID"]),
                    NODE = dr["NODE"]?.ToString() ?? "",
                    FAULT = dr["FAULT"]?.ToString() ?? "",
                    FAULT_TIME = dr["FAULT_TIME"] as DateTime?,
                    PLATFORM = dr["PLATFORM"]?.ToString() ?? "",
                    FAULT_STATUS = dr["FAULT_STATUS"]?.ToString() ?? "",
                    REGION = dr["REGION"]?.ToString() ?? "",
                    PROVINCE = dr["PROVINCE"]?.ToString() ?? "",
                    NW_ENG = dr["NW_ENG"]?.ToString() ?? "",
                    ALARM_TYPE = dr["ALARM_TYPE"]?.ToString() ?? "",
                    SOURCEIP = dr["sourceip"]?.ToString() ?? ""
                });
            }

            return list;
        }
    }
}