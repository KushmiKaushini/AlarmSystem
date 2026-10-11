using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using System.IO;
namespace AlarmSystem.Services
{
    public class FaultDocketService
    {
        private readonly IConfiguration _configuration;

        public FaultDocketService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("TMS"));
        }

        //==================================================
        // GET FAULT DOCKETS
        //==================================================

        public List<FaultDocket> GetFaultDockets(
            string customerType = "~ALL~",
            string workGroup = "~ALL~")
        {
            List<FaultDocket> list = new();

            using SqlConnection conn = GetConnection();

            SqlCommand cmd = new SqlCommand(
                "REPORT_CLARITY_FAULTS", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@cus_type", customerType);
            cmd.Parameters.AddWithValue("@workgroup", workGroup);

            conn.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                FaultDocket item = new FaultDocket();

                item.CLARITY_REF = dr["problem_no"].ToString();
                item.CUS_TYPE = dr["CUS-TYPE"].ToString();
                item.CUS_NAME = dr["CUS-NAME"].ToString();
                item.CIR_NAME = dr["CIR-NAME"].ToString();
                item.CIR_TYPE = dr["CIR-TYPE"].ToString();

                if (dr["reportedtime"] != DBNull.Value)
                    item.REPORTED = Convert.ToDateTime(dr["reportedtime"]);

                if (dr["HOURS"] != DBNull.Value)
                    item.HOURS = Convert.ToInt32(dr["HOURS"]);

                item.SLA = dr["SLA"].ToString();
                item.ACCESS_MEDIUM = dr["ACCESS_MEDIUM"].ToString();

                if (dr["WO_COUNT"] != DBNull.Value)
                    item.WO_COUNT = Convert.ToInt32(dr["WO_COUNT"]);

                item.WORK_GROUP = dr["WORK_GROUP"].ToString();

                item.SLAExceeded =
                    CheckSLA(item.SLA, item.HOURS);

                list.Add(item);
            }

            conn.Close();

            return list;
        }

        //==================================================
        // CUSTOMER TYPES
        //==================================================

        public List<string> GetCustomerTypes()
        {
            List<string> list = new();

            using SqlConnection conn = GetConnection();

            SqlCommand cmd = new SqlCommand(@"
                select '~ALL~' as [CUSTOMER TYPE]

                union

                select distinct(customer_type)

                from CLARITY_DATA_NW_FAULTS

                order by [CUSTOMER TYPE]", conn);

            conn.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(dr[0].ToString());
            }

            conn.Close();

            return list;
        }

        //==================================================
        // SLA
        //==================================================

        private bool CheckSLA(string sla, int hours)
        {
            if (string.IsNullOrWhiteSpace(sla))
                return false;

            if (sla.Contains("99.90%"))
                sla = "2";

            if (sla.Contains("STANDARD") ||
                sla.Contains("24 x 7"))
            {
                sla = sla.Replace("24 x 7", "");
                sla = sla.Replace("STANDARD", "");
                sla = sla.Replace("NONE", "");
                sla = Regex.Replace(sla, "[A-Za-z ]", "");
                sla = sla.Replace("(", "");
                sla = sla.Replace(")", "");
                sla = sla.Replace("/", "");
                sla = sla.Trim();

                if (double.TryParse(sla, out double limit))
                {
                    return hours >= limit;
                }
            }

            return false;
        }
        public List<FaultDocket> GetExportData()
{
    return GetFaultDockets("~ALL~", "~ALL~");
}

