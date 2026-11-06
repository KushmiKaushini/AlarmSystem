using AlarmSystem.Models;
using Microsoft.Data.SqlClient;

namespace AlarmSystem.Services
{
    public class InboxService
    {
        private readonly IConfiguration _configuration;

        public InboxService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("SmsDb"));
        }

        public List<SmsInbox> GetInbox()
        {
            List<SmsInbox> list = new();

            try
            {
                using SqlConnection conn = GetConnection();

                string sql = @"
SELECT
    receivedtime AS Time,
    mobile AS Sender,
    message AS Message,
    from_mobile AS Destination
FROM inbox
WHERE
(
    from_mobile = '94112029595'
    OR from_mobile = '94113134814'
)
AND receivedtime > '2021-10-25 10:39:00'
ORDER BY receivedtime DESC";

                SqlCommand cmd = new SqlCommand(sql, conn);

                conn.Open();

                using SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    SmsInbox item = new SmsInbox();

                    item.Time = Convert.ToDateTime(dr["Time"]);
                    item.Sender = dr["Sender"].ToString() ?? "";
                    item.Message = dr["Message"].ToString() ?? "";
                    item.Destination = dr["Destination"].ToString() ?? "";

                    item.IsRecent = item.Time.AddMinutes(10000) > DateTime.Now;

                    list.Add(item);
                }
            }
            catch (SqlException)
            {
                // SMSPORTAL database is optional in local development environment
            }

            return list;
        }
    }
}