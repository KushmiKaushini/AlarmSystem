using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class PowerAlarmService
    {
        private readonly IConfiguration _configuration;

        public PowerAlarmService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<PowerAlarmModel> GetPowerAlarms()
        {
            var list = new List<PowerAlarmModel>();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("TMS"));

            con.Open();

            string sql = @"
SELECT
ID,
NODE,
FAULT,
FAULT_TIME,
ALARM_TYPE
FROM FAULTS
WHERE PLATFORM='RPB'
ORDER BY FAULT_TIME DESC";

            SqlCommand cmd = new(sql, con);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new PowerAlarmModel
                {
                    Id = dr["ID"].ToString(),
                    Node = dr["NODE"].ToString(),
                    Alarm = dr["FAULT"].ToString(),
                    FaultTime = Convert.ToDateTime(dr["FAULT_TIME"]),
                    AlarmType = dr["ALARM_TYPE"].ToString(),
                    Ack = "NO"
                });
            }

            return list;
        }
    }
}