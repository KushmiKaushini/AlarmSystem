using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AlarmSystem.Services
{
    public class CurrentMsanOutageService
    {
        private readonly IConfiguration _configuration;

        public CurrentMsanOutageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("TMS"));
        }

        public List<CurrentMsanOutage> GetCurrentMsanOutages()
        {
            List<CurrentMsanOutage> list = new();

            using SqlConnection conn = GetConnection();

            SqlCommand cmd = new SqlCommand(
                "VIEW_CURRENT_MSAN_OUTAGE", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@platform", "MSAN");

            conn.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                CurrentMsanOutage model = new CurrentMsanOutage();

                model.PLATFORM = dr["PLATFORM"].ToString();

                if (dr["FAULT_TIME"] != DBNull.Value)
                    model.FAULT_TIME = Convert.ToDateTime(dr["FAULT_TIME"]);

                model.NW_ENG = dr["NW_ENG"].ToString();
                model.PROVINCE = dr["PROVINCE"].ToString();
                model.REGION = dr["REGION"].ToString();
                model.DATE = dr["DATE"].ToString();
                model.NODE = dr["NODE"].ToString();
                model.FAULT_OCCURED_AT = dr["FAULT OCCURED AT"].ToString();
                model.PENDING_HOURS = dr["PENDING HOURS"].ToString();

                list.Add(model);
            }

            conn.Close();

            return list;
        }
    }
}