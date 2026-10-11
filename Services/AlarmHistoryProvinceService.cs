using AlarmSystem.Models;
using Microsoft.Data.SqlClient;

namespace AlarmSystem.Services
{
    public class AlarmHistoryProvinceService : IAlarmHistoryProvinceService
    {
        private readonly IConfiguration _configuration;

        public AlarmHistoryProvinceService(IConfiguration configuration)
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

            SqlCommand cmd = new SqlCommand(@"
                SELECT PLATFORM
                FROM
                (
                    SELECT '~ALL~' AS PLATFORM
                    UNION
                    SELECT DISTINCT PLATFORM
                    FROM HISTORY_FAULTS
                ) A
                ORDER BY PLATFORM", con);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(dr["PLATFORM"].ToString()!);
            }

            return list;
        }

        public async Task<List<string>> GetProvincesAsync()
        {
            var list = new List<string>();

            using var con = Connection();

            await con.OpenAsync();

            SqlCommand cmd = new SqlCommand(@"
                SELECT PROVINCE
                FROM
                (
                    SELECT '~ALL~' AS PROVINCE
                    UNION
                    SELECT DISTINCT PROVINCE
                    FROM HISTORY_FAULTS
                    WHERE PROVINCE IS NOT NULL
                      AND PROVINCE <> ''
                ) A
                ORDER BY PROVINCE", con);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(dr["PROVINCE"].ToString()!);
            }

            return list;
        }

        public async Task<List<AlarmHistoryProvince>> GetReportAsync(
            DateTime? from,
            DateTime? to,
            string platform,
            string province)
        {
            var list = new List<AlarmHistoryProvince>();

            // REPORT_PROVINCEWISE Stored Procedure
            // SQL logic will be added later.

            return list;
        }
    }
}