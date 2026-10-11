using Microsoft.Data.SqlClient;
using AlarmSystem.Models;
using AlarmSystem.Models.EscFault;

namespace AlarmSystem.Services
{
    public class EscalationService
    {
        private readonly SqlConnection _connection;

        public EscalationService(SqlConnection connection)
        {
            _connection = connection;
        }

        public EscalationPrimary GetPrimary(long escId)
        {
            EscalationPrimary primary = null;

            string sql = "SELECT * FROM ESCALATIONS_PRIMARY WHERE ID=@id";

            SqlCommand cmd = new SqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("@id", escId);

            _connection.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                primary = new EscalationPrimary()
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

            return primary;
        }

        public List<EscalationSecondary> GetSecondary(long escId)
        {
            List<EscalationSecondary> list = new List<EscalationSecondary>();

            string sql = "SELECT * FROM ESCALATIONS_SECONDARY WHERE ESC_ID=@id";

            SqlCommand cmd = new SqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("@id", escId);

            _connection.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new EscalationSecondary()
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
                    SCHEDULE_TIME = Convert.ToDateTime(dr["SCHEDULE_TIME"])
                });
            }

            dr.Close();
            _connection.Close();

            return list;
        }

        public List<EscalationPrimary> GetAllPrimary()
        {
            List<EscalationPrimary> list = new List<EscalationPrimary>();

           string sql = @"
SELECT EP.*, T.TICKET_TYPE, T.PLATFORM
FROM ESCALATIONS_PRIMARY EP
INNER JOIN T_TICKETS T
ON EP.TICKET_ID = T.ID
ORDER BY EP.ESCALATION_TIME DESC";

            SqlCommand cmd = new SqlCommand(sql, _connection);

            _connection.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new EscalationPrimary()
{
    ID = Convert.ToInt64(dr["ID"]),
    TICKET_ID = Convert.ToInt64(dr["TICKET_ID"]),
    GROUP_NAME = dr["GROUP_NAME"].ToString(),
    ESCALATION_TIME = Convert.ToDateTime(dr["ESCALATION_TIME"]),
    ESCALATE_BY_SVC = dr["ESCALATE_BY_SVC"].ToString(),
    ESCALATE_BY_NAME = dr["ESCALATE_BY_NAME"].ToString(),
    ESCALATION_LEVEL = dr["ESCALATION_LEVEL"].ToString(),

    TICKET_TYPE = dr["TICKET_TYPE"].ToString(),
    PLATFORM = dr["PLATFORM"].ToString()
});
            }

            dr.Close();
            _connection.Close();

            return list;
        }
        public List<Ticket> GetTickets()
{
    List<Ticket> list = new List<Ticket>();

    string sql = @"SELECT *
                   FROM T_TICKETS
                   ORDER BY ISSUE_TIME DESC";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
       list.Add(new Ticket()
{
    ID = Convert.ToInt64(dr["ID"]),
    NODE = dr["NODE"].ToString(),
    SEVERITY = dr["SEVERITY"].ToString(),
    TICKET_TYPE = dr["TICKET_TYPE"].ToString(),
    TAG = dr["TAG"].ToString(),
    PLATFORM = dr["PLATFORM"].ToString(),
    WORKGROUP = dr["WORKGROUP"].ToString(),
    DESCRIPTION = dr["DESCRIPTION"].ToString(),
    CREATION_METHOD = dr["CREATION_METHOD"].ToString(),

    ISSUE_TIME = dr["ISSUE_TIME"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(dr["ISSUE_TIME"]),
    
    REPORTING_TIME = dr["REPORTING_TIME"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(dr["REPORTING_TIME"]),

    REPORTING_BY = dr["REPORTING_BY"].ToString(),
    REPORTING_PLATFORM = dr["REPORTING_PLATFORM"].ToString(),
    STATUS = dr["STATUS"].ToString(),
    RESPONSIBLE_OFFICER = dr["RESPONSIBLE_OFFICER"].ToString(),

    MESSAGE_TEXT = dr["MESSAGE_TEXT"] == DBNull.Value
                    ? ""
                    : dr["MESSAGE_TEXT"].ToString()
});
    }

    dr.Close();
    _connection.Close();

    return list;
}
public Ticket GetTicket(long id)
{
    return GetTickets().FirstOrDefault(x => x.ID == id);
}
public List<TicketAttribute> GetAttributes(long id)
{
    List<TicketAttribute> list = new();

    string sql = "SELECT * FROM ATTRIBUTES WHERE TICKET_ID=@id";

    SqlCommand cmd = new SqlCommand(sql, _connection);
    cmd.Parameters.AddWithValue("@id", id);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new TicketAttribute()
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
public List<Comment> GetComments(long id)
{
    List<Comment> list = new();

    string sql = @"SELECT *
                   FROM COMMENTS
                   WHERE TICKET_REF=@id
                   ORDER BY COMMENT_TIME DESC";

    SqlCommand cmd = new SqlCommand(sql, _connection);
    cmd.Parameters.AddWithValue("@id", id);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new Comment()
        {
            ID = Convert.ToInt64(dr["ID"]),
            COMMENT_TEXT = dr["COMMENT_TEXT"].ToString(),
            COMMENT_TIME = dr["COMMENT_TIME"] == DBNull.Value
                ? null
                : Convert.ToDateTime(dr["COMMENT_TIME"]),
            COMMENT_BY = dr["COMMENT_BY"].ToString()
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}
public List<EscalationPrimary> GetEscalationsByTicket(long ticketId)
{
    List<EscalationPrimary> list = new List<EscalationPrimary>();

    string sql = @"SELECT *
                   FROM ESCALATIONS_PRIMARY
                   WHERE TICKET_ID=@id
                   ORDER BY ESCALATION_TIME";

    SqlCommand cmd = new SqlCommand(sql, _connection);
    cmd.Parameters.AddWithValue("@id", ticketId);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new EscalationPrimary()
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
public EscalationPrimary GetPrimaryByTicket(long ticketId)
{
    EscalationPrimary primary = null;

    string sql = @"SELECT TOP 1 *
                   FROM ESCALATIONS_PRIMARY
                   WHERE TICKET_ID=@id
                   ORDER BY ESCALATION_TIME";

    SqlCommand cmd = new SqlCommand(sql, _connection);
    cmd.Parameters.AddWithValue("@id", ticketId);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    if (dr.Read())
    {
        primary = new EscalationPrimary()
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

    return primary;
}
public List<Group> GetGroups(int parentId, string profile)
{
    List<Group> list = new List<Group>();

    string sql = @"SELECT ID, GROUP_NAME, PARENT_ID, PROFILE
                   FROM GROUPS
                   WHERE PARENT_ID=@pid
                   AND PROFILE=@profile
                   ORDER BY GROUP_NAME";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@pid", parentId);
    cmd.Parameters.AddWithValue("@profile", profile);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new Group()
        {
            ID = Convert.ToInt32(dr["ID"]),
            GROUP_NAME = dr["GROUP_NAME"].ToString(),
            PARENT_ID = Convert.ToInt32(dr["PARENT_ID"]),
            PROFILE = dr["PROFILE"].ToString()
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}
public void DeleteEscalation(long id)
{
    string sql = @"DELETE FROM ESCALATIONS_PRIMARY
                   WHERE ID=@id";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@id", id);

    _connection.Open();

    cmd.ExecuteNonQuery();

    _connection.Close();
}

    }
    
}