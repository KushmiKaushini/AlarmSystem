using AlarmSystem.Models;
using Microsoft.Data.SqlClient;

namespace AlarmSystem.Services
{
    public class SummaryService : ISummaryService
    {
        private readonly IConfiguration _configuration;

        public SummaryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection Connection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("TMS"));
        }

        public async Task<List<Summary>> GetPendingSummaryAsync()
        {
            var list = new List<Summary>();

            using var con = Connection();
            await con.OpenAsync();

            string sql = @"
SELECT PLATFORM,
COUNT(CASE WHEN DATEDIFF(MINUTE,FAULT_TIME,GETDATE())<=240 THEN 1 END) AS LessThan4Hours,
COUNT(CASE WHEN DATEDIFF(MINUTE,FAULT_TIME,GETDATE())>240
AND DATEDIFF(MINUTE,FAULT_TIME,GETDATE())<=480 THEN 1 END) AS LessThan8Hours,
COUNT(CASE WHEN DATEDIFF(MINUTE,FAULT_TIME,GETDATE())>480 THEN 1 END) AS MoreThan8Hours,
COUNT(PROVINCE) AS Total
FROM FAULTS
WHERE ALARM_TYPE='NODE DOWN'
AND FAULT_TIME>=CONVERT(DATETIME,DATEDIFF(DAY,0,GETDATE()))
AND PROVINCE<>'DEFAULT'
AND SOURCEIP NOT LIKE '58.%'
AND PLATFORM IN ('MSAN','LTE','CEA')
AND CLOSE_TIME IS NULL
GROUP BY PLATFORM";

            using SqlCommand cmd = new SqlCommand(sql, con);

            using SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new Summary
                {
                    Platform = dr["PLATFORM"].ToString(),
                    LessThan4Hours = Convert.ToInt32(dr["LessThan4Hours"]),
                    LessThan8Hours = Convert.ToInt32(dr["LessThan8Hours"]),
                    MoreThan8Hours = Convert.ToInt32(dr["MoreThan8Hours"]),
                    Total = Convert.ToInt32(dr["Total"])
                });
            }

            return list;
        }

        public async Task<List<PendingAlarm>> GetPendingAlarmsAsync()
        {
            var list = new List<PendingAlarm>();

            using var con = Connection();
            await con.OpenAsync();

            string sql = @"
SELECT NODE,
FAULT_TIME,
PLATFORM,
PROVINCE
FROM FAULTS
WHERE ALARM_TYPE='NODE DOWN'
AND SOURCEIP NOT LIKE '58.%'
AND PROVINCE<>'DEFAULT'
AND PLATFORM IN ('CEA','MSAN','LTE','SLBN')
AND FAULT_TIME>=CONVERT(DATETIME,DATEDIFF(DAY,0,GETDATE()))
ORDER BY FAULT_TIME DESC";

            using SqlCommand cmd = new SqlCommand(sql, con);

            using SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new PendingAlarm
                {
                    Node = dr["NODE"].ToString(),
                    FaultTime = Convert.ToDateTime(dr["FAULT_TIME"]),
                    Platform = dr["PLATFORM"].ToString(),
                    Province = dr["PROVINCE"].ToString()
                });
            }

            return list;
        }

        public async Task<List<Summary>> GetHistorySummaryAsync()
        {
            return new List<Summary>();
        }

        public async Task<List<HistoryAlarm>> GetHistoryAlarmsAsync()
        {
            return new List<HistoryAlarm>();
        }
    }
}