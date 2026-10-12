using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class SearchNodeService
    {
        private readonly IConfiguration _configuration;

        public SearchNodeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<SearchNodeModel>> SearchAsync(string node)
        {
            List<SearchNodeModel> list = new();

            const string sql = @"
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

            await using var connection = new SqlConnection(_configuration.GetConnectionString("TMS"));
            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.Add("@node", SqlDbType.NVarChar, 200).Value = node ?? string.Empty;

            await connection.OpenAsync();
            await using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new SearchNodeModel
                {
                    Node = dr["node"]?.ToString() ?? string.Empty,
                    Reason = dr["reason"]?.ToString() ?? string.Empty,
                    AlarmStatus = dr["alstatus"]?.ToString() ?? string.Empty,
                    Platform = dr["vendor"]?.ToString() ?? string.Empty,
                    AlarmTime = dr["sysdatetime"] != DBNull.Value ? Convert.ToDateTime(dr["sysdatetime"]) : DateTime.MinValue
                });
            }

            return list;
        }

        public List<SearchNodeModel> Search(string node)
        {
            return SearchAsync(node).GetAwaiter().GetResult();
        }
    }
}