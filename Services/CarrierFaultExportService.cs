using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using AlarmSystem.Services.Interfaces;

namespace AlarmSystem.Services
{
   public class CarrierFaultExportService : ICarrierFaultExportService
    {
        private readonly IConfiguration _configuration;

        public CarrierFaultExportService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

       public Task<List<CarrierFaultExportModel>> GetCarrierFaultsAsync()
        {
            List<CarrierFaultExportModel> list = new();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("TMS"));

            string sql = @"
SELECT
    f.*,
    c.*,
    cc.COF
FROM CLARITY_DATA_NW_FAULTS f

LEFT JOIN carrier_cct c
ON f.circuit_name=c.cct

LEFT JOIN
(
    SELECT
        c1.cct,
        c1.COF

    FROM CARRIER_COMMENTS c1

    INNER JOIN
    (
        SELECT
            cct,
            MAX(updated_at) maxdate

        FROM CARRIER_COMMENTS

        GROUP BY cct

    )c2

    ON c1.cct=c2.cct
    AND c1.updated_at=c2.maxdate

)cc

ON f.circuit_name=cc.cct

WHERE

c.CUSR_NAME LIKE '%Dialog%'
OR c.CUSR_NAME LIKE '%Mobitel%'
OR c.CUSR_NAME LIKE '%Hutch%'
OR c.CUSR_NAME LIKE '%Airtel%'

ORDER BY f.Reportedtime DESC
";

            SqlCommand cmd = new SqlCommand(sql, con);

            con.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new CarrierFaultExportModel
                {
                    problem_no = dr["problem_no"].ToString(),
                    cr = dr["cr"].ToString(),
                    customer_name = dr["customer_name"].ToString(),
                    circuit_name = dr["circuit_name"].ToString(),
                    circuit_type = dr["circuit_type"].ToString(),
                    description = dr["description"].ToString(),
                    nature = dr["nature"].ToString(),

                    reportedtime =
                        dr["reportedtime"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(dr["reportedtime"]),

                    priority = dr["priority"].ToString(),
                    SLA_type = dr["SLA_type"].ToString(),
                    SLA_time = dr["SLA_time"].ToString(),
                    workgroup = dr["workgroup"].ToString(),
                    customer_type = dr["customer_type"].ToString(),
                    enter_by = dr["enter_by"].ToString(),
                    problem_status = dr["problem_status"].ToString(),
                    esc_level = dr["esc_level"].ToString(),
                    account_number = dr["account_number"].ToString(),
                    reported_by = dr["reported_by"].ToString(),
                    reported_contact = dr["reported_contact"].ToString(),
                    access_medium = dr["access_medium"].ToString(),
                   CR1 = dr["CR"].ToString(),
CCT = dr["CCT"].ToString(),
LEA = dr["LEA"].ToString(),
CUSR_NAME = dr["CUSR_NAME"].ToString(),
BENDADDRESS = dr["BENDADDRESS"].ToString(),
COF = dr["COF"].ToString()
                });
            }

           return Task.FromResult(list);
        }
    }
}