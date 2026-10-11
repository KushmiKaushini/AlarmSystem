using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using AlarmSystem.Models.ViewModels;

namespace AlarmSystem.Services
{
    public class FaultService
    {
        private readonly SqlConnection _connection;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public FaultService(SqlConnection connection, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
        }

        public Ticket GetTicket(long id)
        {
            Ticket ticket = null;

            string sql = "SELECT * FROM T_TICKETS WHERE ID=@id";

            SqlCommand cmd = new SqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("@id", id);

            _connection.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                ticket = new Ticket()
                {
                    ID = Convert.ToInt64(dr["ID"]),
                    TICKET_TYPE = dr["TICKET_TYPE"].ToString(),
                    NODE = dr["NODE"].ToString(),
                    DESCRIPTION = dr["DESCRIPTION"].ToString(),
                    ISSUE_TIME = dr["ISSUE_TIME"] as DateTime?,
                    CLEAR_TIME = dr["CLEAR_TIME"] as DateTime?,
                    SEVERITY = dr["SEVERITY"].ToString(),
                    TAG = dr["TAG"].ToString(),
                    PLATFORM = dr["PLATFORM"].ToString(),
                    WORKGROUP = dr["WORKGROUP"].ToString(),
                    STATUS = dr["STATUS"].ToString(),

                    RESPONSIBLE_OFFICER = dr["RESPONSIBLE_OFFICER"].ToString(),
                    CREATION_METHOD = dr["CREATION_METHOD"].ToString(),
                    REPORTING_PLATFORM = dr["REPORTING_PLATFORM"].ToString(),
                    REPORTING_BY = dr["REPORTING_BY"].ToString(),
                    REPORTING_TIME = dr["REPORTING_TIME"] as DateTime?,
                    ACK_TIME = dr["ACK_TIME"] as DateTime?,
                    ACK_BY = dr["ACK_BY"].ToString(),
                    CLOSE_TIME = dr["CLOSE_TIME"] as DateTime?,
                    CLOSE_BY = dr["CLOSE_BY"].ToString()
                };
            }

            dr.Close();
            _connection.Close();

            return ticket;
        }
        public List<TicketAttribute> GetAttributes(long ticketId)
{
    List<TicketAttribute> list = new List<TicketAttribute>();

    string sql = "SELECT * FROM ATTRIBUTES WHERE TICKET_ID=@id";

    SqlCommand cmd = new SqlCommand(sql, _connection);
    cmd.Parameters.AddWithValue("@id", ticketId);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())


