using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services;

public class PowerAcService
{
    private readonly SqlConnection _connection;

    public PowerAcService(SqlConnection connection)
    {
        _connection = connection;
    }

    public List<PowerAcModel> GetAlarms()
    {
        List<PowerAcModel> list = new();

        using SqlCommand cmd = new("VIEW_P_AC_2", _connection);
        cmd.CommandType = CommandType.StoredProcedure;

        _connection.Open();

        using SqlDataReader dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            list.Add(new PowerAcModel
            {
                ID = Convert.ToInt32(dr["ID"]),
                NODE = dr["NODE"].ToString() ?? "",
                HOURS = Convert.ToInt32(dr["HOURS"]),
                FAULT = dr["FAULT"].ToString() ?? "",
                NW_ENG = dr["NW_ENG"].ToString() ?? "",
                ACK_STATUS = dr["ACK_STATUS"].ToString() ?? "",
                ACK_BY = dr["ACK_BY"].ToString() ?? ""
            });
        }

        _connection.Close();

        return list;
    }
}