using AlarmSystem.Models;
using Microsoft.Data.SqlClient;

namespace AlarmSystem.Services
{
    public class SummaryCarrierFaultService
    {
        private readonly IConfiguration _configuration;

        public SummaryCarrierFaultService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //=========================================================
        // TOP SUMMARY
        //=========================================================

        public CarrierSummaryModel GetSummary()
        {
            CarrierSummaryModel model = new();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("TMS"));

            string sql = @"

SELECT

SUM(CASE WHEN PLATFORM='CEA' THEN 1 ELSE 0 END) AS CEA,
SUM(CASE WHEN PLATFORM='MSAN' THEN 1 ELSE 0 END) AS MSAN,
SUM(CASE WHEN PLATFORM='SLBN' THEN 1 ELSE 0 END) AS SLBN

FROM CLARITY_WO_STATUS

WHERE WO_STATUS <> 'CLOSED';

";

            SqlCommand cmd = new SqlCommand(sql, con);

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.CEA = Convert.ToInt32(dr["CEA"]);
                model.MSAN = Convert.ToInt32(dr["MSAN"]);
                model.SLBN = Convert.ToInt32(dr["SLBN"]);
            }

            return model;
        }

        //=========================================================
        // DETAIL GRID
        //=========================================================

        public List<SummaryCarrierFaultModel> GetDetails()
        {
            List<SummaryCarrierFaultModel> list = new();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("TMS"));

            string sql = @"

            WITH BaseData AS
(
    SELECT
        REGION,
        PROVINCE,
        PLATFORM,

        CASE
            WHEN DATEDIFF(HOUR, STARTED_TIME, GETDATE()) < 24 THEN 'LT1D'
            WHEN DATEDIFF(HOUR, STARTED_TIME, GETDATE()) < 72 THEN '1D_3D'
            WHEN DATEDIFF(HOUR, STARTED_TIME, GETDATE()) < 168 THEN '3D_7D'
            ELSE 'GT7D'
        END AS AGE_GROUP

    FROM CLARITY_WO_STATUS

    WHERE
        WO_STATUS <> 'CLOSED'
        AND PLATFORM IN ('CEA','MSAN','SLBN')
),

FinalData AS
(
    SELECT
        REGION,
        PROVINCE,
        PLATFORM + '_' + AGE_GROUP AS COLNAME
    FROM BaseData
)

SELECT

REGION,
PROVINCE,

ISNULL([CEA_LT1D],0) AS CEA_LT1D,
ISNULL([CEA_1D_3D],0) AS CEA_1D_3D,
ISNULL([CEA_3D_7D],0) AS CEA_3D_7D,
ISNULL([CEA_GT7D],0) AS CEA_GT7D,

ISNULL([MSAN_LT1D],0) AS MSAN_LT1D,
ISNULL([MSAN_1D_3D],0) AS MSAN_1D_3D,
ISNULL([MSAN_3D_7D],0) AS MSAN_3D_7D,
ISNULL([MSAN_GT7D],0) AS MSAN_GT7D,

ISNULL([SLBN_LT1D],0) AS SLBN_LT1D,
ISNULL([SLBN_1D_3D],0) AS SLBN_1D_3D,
ISNULL([SLBN_3D_7D],0) AS SLBN_3D_7D,
ISNULL([SLBN_GT7D],0) AS SLBN_GT7D

FROM FinalData

PIVOT
(
COUNT(COLNAME)

FOR COLNAME IN
(
[CEA_LT1D],
[CEA_1D_3D],
[CEA_3D_7D],
[CEA_GT7D],

[MSAN_LT1D],
[MSAN_1D_3D],
[MSAN_3D_7D],
[MSAN_GT7D],

[SLBN_LT1D],
[SLBN_1D_3D],
[SLBN_3D_7D],
[SLBN_GT7D]
)

) P

ORDER BY REGION, PROVINCE;

";

            SqlCommand cmd = new SqlCommand(sql, con);

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new SummaryCarrierFaultModel
                {
                    REGION = dr["REGION"]?.ToString() ?? "",
                    PROVINCE = dr["PROVINCE"]?.ToString() ?? "",

                    CEA_LT1D = Convert.ToInt32(dr["CEA_LT1D"]),
                    CEA_1D_3D = Convert.ToInt32(dr["CEA_1D_3D"]),
                    CEA_3D_7D = Convert.ToInt32(dr["CEA_3D_7D"]),
                    CEA_GT7D = Convert.ToInt32(dr["CEA_GT7D"]),

                    MSAN_LT1D = Convert.ToInt32(dr["MSAN_LT1D"]),
                    MSAN_1D_3D = Convert.ToInt32(dr["MSAN_1D_3D"]),
                    MSAN_3D_7D = Convert.ToInt32(dr["MSAN_3D_7D"]),
                    MSAN_GT7D = Convert.ToInt32(dr["MSAN_GT7D"]),

                    SLBN_LT1D = Convert.ToInt32(dr["SLBN_LT1D"]),
                    SLBN_1D_3D = Convert.ToInt32(dr["SLBN_1D_3D"]),
                    SLBN_3D_7D = Convert.ToInt32(dr["SLBN_3D_7D"]),
                    SLBN_GT7D = Convert.ToInt32(dr["SLBN_GT7D"])
                });
            }

            return list;
        }
    }
}