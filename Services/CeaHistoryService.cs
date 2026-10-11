using AlarmSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class CeaHistoryService
    {
        private readonly SqlConnection _connection;

        public CeaHistoryService(SqlConnection connection)
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
    Text = "ALL",
    Value = "ALL"
});

            try
            {
                using SqlCommand cmd = new SqlCommand(
                    @"SELECT DISTINCT NW_ENG
                      FROM HISTORY_FAULTS
                      WHERE PLATFORM = 'CEA'
                        AND NW_ENG IS NOT NULL
                        AND LTRIM(RTRIM(NW_ENG)) <> ''
                      ORDER BY NW_ENG",
                    _connection);

                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new SelectListItem
                    {
                        Text = dr["NW_ENG"].ToString(),
                        Value = dr["NW_ENG"].ToString()
                    });
                }
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
            }

            return list;
        }

        //====================================================
        // GET HISTORY
        //====================================================
        public List<CeaHistoryAlarmModel> GetHistory(
            DateTime from,
            DateTime to,
            string nwEng)
        {
            List<CeaHistoryAlarmModel> list = new();

            try
            {
                using SqlCommand cmd = new SqlCommand(
                    "REPORT_CEA_CAUSEOFFAULT",
                    _connection);

                cmd.CommandType = CommandType.StoredProcedure;

                // Strongly typed parameters
                cmd.Parameters.Add("@from", SqlDbType.DateTime).Value = from;
                cmd.Parameters.Add("@to", SqlDbType.DateTime).Value = to;
                cmd.Parameters.Add("@nw_eng", SqlDbType.VarChar, 100).Value =
                    string.IsNullOrWhiteSpace(nwEng) ? "ALL" : nwEng.Trim();

                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new CeaHistoryAlarmModel
                    {
                        ID = Convert.ToInt32(dr["ID"]),
                        NODE = dr["NODE"]?.ToString(),
                        IP = dr["IP"]?.ToString(),

                        FAULT_TIME = Convert.ToDateTime(dr["FAULT_TIME"]),

                        CLOSE_TIME = dr["CLOSE_TIME"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(dr["CLOSE_TIME"]),

                        Minutes = dr["Minutes"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(dr["Minutes"]),

                        FAULT = dr["FAULT"]?.ToString(),
                        NW_ENG = dr["NW_ENG"]?.ToString(),
                        Province = dr["Province"]?.ToString(),
                        Region = dr["Region"]?.ToString(),
                        TAG = dr["TAG"]?.ToString()
                    });
                }
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