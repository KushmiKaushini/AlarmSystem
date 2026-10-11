using AlarmSystem.Models;
using Microsoft.Data.SqlClient;

namespace AlarmSystem.Services
{
    public class EventService
    {
        private readonly IConfiguration _configuration;

        public EventService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("TMS"));
        }

        public List<Event> GetEvents()
        {
            List<Event> list = new();

            using SqlConnection conn = GetConnection();

            conn.Open();

            SqlCommand cmd = new SqlCommand(@"
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
ORDER BY CLARITY_COMMENTS.event_time DESC", conn);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                Event item = new();

                item.REF = dr["problem_no"].ToString() ?? "";
                item.CIRCUIT = dr["circuit_name"].ToString() ?? "";
                item.CUSTOMER = dr["customer_name"].ToString() ?? "";
                item.COMMENT = dr["description"].ToString() ?? "";

                if (dr["event_time"] != DBNull.Value)
                {
                    item.DATE_TIME = Convert.ToDateTime(dr["event_time"]);

                    item.IsRecent =
                        item.DATE_TIME.AddMinutes(10) > DateTime.Now;
                }

                list.Add(item);
            }

            conn.Close();

            return list;
        }
    }
}