using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class GroupService
    {
        private readonly SqlConnection _connection;

        public GroupService(IConfiguration configuration)
        {
            _connection = new SqlConnection(
                configuration.GetConnectionString("TMS"));
        }

        public List<EscalationGroup> GetGroups(int parentId, string profile)
        {
           List<EscalationGroup> list = new List<EscalationGroup>();

            string sql = @"
                SELECT ID, GROUP_NAME, PARENT_ID
                FROM GROUPS
                WHERE PARENT_ID=@ParentId
                AND PROFILE=@Profile
                ORDER BY GROUP_NAME";

            SqlCommand cmd = new SqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("@ParentId", parentId);
            cmd.Parameters.AddWithValue("@Profile", profile);

            _connection.Open();

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new EscalationGroup
                {
                    ID = Convert.ToInt32(reader["ID"]),
                    GROUP_NAME = reader["GROUP_NAME"].ToString(),
                    PARENT_ID = Convert.ToInt32(reader["PARENT_ID"])
                });
            }

            reader.Close();
            _connection.Close();

            return list;
        }

        public string GetGroupName(int id)
        {
            string sql = "SELECT GROUP_NAME FROM GROUPS WHERE ID=@Id";

            SqlCommand cmd = new SqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("@Id", id);

            _connection.Open();

            object result = cmd.ExecuteScalar();

            _connection.Close();

            if (result == null)
                return "";

            return result.ToString();
        }
    }
}