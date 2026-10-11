using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class EventNewService
    {
        private readonly IConfiguration _configuration;

        public EventNewService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("TMS"));
        }

        public List<EventNew> GetEvents(string workGroup)
        {
            List<EventNew> list = new();

            using SqlConnection conn = GetConnection();

            string sql = @"
SELECT
problem_no,
CLARITY_COMMENTS.event_time,
circuit_name,
customer_name,
CLARITY_COMMENTS.description
FROM CLARITY_COMMENTS
LEFT JOIN CLARITY_DATA_NW_FAULTS
ON docket_id = problem_no
WHERE problem_no IS NOT NULL
AND CLARITY_DATA_NW_FAULTS.workgroup=@workgroup
ORDER BY CLARITY_COMMENTS.event_time DESC";

            SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@workgroup", workGroup);

            conn.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                EventNew item = new();

                item.REF = dr["problem_no"].ToString();

                if (dr["event_time"] != DBNull.Value)
                    item.EVENT_TIME =
                        Convert.ToDateTime(dr["event_time"]);

                item.CIRCUIT =
                    dr["circuit_name"].ToString();

                item.CUSTOMER =
                    dr["customer_name"].ToString();

                item.DESCRIPTION =
                    dr["description"].ToString();

                list.Add(item);
            }

            conn.Close();

            return list;
        }
    }
}