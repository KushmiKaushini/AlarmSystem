using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class WorkGroupService
    {
        private readonly IConfiguration _configuration;

        public WorkGroupService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("TMS"));
        }
        public List<string> GetRegions(string area)
{
    List<string> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(
        "SELECT_REGION_07052024", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@Area", area);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(dr["REGION"].ToString());
    }

    conn.Close();

    return list;
}

public List<string> GetProvinces(string region)
{
    List<string> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(
        "SELECT_PROVINCE_07052024", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@region", region);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(dr["PROVINCE"].ToString());
    }

    conn.Close();

    return list;
}

public List<string> GetWorkGroups(string province)
{
    List<string> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(
        "SELECT_WG_07052024", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@province", province);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(dr["WORK_GROUP"].ToString());
    }

    conn.Close();

    return list;
}
public List<WorkGroupOrder> GetOrders(
    string area,
    string region,
    string province,
    string workGroup)
{
    List<WorkGroupOrder> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(
        "WO_View_08072024", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@area", area);
    cmd.Parameters.AddWithValue("@region", region);
    cmd.Parameters.AddWithValue("@province", province);
    cmd.Parameters.AddWithValue("@workgroup", workGroup);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        WorkGroupOrder item = new();

        item.CLARITY_REF = dr["CLARITY_REF"].ToString();
        item.CIRCUIT = dr["CIRCUIT"].ToString();
        item.CUSTOMER = dr["CUSTOMER"].ToString();
        item.REGION = dr["REGION"].ToString();
        item.PROVINCE = dr["PROVINCE"].ToString();
        item.OPMC_NW = dr["OPMC_NW"].ToString();
        item.WO_STATUS = dr["WO STATUS"].ToString();

        if (dr["PENDING HRS"] != DBNull.Value)
            item.PENDING_HRS =
                Convert.ToInt32(dr["PENDING HRS"]);

        item.WORKGROUP = dr["WORKGROUP"].ToString();

        list.Add(item);
    }

    conn.Close();

    return list;
}
    }
}