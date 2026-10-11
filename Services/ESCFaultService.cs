using AlarmSystem.Models.EscFault;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AlarmSystem.Services
{
    public class EscFaultService
    {
        private readonly string _connectionString;

        public EscFaultService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TMS");
        }

        public EscFaultViewModel GetTicket(int id)
{
    EscFaultViewModel model = new EscFaultViewModel();

    using SqlConnection con = new SqlConnection(_connectionString);

    con.Open();

  SqlCommand cmd = new SqlCommand("SELECT * FROM T_TICKETS WHERE ID=@ID", con);
    cmd.Parameters.AddWithValue("@ID", id);

    SqlDataReader dr = cmd.ExecuteReader();

    if (dr.Read())
    {
        model.ID = Convert.ToInt32(dr["ID"]);
        model.TICKET_TYPE = dr["TICKET_TYPE"].ToString();
        model.NODE = dr["NODE"].ToString();
        model.DESCRIPTION = dr["DESCRIPTION"].ToString();
        model.SEVERITY = dr["SEVERITY"].ToString();
        model.PLATFORM = dr["PLATFORM"].ToString();
        model.WORKGROUP = dr["WORKGROUP"].ToString();
        model.STATUS = dr["STATUS"].ToString();

        model.RESPONSIBLE_OFFICER = dr["RESPONSIBLE_OFFICER"].ToString();
        model.REPORTING_PLATFORM = dr["REPORTING_PLATFORM"].ToString();
        model.REPORTING_BY = dr["REPORTING_BY"].ToString();
        model.CREATION_METHOD = dr["CREATION_METHOD"].ToString();

        if (dr["ISSUE_TIME"] != DBNull.Value)
            model.ISSUE_TIME = Convert.ToDateTime(dr["ISSUE_TIME"]);

        if (dr["REPORTING_TIME"] != DBNull.Value)
            model.REPORTING_TIME = Convert.ToDateTime(dr["REPORTING_TIME"]);

        if (dr["ACK_TIME"] != DBNull.Value)
            model.ACK_TIME = Convert.ToDateTime(dr["ACK_TIME"]);

        if (dr["CLEAR_TIME"] != DBNull.Value)
            model.CLEAR_TIME = Convert.ToDateTime(dr["CLEAR_TIME"]);

        if (dr["CLOSE_TIME"] != DBNull.Value)
            model.CLOSE_TIME = Convert.ToDateTime(dr["CLOSE_TIME"]);

        model.ACK_BY = Convert.ToString(dr["ACK_BY"]) ?? "";
        model.CLEAR_BY = Convert.ToString(dr["CLEAR_BY"]) ?? "";
        model.CLOSE_BY = Convert.ToString(dr["CLOSE_BY"]) ?? "";
    }

    dr.Close();

    return model;
}

public List<AttributeModel> GetAttributes(int ticketId)
{
    List<AttributeModel> list = new();

    using SqlConnection con = new SqlConnection(_connectionString);

    con.Open();

    SqlCommand cmd = new SqlCommand(@"
        SELECT KEY_FIELD, VALUE_FIELD
       FROM ATTRIBUTES
        WHERE TICKET_REF=@ID", con);

    cmd.Parameters.AddWithValue("@ID", ticketId);

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new AttributeModel
        {
            KEY_FIELD = Convert.ToString(dr["KEY_FIELD"]) ?? "",
            VALUE_FIELD = Convert.ToString(dr["VALUE_FIELD"]) ?? ""
        });
    }

    dr.Close();

    return list;
}

public List<CommentModel> GetComments(int ticketId)
{
    List<CommentModel> list = new();

    using SqlConnection con = new SqlConnection(_connectionString);

    con.Open();

    SqlCommand cmd = new SqlCommand(@"
        SELECT
            COMMENT,
            COMMENT_BY,
            COMMENT_TIME
        FROM COMMENTS
        WHERE TICKET_REF=@ID
        ORDER BY COMMENT_TIME DESC", con);

    cmd.Parameters.AddWithValue("@ID", ticketId);

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new CommentModel
        {
            COMMENT_TEXT = dr["COMMENT"].ToString() ?? "",
            COMMENT_BY = dr["COMMENT_BY"].ToString() ?? "",
            COMMENT_TIME = Convert.ToDateTime(dr["COMMENT_TIME"])
        });
    }

    dr.Close();

    return list;
}

public List<EscalationModel> GetEscalations(int ticketId)
{
    List<EscalationModel> list = new();

    using SqlConnection con = new SqlConnection(_connectionString);

    con.Open();

    SqlCommand cmd = new SqlCommand(@"
        SELECT
            ID,
            GROUP_NAME,
            ESCALATION_TIME,
            ESCALATION_LEVEL
        FROM ESCALATIONS
        WHERE TICKET_REF=@ID
        ORDER BY ESCALATION_LEVEL", con);

    cmd.Parameters.AddWithValue("@ID", ticketId);

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new EscalationModel
        {
            ID = Convert.ToInt32(dr["ID"]),
            GROUP_NAME = dr["GROUP_NAME"].ToString() ?? "",
            ESCALATION_LEVEL = dr["ESCALATION_LEVEL"].ToString() ?? "",
            ESCALATION_TIME = Convert.ToDateTime(dr["ESCALATION_TIME"])
        });
    }

    dr.Close();

    return list;
}

    }
}
    
