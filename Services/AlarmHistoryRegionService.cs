using AlarmSystem.Models;
using Microsoft.Data.SqlClient;

namespace AlarmSystem.Services
{
    public class AlarmHistoryRegionService
        : IAlarmHistoryRegionService
    {
        private readonly IConfiguration _configuration;

        public AlarmHistoryRegionService(IConfiguration configuration)
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

        public async Task<List<string>> GetRegionsAsync()
        {
            var list = new List<string>();

            using var con = Connection();

            await con.OpenAsync();

            SqlCommand cmd = new SqlCommand(@"
                SELECT REGION
                FROM
                (
                    SELECT '~ALL~' AS REGION
                    UNION
                    SELECT DISTINCT REGION
                    FROM HISTORY_FAULTS
                    WHERE REGION IS NOT NULL
                    AND REGION <> ''
                ) A
                ORDER BY REGION", con);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(dr["REGION"].ToString()!);
            }

            return list;
        }

        public async Task<List<AlarmHistoryRegion>> GetReportAsync(
            DateTime? from,
            DateTime? to,
            string platform,
            string region)
        {
            var list = new List<AlarmHistoryRegion>();

            // REPORT_REGIONWISE Stored Procedure
            // SQL will be added later.

            return list;
        }
    }
}