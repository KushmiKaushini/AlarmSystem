using AlarmSystem.Models;
using Microsoft.Data.SqlClient;

namespace AlarmSystem.Services
{
    public class NodeDownReportService
    {
        private readonly IConfiguration _configuration;

        public NodeDownReportService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //=========================================
        // LOAD NW ENGINEERS
        //=========================================

        public List<NetworkEngineerModel> GetEngineers()
        {
            List<NetworkEngineerModel> list = new();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("TMS"));

            string sql = @"

SELECT '~ALL~' AS NW_Designation

UNION

SELECT DISTINCT NW_Designation

FROM NW_REGION_MAPPING

ORDER BY NW_Designation;

";

            SqlCommand cmd = new SqlCommand(sql, con);

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new NetworkEngineerModel
                {
                    NW_Designation = dr["NW_Designation"].ToString() ?? ""
                });
            }

            return list;
        }

        //=========================================
        // REPORT
        //=========================================

        public List<NodeDownReportModel> GetReport(
            DateTime from,
            DateTime to,
            string nwEngineer)
        {
            List<NodeDownReportModel> list = new();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("TMS"));

            SqlCommand cmd = new SqlCommand(
                "REPORT_New_NW_MAPPING_REGION_MSAN_OPEN",
                con);

            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
            cmd.Parameters.AddWithValue("@nw_eng", nwEngineer);

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new NodeDownReportModel
                {
                    ID = Convert.ToInt32(dr["ID"]),

                    NODE = dr["NODE"]?.ToString() ?? "",

                    FAULT = dr["FAULT"]?.ToString() ?? "",

                    FAULT_TIME = dr["FAULT_TIME"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(dr["FAULT_TIME"]),

                    PLATFORM = dr["PLATFORM"]?.ToString() ?? "",

                    VENDOR = dr["VENDOR"]?.ToString() ?? "",

                    FAULT_STATUS = dr["FAULT_STATUS"]?.ToString() ?? "",

                    CLOSE_TIME = dr["CLOSE_TIME"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(dr["CLOSE_TIME"]),

                    ALARM_TYPE = dr["ALARM_TYPE"]?.ToString() ?? "",

                    sourceip = dr["sourceip"]?.ToString() ?? "",

                    Region1 = dr["Region1"]?.ToString() ?? "",

                    Province1 = dr["Province1"]?.ToString() ?? "",

                    NW_Designation = dr["NW_Designation"]?.ToString() ?? "",

                    RTOM = dr["RTOM"]?.ToString() ?? "",

                    LEA_Code = dr["LEA_Code"]?.ToString() ?? "",

                    LEA_Name = dr["LEA_Name"]?.ToString() ?? "",

                    REGION = dr["REGION"]?.ToString() ?? ""
                });
            }

            return list;
        }
    }
}