    {
        list.Add(new TicketAttribute()
        {
            ID = Convert.ToInt64(dr["ID"]),
            TICKET_ID = Convert.ToInt64(dr["TICKET_ID"]),
            KEY_FIELD = dr["KEY_FIELD"].ToString(),
            VALUE_FIELD = dr["VALUE_FIELD"].ToString(),
            ENTER_DATE = dr["ENTER_DATE"] == DBNull.Value
                ? null
                : Convert.ToDateTime(dr["ENTER_DATE"])
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}

public List<Comment> GetComments(long ticketId)
{
    List<Comment> list = new List<Comment>();

    string sql = "SELECT * FROM COMMENTS WHERE TICKET_REF=@id";

    SqlCommand cmd = new SqlCommand(sql, _connection);
    cmd.Parameters.AddWithValue("@id", ticketId);

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
            TICKET_REF = Convert.ToInt64(dr["TICKET_REF"]),
            OTHER_REF = dr["OTHER_REF"].ToString(),
            COMMENT_BY = dr["COMMENT_BY"].ToString()
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}

public List<EscalationPrimary> GetEscalations(long ticketId)
{
    List<EscalationPrimary> list = new List<EscalationPrimary>();

    string sql = "SELECT * FROM ESCALATIONS_PRIMARY WHERE TICKET_ID=@id";

    SqlCommand cmd = new SqlCommand(sql, _connection);
    cmd.Parameters.AddWithValue("@id", ticketId);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new EscalationPrimary()
        {
            ID = Convert.ToInt64(dr["ID"]),
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
public List<Ticket> GetAllFaults()
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
            TAG = dr["TAG"].ToString(),
            PLATFORM = dr["PLATFORM"].ToString(),
            DESCRIPTION = dr["DESCRIPTION"].ToString(),
            ISSUE_TIME = dr["ISSUE_TIME"] == DBNull.Value ? null : Convert.ToDateTime(dr["ISSUE_TIME"]),
            REPORTING_TIME = dr["REPORTING_TIME"] == DBNull.Value ? null : Convert.ToDateTime(dr["REPORTING_TIME"]),
            STATUS = dr["STATUS"].ToString(),
            RESPONSIBLE_OFFICER = dr["RESPONSIBLE_OFFICER"].ToString()
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}
public List<EscalationSecondary> GetEscalationMembers(long escId)
{
    List<EscalationSecondary> list = new List<EscalationSecondary>();

    string sql = @"SELECT *
                   FROM ESCALATIONS_SECONDARY
                   WHERE ESC_ID=@id";

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
            SCHEDULE_TIME = dr["SCHEDULE_TIME"] == DBNull.Value
                ? null
                : Convert.ToDateTime(dr["SCHEDULE_TIME"])
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}
public List<Group> GetGroups()
{
    List<Group> list = new List<Group>();

    string sql = "SELECT * FROM GROUPS ORDER BY GROUP_NAME";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new Group()
        {
            ID = Convert.ToInt64(dr["ID"]),
            GROUP_NAME = dr["GROUP_NAME"].ToString(),
            DESCRIPTION = dr["DESCRIPTION"].ToString(),
            PARENT_ID = Convert.ToInt64(dr["PARENT_ID"]),
            PROFILE = dr["PROFILE"].ToString(),
            GROUP_ORDER = dr["GROUP_ORDER"] == DBNull.Value
                            ? null
                            : Convert.ToInt32(dr["GROUP_ORDER"])
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}

public void AckTicket(long id)
{
    string sql = @"
UPDATE T_TICKETS
SET
    ACK_TIME = GETDATE(),
    ACK_BY = 'SYSTEM'
WHERE ID=@id";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@id", id);

    _connection.Open();

    cmd.ExecuteNonQuery();

    _connection.Close();
}
public void ClearTicket(long id)
{
    string sql = @"
UPDATE T_TICKETS
SET
    CLEAR_TIME = GETDATE()
WHERE ID=@id";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@id", id);

    _connection.Open();

    cmd.ExecuteNonQuery();

    _connection.Close();
}
public void CloseTicket(long id)
{
    string sql = @"
UPDATE T_TICKETS
SET
    CLOSE_TIME = GETDATE(),
    CLOSE_BY = 'SYSTEM',
    STATUS = 'CLOSED'
WHERE ID=@id";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@id", id);

    _connection.Open();

    cmd.ExecuteNonQuery();

    _connection.Close();
}
public void UpdateTicketTags(long ticketId, string tags)
{
    string sql = @"
UPDATE T_TICKETS
SET TAG=@TAG
WHERE ID=@ID";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@ID", ticketId);
    cmd.Parameters.AddWithValue("@TAG", tags);

    _connection.Open();
    cmd.ExecuteNonQuery();
    _connection.Close();
}
public void UpdateResponsibleOfficer(long ticketId, string officer)
{
    string sql = @"
UPDATE T_TICKETS
SET RESPONSIBLE_OFFICER=@OFFICER
WHERE ID=@ID";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@ID", ticketId);
    cmd.Parameters.AddWithValue("@OFFICER", officer);