public FileContentResult ExportToExcel(List<FaultDocket> data)
{
    using var workbook = new XLWorkbook();

    var ws = workbook.Worksheets.Add("Clarity Fault Dockets");

    ws.Cell(1, 1).Value = "CLARITY REF";
    ws.Cell(1, 2).Value = "CUS TYPE";
    ws.Cell(1, 3).Value = "CUS NAME";
    ws.Cell(1, 4).Value = "CIR NAME";
    ws.Cell(1, 5).Value = "CIR TYPE";
    ws.Cell(1, 6).Value = "REPORTED";
    ws.Cell(1, 7).Value = "HOURS";
    ws.Cell(1, 8).Value = "SLA";
    ws.Cell(1, 9).Value = "ACCESS MEDIUM";
    ws.Cell(1,10).Value = "PENDING WO";
    ws.Cell(1,11).Value = "WORK GROUP";

    int row = 2;

    foreach (var item in data)
    {
        ws.Cell(row, 1).Value = item.CLARITY_REF;
        ws.Cell(row, 2).Value = item.CUS_TYPE;
        ws.Cell(row, 3).Value = item.CUS_NAME;
        ws.Cell(row, 4).Value = item.CIR_NAME;
        ws.Cell(row, 5).Value = item.CIR_TYPE;
       ws.Cell(row, 6).Value =
    item.REPORTED.HasValue
        ? item.REPORTED.Value.ToString("dd/MM/yyyy hh:mm tt")
        : "";
        ws.Cell(row, 7).Value = item.HOURS;
        ws.Cell(row, 8).Value = item.SLA;
        ws.Cell(row, 9).Value = item.ACCESS_MEDIUM;
        ws.Cell(row,10).Value = item.WO_COUNT;
        ws.Cell(row,11).Value = item.WORK_GROUP;

        row++;
    }

    ws.Columns().AdjustToContents();

    using var stream = new MemoryStream();

    workbook.SaveAs(stream);

    return new FileContentResult(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
    {
        FileDownloadName = "ClarityFaultDockets.xlsx"
    };
}

public ClarityFaultDetail GetFaultDetails(int clarityRef)
{
    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(
        @"SELECT *
          FROM CLARITY_DATA_NW_FAULTS
          WHERE problem_no=@id", conn);

    cmd.Parameters.AddWithValue("@id", clarityRef);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    ClarityFaultDetail model = new();

    if (dr.Read())
    {
        model.CLARITY_REF = Convert.ToInt32(dr["problem_no"]);

        model.CUSTOMER = dr["customer_name"]?.ToString() ?? "";

        model.CIRCUIT = dr["circuit_name"]?.ToString() ?? "";

        model.CIRCUIT_TYPE = dr["circuit_type"]?.ToString() ?? "";

        model.DESCRIPTION = dr["description"]?.ToString() ?? "";

        model.NATURE = dr["nature"]?.ToString() ?? "";

        if (dr["reportedtime"] != DBNull.Value)
            model.REPORTED_TIME =
                Convert.ToDateTime(dr["reportedtime"]);

        model.PRIORITY = dr["priority"]?.ToString() ?? "";

        model.SLA_TYPE = dr["SLA_type"]?.ToString() ?? "";

        model.SLA_TIME = dr["SLA_time"]?.ToString() ?? "";

        model.WORKGROUP = dr["workgroup"]?.ToString() ?? "";

        model.CUSTOMER_TYPE =
            dr["customer_type"]?.ToString() ?? "";

        model.ENTER_BY =
            dr["enter_by"]?.ToString() ?? "";

        model.STATUS =
            dr["problem_status"]?.ToString() ?? "";

        model.REPORTED_BY =
            dr["reported_by"]?.ToString() ?? "";

        model.REPORTED_CONTACT =
            dr["reported_contact"]?.ToString() ?? "";
    }

    return model;
}
public List<ClarityWorkOrder> GetWorkOrders(int clarityRef)
{
    List<ClarityWorkOrder> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(@"
        SELECT *
        FROM CLARITY_WO_STATUS
        WHERE CLARITY_REF=@id", conn);

    cmd.Parameters.AddWithValue("@id", clarityRef);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new ClarityWorkOrder
        {
            WO_REF = dr["WO_REF"]?.ToString() ?? "",

            WO_STATUS = dr["WO_STATUS"]?.ToString() ?? "",

            STARTED_TIME =
                dr["STARTED_TIME"] == DBNull.Value
                ? null
                : Convert.ToDateTime(dr["STARTED_TIME"]),

            WORK_GROUP =
                dr["WORK_GROUP"]?.ToString() ?? ""
        });
    }

    return list;
}
public List<ClarityComment> GetComments(int clarityRef)
{
    List<ClarityComment> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(@"
        SELECT *
        FROM CLARITY_COMMENTS
        WHERE docket_id=@id
        ORDER BY event_time", conn);

    cmd.Parameters.AddWithValue("@id", clarityRef);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new ClarityComment
        {
            TEXT =
                dr["description"]?.ToString() ?? "",

            EVENT_TIME =
                dr["event_time"] == DBNull.Value
                ? null
                : Convert.ToDateTime(dr["event_time"])
        });
    }

    return list;
}
    }
}