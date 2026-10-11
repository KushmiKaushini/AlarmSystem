using AlarmSystem.Models;
using AlarmSystem.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using System.IO;
namespace AlarmSystem.Services
{
    public class CarrierBusinessSummaryService
    {
        private readonly IConfiguration _configuration;

        public CarrierBusinessSummaryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("TMS"));
        }

      public DataTable GetSummary()
{
    using SqlConnection con = GetConnection();

    string sql = @"
SELECT
SUM(CASE WHEN c.CUSR_NAME LIKE '%Dialog%' THEN 1 ELSE 0 END) AS Dialog,
SUM(CASE WHEN c.CUSR_NAME LIKE '%Mobitel%' THEN 1 ELSE 0 END) AS Mobitel,
SUM(CASE WHEN c.CUSR_NAME LIKE '%Hutch%' THEN 1 ELSE 0 END) AS Hutch,
SUM(CASE WHEN c.CUSR_NAME LIKE '%Airtel%' THEN 1 ELSE 0 END) AS Airtel
FROM CLARITY_DATA_NW_FAULTS f
LEFT JOIN carrier_cct c
ON f.circuit_name = c.cct";

    SqlDataAdapter da = new SqlDataAdapter(sql, con);

    DataTable dt = new DataTable();

    da.Fill(dt);

    return dt;
}

public List<CarrierBusinessSummaryViewModel> GetFaults()
{
    List<CarrierBusinessSummaryViewModel> list = new();

    using SqlConnection con = GetConnection();

    string sql = @"
SELECT
    f.problem_no,
    f.customer_name,
    f.circuit_name,
    f.reportedtime,
    f.reported_by,
    f.reported_contact,
    f.access_medium,
    c.LEA,
    c.BENDADDRESS,
    cc.COF
FROM CLARITY_DATA_NW_FAULTS f
LEFT JOIN carrier_cct c
    ON f.circuit_name = c.cct
LEFT JOIN
(
    SELECT c1.cct, c1.COF
    FROM CARRIER_COMMENTS c1
    INNER JOIN
    (
        SELECT cct,
               MAX(updated_at) AS maxdate
        FROM CARRIER_COMMENTS
        GROUP BY cct
    ) c2
      ON c1.cct = c2.cct
     AND c1.updated_at = c2.maxdate
) cc
ON f.circuit_name = cc.cct
WHERE
      c.CUSR_NAME LIKE '%Dialog%'
   OR c.CUSR_NAME LIKE '%Mobitel%'
   OR c.CUSR_NAME LIKE '%Hutch%'
   OR c.CUSR_NAME LIKE '%Airtel%'
ORDER BY f.reportedtime DESC";

    SqlCommand cmd = new SqlCommand(sql, con);

    con.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        CarrierBusinessSummaryViewModel item = new();

        item.CustomerRef = dr["problem_no"].ToString();
        item.CustomerName = dr["customer_name"].ToString();
        item.CircuitID = dr["circuit_name"].ToString();
        item.ReportedTime = dr["reportedtime"].ToString();
        item.ReportedBy = dr["reported_by"].ToString();
        item.ReportedContact = dr["reported_contact"].ToString();
        item.AccessMedium = dr["access_medium"].ToString();
        item.LEA = dr["LEA"].ToString();
        item.BendAddress = dr["BENDADDRESS"].ToString();
        item.COF = dr["COF"].ToString();
        item.Remark = "";

        list.Add(item);
    }

    con.Close();

    return list;
}
public FileContentResult ExportToExcel(List<CarrierBusinessSummaryViewModel> data)
{
    using var workbook = new XLWorkbook();

    var ws = workbook.Worksheets.Add("Carrier Business Summary");

    ws.Cell(1, 1).Value = "Customer Ref";
    ws.Cell(1, 2).Value = "Customer Name";
    ws.Cell(1, 3).Value = "Circuit ID";
    ws.Cell(1, 4).Value = "Reported Time";
    ws.Cell(1, 5).Value = "Reported By";
    ws.Cell(1, 6).Value = "Reported Contact";
    ws.Cell(1, 7).Value = "Access Medium";
    ws.Cell(1, 8).Value = "LEA";
    ws.Cell(1, 9).Value = "Bend Address";
    ws.Cell(1, 10).Value = "COF";

    int row = 2;

    foreach (var item in data)
    {
        ws.Cell(row, 1).Value = item.CustomerRef;
        ws.Cell(row, 2).Value = item.CustomerName;
        ws.Cell(row, 3).Value = item.CircuitID;
        ws.Cell(row, 4).Value = item.ReportedTime;
        ws.Cell(row, 5).Value = item.ReportedBy;
        ws.Cell(row, 6).Value = item.ReportedContact;
        ws.Cell(row, 7).Value = item.AccessMedium;
        ws.Cell(row, 8).Value = item.LEA;
        ws.Cell(row, 9).Value = item.BendAddress;
        ws.Cell(row, 10).Value = item.COF;

        row++;
    }

    using var stream = new MemoryStream();

    workbook.SaveAs(stream);

    return new FileContentResult(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
    {
        FileDownloadName = "CarrierBusinessSummary.xlsx"
    };
}
    }
}