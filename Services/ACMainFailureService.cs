using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class ACMainFailureService
    {
        private readonly IConfiguration _configuration;

        public ACMainFailureService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //========================================
        // PLATFORM
        //========================================

        public List<PlatformModel> GetPlatforms()
        {
            List<PlatformModel> list = new();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("TMS"));

            string sql = @"

SELECT '~ALL~' AS PLATFORM

UNION

SELECT DISTINCT PLATFORM
FROM HISTORY_FAULTS

ORDER BY PLATFORM

";

            SqlCommand cmd = new SqlCommand(sql, con);

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new PlatformModel
                {
                    PLATFORM = dr["PLATFORM"].ToString() ?? ""
                });
            }

            return list;
        }

        //========================================
        // NW ENGINEERS
        //========================================

        public List<NWEngineerModel> GetEngineers()
        {
            List<NWEngineerModel> list = new();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("TMS"));

            string sql = @"

SELECT '~ALL~' AS NW_ENG

UNION

SELECT DISTINCT NW_ENG
FROM HISTORY_FAULTS
WHERE NW_ENG IS NOT NULL

ORDER BY NW_ENG

";

            SqlCommand cmd = new SqlCommand(sql, con);

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new NWEngineerModel
                {
                    NW_ENG = dr["NW_ENG"].ToString() ?? ""
                });
            }

            return list;
        }

        

        //========================================
        // REPORT
        //========================================

        public List<ACMainFailureModel> GetReport(
            DateTime from,
            DateTime to,
            string platform,
            string nwEng)
        {
            List<ACMainFailureModel> list = new();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("TMS"));

            SqlCommand cmd = new SqlCommand("R_ACMAIN", con);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
            cmd.Parameters.AddWithValue("@platform", platform);
            cmd.Parameters.AddWithValue("@nw_eng", nwEng);

            cmd.CommandTimeout = 120;

Console.WriteLine("========== R_ACMAIN ==========");
Console.WriteLine($"FROM     : {from:yyyy-MM-dd}");
Console.WriteLine($"TO       : {to:yyyy-MM-dd}");
Console.WriteLine($"PLATFORM : {platform}");
Console.WriteLine($"NW_ENG   : {nwEng}");
Console.WriteLine("==============================");

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();
            int rowCount = 0;

            while (dr.Read())
            {

                rowCount++;

                list.Add(new ACMainFailureModel
                {
                    NODE = dr["NODE"]?.ToString() ?? "",
                    ALARM_TYPE = dr["ALARM_TYPE"]?.ToString() ?? "",
                    PLATFORM = dr["PLATFORM"]?.ToString() ?? "",

                    FAULT_TIME = dr["FAULT_TIME"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(dr["FAULT_TIME"]),

                    CLOSE_TIME = dr["CLOSE_TIME"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(dr["CLOSE_TIME"]),

                    REGION = dr["REGION"]?.ToString() ?? "",
                    PROVINCE = dr["PROVINCE"]?.ToString() ?? "",
                    NW_ENG = dr["NW_ENG"]?.ToString() ?? "",
                    RTOM = dr["RTOM"]?.ToString() ?? "",

                    DURATION = dr["DURATION"] == DBNull.Value
                        ? 0
                        : Convert.ToDouble(dr["DURATION"])
                });
            }

            Console.WriteLine($"Rows from SQL = {rowCount}");

            return list;
        }
    }
}