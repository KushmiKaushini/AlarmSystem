using AlarmSystem.Models.History;
using Microsoft.Data.SqlClient;

namespace AlarmSystem.Services
{
    public class HistoryService
    {
        private readonly SqlConnection _connection;

        public HistoryService(SqlConnection connection)
        {
            _connection = connection;
        }

        //==========================================
        // GET HISTORY TICKETS
        //==========================================
        public List<HistoryTicket> GetHistoryTickets(
            DateTime from,
            DateTime to,
            string ticketType)
        {
            List<HistoryTicket> list = new();

            string sql = @"
SELECT *
FROM HISTORY_T_TICKETS
WHERE TICKET_TYPE=@type
AND
(
    CONVERT(date, ISSUE_TIME)
        BETWEEN CONVERT(date,@from)
        AND CONVERT(date,@to)

    OR

    CONVERT(date, CLOSE_TIME)
        BETWEEN CONVERT(date,@from)
        AND CONVERT(date,@to)
)
ORDER BY ISSUE_TIME DESC";

            SqlCommand cmd = new SqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
            cmd.Parameters.AddWithValue("@type", ticketType);

            _connection.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new HistoryTicket()
                {
                    ID = Convert.ToInt64(dr["ID"]),
                    NODE = dr["NODE"].ToString(),
                    SEVERITY = dr["SEVERITY"].ToString(),
                    TAG = dr["TAG"].ToString(),
                    PLATFORM = dr["PLATFORM"].ToString(),
                    DESCRIPTION = dr["DESCRIPTION"].ToString(),

                    ISSUE_TIME = dr["ISSUE_TIME"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(dr["ISSUE_TIME"]),

                    REPORTING_TIME = dr["REPORTING_TIME"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(dr["REPORTING_TIME"]),

                    STATUS = dr["STATUS"].ToString(),
                    RESPONSIBLE_OFFICER = dr["RESPONSIBLE_OFFICER"].ToString(),

                    TICKET_TYPE = dr["TICKET_TYPE"].ToString()
                });
            }

            dr.Close();
            _connection.Close();

            return list;
        }
        //==========================================
// GET SINGLE HISTORY TICKET
//==========================================
public HistoryTicket? GetHistoryTicket(long id)
{
    return GetHistoryTickets(
        DateTime.Today.AddYears(-20),
        DateTime.Today.AddYears(20),
        "FAULT")
        .FirstOrDefault(x => x.ID == id);
}

//==========================================
// GET HISTORY ATTRIBUTES
//==========================================
public List<HistoryAttribute> GetHistoryAttributes(long ticketId)
{
    List<HistoryAttribute> list = new();

    string sql = @"SELECT *
                   FROM HISTORY_ATTRIBUTES
                   WHERE TICKET_ID=@id";

    SqlCommand cmd = new SqlCommand(sql, _connection);
    cmd.Parameters.AddWithValue("@id", ticketId);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new HistoryAttribute
        {
            ID = Convert.ToInt64(dr["ID"]),
            TICKET_ID = Convert.ToInt64(dr["TICKET_ID"]),
            KEY_FIELD = dr["KEY_FIELD"].ToString(),
            VALUE_FIELD = dr["VALUE_FIELD"].ToString()
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}

//==========================================
// GET HISTORY COMMENTS
//==========================================
public List<HistoryComment> GetHistoryComments(long ticketId)
{
    List<HistoryComment> list = new();

    string sql = @"SELECT *
                   FROM HISTORY_COMMENTS
                   WHERE TICKET_REF=@id
                   ORDER BY COMMENT_TIME DESC";

    SqlCommand cmd = new SqlCommand(sql, _connection);
    cmd.Parameters.AddWithValue("@id", ticketId);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new HistoryComment
        {
            ID = Convert.ToInt64(dr["ID"]),
            TICKET_REF = Convert.ToInt64(dr["TICKET_REF"]),
            COMMENT_TEXT = dr["COMMENT_TEXT"].ToString(),
            COMMENT_BY = dr["COMMENT_BY"].ToString(),
            COMMENT_TIME = dr["COMMENT_TIME"] == DBNull.Value
                ? null
                : Convert.ToDateTime(dr["COMMENT_TIME"])
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}

//==========================================
// GET HISTORY ESCALATIONS
//==========================================
public List<HistoryEscalationPrimary> GetHistoryEscalations(long ticketId)
{
    List<HistoryEscalationPrimary> list = new();

    string sql = @"SELECT *
                   FROM HISTORY_ESCALATIONS_PRIMARY
                   WHERE TICKET_ID=@id
                   ORDER BY ESCALATION_TIME";

    SqlCommand cmd = new SqlCommand(sql, _connection);
    cmd.Parameters.AddWithValue("@id", ticketId);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new HistoryEscalationPrimary
        {
            ID = Convert.ToInt64(dr["ID"]),
            TICKET_ID = Convert.ToInt64(dr["TICKET_ID"]),
            GROUP_NAME = dr["GROUP_NAME"].ToString(),
            ESCALATION_TIME = Convert.ToDateTime(dr["ESCALATION_TIME"]),
            ESCALATE_BY_SVC = dr["ESCALATE_BY_SVC"].ToString(),
            ESCALATE_BY_NAME = dr["ESCALATE_BY_NAME"].ToString(),
            ESCALATION_LEVEL = dr["ESCALATION_LEVEL"].ToString()
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}

//==========================================
// GET HISTORY PRIMARY
//==========================================
public HistoryEscalationPrimary? GetHistoryPrimary(long escId)
{
    string sql = @"SELECT *
                   FROM HISTORY_ESCALATIONS_PRIMARY
                   WHERE ID=@id";

    SqlCommand cmd = new SqlCommand(sql, _connection);
    cmd.Parameters.AddWithValue("@id", escId);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    HistoryEscalationPrimary? item = null;

    if (dr.Read())
    {
        item = new HistoryEscalationPrimary
        {
            ID = Convert.ToInt64(dr["ID"]),
            TICKET_ID = Convert.ToInt64(dr["TICKET_ID"]),
            GROUP_NAME = dr["GROUP_NAME"].ToString(),
            ESCALATION_TIME = Convert.ToDateTime(dr["ESCALATION_TIME"]),
            ESCALATE_BY_SVC = dr["ESCALATE_BY_SVC"].ToString(),
            ESCALATE_BY_NAME = dr["ESCALATE_BY_NAME"].ToString(),
            ESCALATION_LEVEL = dr["ESCALATION_LEVEL"].ToString()
        };
    }

    dr.Close();
    _connection.Close();

    return item;
}

//==========================================
// GET HISTORY SECONDARY
//==========================================
public List<HistoryEscalationSecondary> GetHistorySecondary(long escId)
{
    List<HistoryEscalationSecondary> list = new();

    string sql = @"SELECT *
                   FROM HISTORY_ESCALATIONS_SECONDARY
                   WHERE ESC_ID=@id";

    SqlCommand cmd = new SqlCommand(sql, _connection);
    cmd.Parameters.AddWithValue("@id", escId);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new HistoryEscalationSecondary
        {
            ID = Convert.ToInt64(dr["ID"]),
            ESC_ID = Convert.ToInt64(dr["ESC_ID"]),
            SVC = dr["SVC"].ToString(),
            NAME = dr["NAME"].ToString(),
            MOBILE = dr["MOBILE"].ToString(),
            MESSAGE = dr["MESSAGE"].ToString(),
            CLI = dr["CLI"].ToString(),
            SEND_STATUS = dr["SEND_STATUS"].ToString(),
            SENT_TIME = dr["SENT_TIME"] == DBNull.Value
                ? null
                : Convert.ToDateTime(dr["SENT_TIME"]),
            SCHEDULE_TIME = dr["SCHEDULE_TIME"] == DBNull.Value
                ? null
                : Convert.ToDateTime(dr["SCHEDULE_TIME"])
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}

    }
}