    _connection.Open();
    cmd.ExecuteNonQuery();
    _connection.Close();
}
public string GetGroupName(long id)
{
    string groupName = "";

    string sql = @"
SELECT GROUP_NAME
FROM GROUPS
WHERE ID=@ID";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@ID", id);

    _connection.Open();

    object result = cmd.ExecuteScalar();

    if (result != null)
        groupName = result.ToString();

    _connection.Close();

    return groupName;
}
public List<Group> GetLevel3Groups()
{
    List<Group> list = new();

    string sql = @"
SELECT *
FROM GROUPS
WHERE GROUP_ORDER=3
ORDER BY GROUP_NAME";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new Group
        {
            ID = Convert.ToInt64(dr["ID"]),
            GROUP_NAME = dr["GROUP_NAME"].ToString()
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}
public List<Group> GetChildGroups(long parentId)
{
    List<Group> list = new();

    string sql = @"
SELECT *
FROM GROUPS
WHERE PARENT_ID=@ID
ORDER BY GROUP_NAME";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@ID", parentId);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new Group
        {
            ID = Convert.ToInt64(dr["ID"]),
            GROUP_NAME = dr["GROUP_NAME"].ToString()
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}
public List<EscalationSecondary> GetGroupMembers(long groupId)
{
    List<EscalationSecondary> list = new();

    string sql = @"
SELECT *
FROM GROUP_MEMBERS
WHERE GROUP_ID=@ID";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@ID", groupId);

    _connection.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new EscalationSecondary
        {
            NAME = dr["NAME"].ToString(),
            MOBILE = dr["MOBILE"].ToString(),
            SVC = dr["SVC"].ToString()
        });
    }

    dr.Close();
    _connection.Close();

    return list;
}
public void SubmitComment(long ticketId, string comment, string user)
{
    string sql = @"
    INSERT INTO COMMENTS
    (
        TICKET_REF,
        COMMENT_TEXT,
        COMMENT_TIME,
        COMMENT_BY
    )
    VALUES
    (
        @TICKET_REF,
        @COMMENT_TEXT,
        GETDATE(),
        @COMMENT_BY
    )";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@TICKET_REF", ticketId);
    cmd.Parameters.AddWithValue("@COMMENT_TEXT", comment);
    cmd.Parameters.AddWithValue("@COMMENT_BY", user);

    _connection.Open();

    cmd.ExecuteNonQuery();

    _connection.Close();
}

public void InsertEscalation(long ticketId,
                             long groupId,
                             string groupName,
                             string level,
                             string svc,
                             string name)
{
    string sql = @"
    INSERT INTO ESCALATIONS_PRIMARY
    (
        TICKET_ID,
        GROUP_ID,
        GROUP_NAME,
        ESCALATION_LEVEL,
        ESCALATE_BY_SVC,
        ESCALATE_BY_NAME,
        ESCALATION_TIME
    )
    VALUES
    (
        @TICKET_ID,
        @GROUP_ID,
        @GROUP_NAME,
        @LEVEL,
        @SVC,
        @NAME,
        GETDATE()
    )";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@TICKET_ID", ticketId);
    cmd.Parameters.AddWithValue("@GROUP_ID", groupId);
    cmd.Parameters.AddWithValue("@GROUP_NAME", groupName);
    cmd.Parameters.AddWithValue("@LEVEL", level);
    cmd.Parameters.AddWithValue("@SVC", svc);
    cmd.Parameters.AddWithValue("@NAME", name);

    _connection.Open();

    cmd.ExecuteNonQuery();

    _connection.Close();
}

public string GetMessage(long ticketId)
{
    string message = "";

    string sql = @"
    SELECT MESSAGE_TEXT
    FROM T_TICKETS
    WHERE ID=@ID";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@ID", ticketId);

    _connection.Open();

    object obj = cmd.ExecuteScalar();

    if (obj != null && obj != DBNull.Value)
        message = obj.ToString();

    _connection.Close();

    return message;
}

public void UpdateMessage(long ticketId, string message)
{
    string sql = @"
    UPDATE T_TICKETS
    SET MESSAGE_TEXT=@MESSAGE
    WHERE ID=@ID";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@ID", ticketId);
    cmd.Parameters.AddWithValue("@MESSAGE", message);

    _connection.Open();

    cmd.ExecuteNonQuery();

    _connection.Close();
}
public void InsertEscalationSecondary(
    long escId,
    string svc,
    string name,
    string mobile,
    string cli,
    string message)
{
    string sql = @"
    INSERT INTO ESCALATIONS_SECONDARY
    (
        ESC_ID,
        SVC,
        NAME,
        MOBILE,
        CLI,
        MESSAGE,
        SEND_STATUS,
        SCHEDULE_TIME
    )
    VALUES
    (
        @ESC_ID,
        @SVC,
        @NAME,
        @MOBILE,
        @CLI,
        @MESSAGE,
        'PENDING',
        GETDATE()
    )";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@ESC_ID", escId);
    cmd.Parameters.AddWithValue("@SVC", svc);
    cmd.Parameters.AddWithValue("@NAME", name);
    cmd.Parameters.AddWithValue("@MOBILE", mobile);
    cmd.Parameters.AddWithValue("@CLI", cli);
    cmd.Parameters.AddWithValue("@MESSAGE", message);

    _connection.Open();

    cmd.ExecuteNonQuery();

    _connection.Close();
}
public string GetCli()
{
    return "SYSTEM";
}

