using AlarmSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class MsanHistoryService
    {
        private readonly SqlConnection _connection;

        public MsanHistoryService(SqlConnection connection)
        {
            _connection = connection;
        }

        //====================================================
        // GET NETWORK ENGINEERS
        //====================================================
        public List<SelectListItem> GetNetworkEngineers()
        {
            List<SelectListItem> list = new();

            list.Add(new SelectListItem
            {
                Text = "~ALL~",
                Value = "~ALL~"
            });

            try
            {
               using SqlCommand cmd = new SqlCommand(
    "SELECT DISTINCT NW_ENG FROM HISTORY_FAULTS WHERE NW_ENG IS NOT NULL ORDER BY NW_ENG",
    _connection);

                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new SelectListItem
                    {
                       Text = dr["NW_ENG"].ToString(),
Value = dr["NW_ENG"].ToString()
                    });
                }

                dr.Close();
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
            }

            return list;
        }

        //====================================================
        // GET HISTORY ALARMS
        //====================================================
        public List<MsanHistoryAlarmModel> GetHistory(
            DateTime from,
            DateTime to,
            string engineer)
        {
            List<MsanHistoryAlarmModel> list = new();

            try
            {
                using SqlCommand cmd = new SqlCommand(
                    "REPORT_MSAN_CAUSEOFFAULT",
                    _connection);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@from", from);
                cmd.Parameters.AddWithValue("@to", to);
               if (engineer == "~ALL~")
{
    engineer = "ALL";
}

cmd.Parameters.AddWithValue("@nw_eng", engineer);

                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new MsanHistoryAlarmModel
                    {
                        ID = Convert.ToInt32(dr["ID"]),
                        NODE = dr["NODE"].ToString(),
                        PLATFORM = dr["PLATFORM"].ToString(),
                        FAULT_TIME = Convert.ToDateTime(dr["FAULT_TIME"]),
                        CLOSE_TIME = dr["CLOSE_TIME"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(dr["CLOSE_TIME"]),
                        MINUTES = Convert.ToInt32(dr["MINUTES"]),
                        FAULT = dr["FAULT"].ToString(),
                        NW_Designation = dr["NW_Designation"].ToString(),
                        Province = dr["Province"].ToString(),
                        Region = dr["Region"].ToString(),
                        TAG = dr["TAG"].ToString()
                    });
                }

                dr.Close();
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
            }

            return list;
        }
    }
}