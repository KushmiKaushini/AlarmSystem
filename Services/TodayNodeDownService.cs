using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class TodayNodeDownService
    {
        private readonly IConfiguration _configuration;

        public TodayNodeDownService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<TodayNodeDownModel> GetTodayNodeDown()
        {
            List<TodayNodeDownModel> list = new();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("TMS"));

            string sql = @"
SELECT
    MAX(msanolt.site_name) AS Site_Name,
    faults.node,

    MAX(NW_ENG) AS Network_Eng,
    MAX(Region) AS Region,
    MAX(Province) AS Province,

    MAX(MSANOLT.PLATFORM) AS Vendor,
    MAX(MSANOLT) AS MSANOLT,

    MAX(CASE WHEN alarm_type='AC Mains Failure'
        THEN FAULT_TIME END) AS AC_Mains_Failure_time,

    MAX(CASE WHEN alarm_type='AC Mains Failure'
        THEN alarm_type END) AS AC_Mains_Failure,

    MAX(CASE WHEN alarm_type='Battery Low'
        THEN FAULT_TIME END) AS Battery_Low_time,

    MAX(CASE WHEN alarm_type='Battery Low'
        THEN alarm_type END) AS Battery_Low,

    MAX(CASE WHEN alarm_type='Node Down'
        THEN FAULT_TIME END) AS Node_Down_time,

    MAX(CASE WHEN alarm_type='Node Down'
        THEN alarm_type END) AS Node_Down

FROM FAULTS

LEFT JOIN MSANOLT
ON FAULTS.NODE = MSANOLT.element_code

WHERE
FAULTS.PLATFORM='MSAN'
AND NODE!='WLW-HPG_ZXDSL9806H_02'

GROUP BY faults.node

ORDER BY faults.node";

            using SqlCommand cmd = new SqlCommand(sql, con);

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new TodayNodeDownModel
                {
                    Site_Name = dr["Site_Name"]?.ToString(),
                    Node = dr["node"]?.ToString(),
                    Network_Eng = dr["Network_Eng"]?.ToString(),
                    Region = dr["Region"]?.ToString(),
                    Province = dr["Province"]?.ToString(),
                    Vendor = dr["Vendor"]?.ToString(),
                    MSANOLT = dr["MSANOLT"]?.ToString(),

                    AC_Mains_Failure_time =
                        dr["AC_Mains_Failure_time"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(dr["AC_Mains_Failure_time"]),

                    AC_Mains_Failure =
                        dr["AC_Mains_Failure"]?.ToString(),

                    Battery_Low_time =
                        dr["Battery_Low_time"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(dr["Battery_Low_time"]),

                    Battery_Low =
                        dr["Battery_Low"]?.ToString(),

                    Node_Down_time =
                        dr["Node_Down_time"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(dr["Node_Down_time"]),

                    Node_Down =
                        dr["Node_Down"]?.ToString()
                });
            }

            return list;
        }
    }
}