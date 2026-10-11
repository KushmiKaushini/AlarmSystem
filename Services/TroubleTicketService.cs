using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class TroubleTicketService
    {
        private readonly SqlConnection _connection;

        public TroubleTicketService(SqlConnection connection)
        {
            _connection = connection;
        }

        public List<TroubleTicket> GetTickets(DateTime from, DateTime to)
        {
            List<TroubleTicket> list = new();

            string sql = @"
SELECT *
FROM HISTORY_T_TICKETS
WHERE ISSUE_TIME >= @From
AND ISSUE_TIME < DATEADD(DAY,1,@To)
ORDER BY ISSUE_TIME DESC";

            using SqlCommand cmd = new SqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("@From", from);
            cmd.Parameters.AddWithValue("@To", to);

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                TroubleTicket t = new TroubleTicket();

                t.ID = dr["ID"] == DBNull.Value ? 0 : Convert.ToInt64(dr["ID"]);
                t.NODE = dr["NODE"]?.ToString();
                t.SEVERITY = dr["SEVERITY"]?.ToString();
                t.TICKET_TYPE = dr["TICKET_TYPE"]?.ToString();
                t.TAG = dr["TAG"]?.ToString();
                t.PLATFORM = dr["PLATFORM"]?.ToString();
                t.WORKGROUP = dr["WORKGROUP"]?.ToString();
                t.DESCRIPTION = dr["DESCRIPTION"]?.ToString();
                t.CREATION_METHOD = dr["CREATION_METHOD"]?.ToString();

                t.ISSUE_TIME =
                    dr["ISSUE_TIME"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(dr["ISSUE_TIME"]);

                t.REPORTING_TIME =
                    dr["REPORTING_TIME"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(dr["REPORTING_TIME"]);

                if (HasColumn(dr, "REPORTING_BY"))
                    t.REPORTING_BY = dr["REPORTING_BY"]?.ToString();

                if (HasColumn(dr, "REPORTED_TIME") &&
                    dr["REPORTED_TIME"] != DBNull.Value)
                    t.REPORTED_TIME = Convert.ToDateTime(dr["REPORTED_TIME"]);

                if (HasColumn(dr, "ASSIGNED_TO"))
                    t.ASSIGNED_TO = dr["ASSIGNED_TO"]?.ToString();

                if (HasColumn(dr, "ASSIGNED_TIME") &&
                    dr["ASSIGNED_TIME"] != DBNull.Value)
                    t.ASSIGNED_TIME = Convert.ToDateTime(dr["ASSIGNED_TIME"]);

                if (HasColumn(dr, "CLOSED_TIME") &&
                    dr["CLOSED_TIME"] != DBNull.Value)
                    t.CLOSED_TIME = Convert.ToDateTime(dr["CLOSED_TIME"]);

                if (HasColumn(dr, "STATUS"))
                    t.STATUS = dr["STATUS"]?.ToString();

                list.Add(t);
            }

            _connection.Close();

            return list;
        }

        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName,
                    StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}