public string GetTicketStatus(long ticketId)
{
    string status = "";

    string sql = "SELECT STATUS FROM T_TICKETS WHERE ID=@ID";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    cmd.Parameters.AddWithValue("@ID", ticketId);

    _connection.Open();

    object obj = cmd.ExecuteScalar();

    if (obj != null)
        status = obj.ToString();

    _connection.Close();

    return status;
}
public long GetLastEscalationId()
{
    string sql = "SELECT MAX(ID) FROM ESCALATIONS_PRIMARY";

    SqlCommand cmd = new SqlCommand(sql, _connection);

    _connection.Open();

    object obj = cmd.ExecuteScalar();

    _connection.Close();

    if (obj == DBNull.Value || obj == null)
        return 0;

    return Convert.ToInt64(obj);
}


public List<MapNodeViewModel> GetMapNodes(DateTime from, DateTime to)
{
    List<MapNodeViewModel> list = new();

    // Query uses NW_fault (the real alarm table in FaultDB) joined to
    // vw_coordinates_normalized for lat/lng via the CCT (circuit) key.
    // Node type is derived from the NODE name pattern since FaultDB has no
    // separate PLATFORM column.
    string sql = @"
SELECT
    f.NODE                                              AS Location,
    ISNULL(f.NW_ENG, '')                               AS NWENG,
    ISNULL(TRY_CAST(c.latitude  AS decimal(18,8)), 0)  AS latitude,
    ISNULL(TRY_CAST(c.longitude AS decimal(18,8)), 0)  AS longitude,
    CASE
        WHEN PATINDEX('%Critical :%', f.FAULT) > 0 THEN 2
        WHEN PATINDEX('%Minor :%',    f.FAULT) > 0 THEN 1
        ELSE 0
    END AS status,
    CASE WHEN UPPER(f.NODE) LIKE '%MSAN%' THEN 'Y' ELSE NULL END AS MSAN,
    CASE WHEN UPPER(f.NODE) LIKE '%CEA%'  THEN 'Y' ELSE NULL END AS CEANode,
    CASE WHEN UPPER(f.NODE) LIKE '%RPB%'  THEN 'Y' ELSE NULL END AS RPB,
    f.FAULT                                             AS Progress
FROM dbo.NW_fault f
OUTER APPLY (
    SELECT TOP 1 c.latitude, c.longitude
    FROM dbo.vw_coordinates_normalized c
    WHERE NULLIF(LTRIM(RTRIM(f.CCTID)), '') IS NOT NULL
      AND LOWER(LTRIM(RTRIM(ISNULL(c.CCT, '')))) = LOWER(LTRIM(RTRIM(ISNULL(f.CCTID, ''))))
      AND c.latitude  IS NOT NULL
      AND c.longitude IS NOT NULL
    ORDER BY c.CCT
) c
WHERE f.FAULT_TIME >= @from
  AND f.FAULT_TIME <  @to
ORDER BY f.FAULT_TIME DESC";

    var connStr = _configuration.GetConnectionString("FaultDb")
        ?? throw new InvalidOperationException("Connection string 'FaultDb' is not configured.");

    using SqlConnection conn = new SqlConnection(connStr);
    SqlCommand cmd = new SqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@from", from);
    cmd.Parameters.AddWithValue("@to", to);

    conn.Open();

    using SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new MapNodeViewModel
        {
            Location  = dr["Location"].ToString() ?? "",
            Progress  = dr["Progress"]?.ToString() ?? "",
            NWENG     = dr["NWENG"]?.ToString() ?? "",
            Latitude  = dr["latitude"]  != DBNull.Value ? Convert.ToDecimal(dr["latitude"])  : 0,
            Longitude = dr["longitude"] != DBNull.Value ? Convert.ToDecimal(dr["longitude"]) : 0,
            Status    = dr["status"]    != DBNull.Value ? Convert.ToInt32(dr["status"])   : 0,
            MSAN      = dr["MSAN"]?.ToString(),
            CEANode   = dr["CEANode"]?.ToString(),
            RPB       = dr["RPB"]?.ToString()
        });
    }

    dr.Close();
    conn.Close();

    return list;
}
    }
}
