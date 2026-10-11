using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class AlarmDelayService
    {
        private readonly SqlConnection _connection;

        public AlarmDelayService(SqlConnection connection)
        {
            _connection = connection;
        }

        public List<AlarmDelayModel> GetAlarmDelays()
        {
            List<AlarmDelayModel> list = new();

            try
            {
                using SqlCommand cmd = new SqlCommand(@"
                    SELECT *
                    FROM ALARM_TYPES_DELAY
                    ORDER BY PLATFORM DESC", _connection);

                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new AlarmDelayModel
                    {
                        PLATFORM = dr["PLATFORM"].ToString(),
                        ALARM = dr["ALARM"].ToString(),
                        DELAY_MINUTES = Convert.ToInt32(dr["DELAY_MINUTES"]),
                        ALARM_TYPE = dr["ALARM_TYPE"].ToString()
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