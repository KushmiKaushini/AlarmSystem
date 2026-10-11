using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class SearchNodeService
    {
        private readonly SqlConnection _connection;

        public SearchNodeService(SqlConnection connection)
        {
            _connection = connection;
        }

        public List<SearchNodeModel> Search(string node)
        {
            List<SearchNodeModel> list = new();

            string sql = @"
SELECT
    node,
    reason,
    alstatus,
    vendor,
    sysdatetime
FROM traps2
WHERE node=@node
AND reason LIKE '%AC Mains Failure%'
ORDER BY sysdatetime DESC";

            using SqlCommand cmd = new(sql, _connection);

            cmd.Parameters.AddWithValue("@node", node);

            _connection.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new SearchNodeModel
                {
                    Node = dr["node"].ToString(),
                    Reason = dr["reason"].ToString(),
                    AlarmStatus = dr["alstatus"].ToString(),
                    Platform = dr["vendor"].ToString(),
                    AlarmTime = Convert.ToDateTime(dr["sysdatetime"])
                });
            }

            _connection.Close();

            return list;
        }
    }
}