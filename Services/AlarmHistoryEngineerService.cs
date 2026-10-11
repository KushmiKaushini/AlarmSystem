using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class AlarmHistoryEngineerService
        : IAlarmHistoryEngineerService
    {
        private readonly IConfiguration _configuration;

        public AlarmHistoryEngineerService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection Connection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("TMS"));
        }

        public async Task<List<string>> GetPlatformsAsync()
        {
            var list = new List<string>();

            using var con = Connection();

            await con.OpenAsync();

            SqlCommand cmd = new SqlCommand(
                @"SELECT DISTINCT PLATFORM
                  FROM HISTORY_FAULTS
                  ORDER BY PLATFORM", con);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(dr["PLATFORM"].ToString());
            }

            return list;
        }

        public async Task<List<string>> GetEngineersAsync()
        {
            var list = new List<string>();

            using var con = Connection();

            await con.OpenAsync();

            SqlCommand cmd = new SqlCommand(
                @"SELECT DISTINCT NW_ENG
                  FROM HISTORY_FAULTS
                  ORDER BY NW_ENG", con);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(dr["NW_ENG"].ToString());
            }

            return list;
        }

        public async Task<List<AlarmHistoryEngineer>> GetAllAsync(
            DateTime? from,
            DateTime? to,
            string platform,
            string engineer)
        {
            var list = new List<AlarmHistoryEngineer>();

            // REPORT_1 Stored Procedure
            // Add SQL logic here after you provide it.

            return list;
        }

        public async Task<List<AlarmHistoryEngineer>> GetMsanAsync(
            DateTime? from,
            DateTime? to,
            string engineer)
        {
            var list = new List<AlarmHistoryEngineer>();

            // REPORT_New_NW_MAPPING_REGION
            // Add SQL logic here after you provide it.

            return list;
        }
    }
}