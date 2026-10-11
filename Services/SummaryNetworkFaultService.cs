using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using ClosedXML.Excel;
using System.IO;

namespace AlarmSystem.Services
{
    public class SummaryNetworkFaultService
    {
        private readonly IConfiguration _configuration;

        public SummaryNetworkFaultService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //=========================================================
        // TOP SUMMARY
        //=========================================================

        public NetworkSummaryModel GetSummary()
        {
            NetworkSummaryModel model = new();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("TMS"));

            string sql = @"

WITH fault_counts AS
(
    SELECT

        CASE

            WHEN c.CUSR_NAME LIKE '%Dialog%' THEN 'Dialog'
            WHEN c.CUSR_NAME LIKE '%Mobitel%' THEN 'Mobitel'
            WHEN c.CUSR_NAME LIKE '%Hutch%' THEN 'Hutch'
            WHEN c.CUSR_NAME LIKE '%Airtel%' THEN 'Airtel'

        END customer_group,

        COUNT(*) total_count

    FROM CLARITY_DATA_NW_FAULTS f

    LEFT JOIN carrier_cct c
        ON f.circuit_name = c.cct

    WHERE

        c.CUSR_NAME LIKE '%Dialog%'
        OR c.CUSR_NAME LIKE '%Mobitel%'
        OR c.CUSR_NAME LIKE '%Hutch%'
        OR c.CUSR_NAME LIKE '%Airtel%'

    GROUP BY

        CASE

            WHEN c.CUSR_NAME LIKE '%Dialog%' THEN 'Dialog'
            WHEN c.CUSR_NAME LIKE '%Mobitel%' THEN 'Mobitel'
            WHEN c.CUSR_NAME LIKE '%Hutch%' THEN 'Hutch'
            WHEN c.CUSR_NAME LIKE '%Airtel%' THEN 'Airtel'

        END
)

SELECT

ISNULL([Dialog],0) AS Dialog,
ISNULL([Mobitel],0) AS Mobitel,
ISNULL([Hutch],0) AS Hutch,
ISNULL([Airtel],0) AS Airtel

FROM fault_counts

PIVOT
(
    MAX(total_count)

    FOR customer_group IN
    (
        [Dialog],
        [Mobitel],
        [Hutch],
        [Airtel]
    )

) p;
";

            SqlCommand cmd = new SqlCommand(sql, con);

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                model.Dialog = Convert.ToInt32(dr["Dialog"]);
                model.Mobitel = Convert.ToInt32(dr["Mobitel"]);
                model.Hutch = Convert.ToInt32(dr["Hutch"]);
                model.Airtel = Convert.ToInt32(dr["Airtel"]);
            }

            return model;
        }

        //=========================================================
        // DETAIL GRID
        //=========================================================

        public List<SummaryNetworkFaultModel> GetDetails()
        {
            List<SummaryNetworkFaultModel> list = new();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("TMS"));

            string sql = @"

            WITH BaseData AS
(
    SELECT

        W.REGION,
        W.PROVINCE,

        CASE
            WHEN customer_name LIKE '%Dialog%' THEN 'Dialog'
            WHEN customer_name LIKE '%Mobitel%' THEN 'Mobitel'
            WHEN customer_name LIKE '%Hutch%' THEN 'Hutch'
            WHEN customer_name LIKE '%Airtel%' THEN 'Airtel'
            ELSE 'Other'
        END AS CUSTOMER_GROUP,

        CASE
            WHEN DATEDIFF(HOUR, STARTED_TIME, GETDATE()) < 24
                THEN 'LT1D'

            WHEN DATEDIFF(HOUR, STARTED_TIME, GETDATE()) >= 24
             AND DATEDIFF(HOUR, STARTED_TIME, GETDATE()) < 72
                THEN '1D_3D'

            WHEN DATEDIFF(HOUR, STARTED_TIME, GETDATE()) >= 72
             AND DATEDIFF(HOUR, STARTED_TIME, GETDATE()) < 168
                THEN '3D_7D'

            ELSE 'GT7D'

        END AS TIME_CATEGORY

    FROM CLARITY_DATA_NW_FAULTS D

    LEFT JOIN CLARITY_WO_STATUS W
        ON D.problem_no = W.CLARITY_REF

    WHERE

        W.WO_STATUS <> 'CLOSED'
        AND W.AREA = 'Region'

        AND
        (
            customer_name LIKE '%Dialog%'
            OR customer_name LIKE '%Mobitel%'
            OR customer_name LIKE '%Hutch%'
            OR customer_name LIKE '%Airtel%'
        )

),

Combined AS
(
    SELECT

        REGION,
        PROVINCE,

        CUSTOMER_GROUP + '_' + TIME_CATEGORY AS GroupTime

    FROM BaseData
)

SELECT

    REGION,
    PROVINCE,

    -- Dialog
    ISNULL([Dialog_LT1D],0) AS Dialog_LT1D,
    ISNULL([Dialog_1D_3D],0) AS Dialog_1D_3D,
    ISNULL([Dialog_3D_7D],0) AS Dialog_3D_7D,
    ISNULL([Dialog_GT7D],0) AS Dialog_GT7D,

    -- Mobitel
    ISNULL([Mobitel_LT1D],0) AS Mobitel_LT1D,
    ISNULL([Mobitel_1D_3D],0) AS Mobitel_1D_3D,
    ISNULL([Mobitel_3D_7D],0) AS Mobitel_3D_7D,
    ISNULL([Mobitel_GT7D],0) AS Mobitel_GT7D,

    -- Airtel
    ISNULL([Airtel_LT1D],0) AS Airtel_LT1D,
    ISNULL([Airtel_1D_3D],0) AS Airtel_1D_3D,
    ISNULL([Airtel_3D_7D],0) AS Airtel_3D_7D,
    ISNULL([Airtel_GT7D],0) AS Airtel_GT7D,

    -- Hutch
    ISNULL([Hutch_LT1D],0) AS Hutch_LT1D,
    ISNULL([Hutch_1D_3D],0) AS Hutch_1D_3D,
    ISNULL([Hutch_3D_7D],0) AS Hutch_3D_7D,
    ISNULL([Hutch_GT7D],0) AS Hutch_GT7D

FROM Combined

PIVOT
(
    COUNT(GroupTime)

    FOR GroupTime IN
    (
        [Dialog_LT1D],
        [Dialog_1D_3D],
        [Dialog_3D_7D],
        [Dialog_GT7D],

        [Mobitel_LT1D],
        [Mobitel_1D_3D],
        [Mobitel_3D_7D],
        [Mobitel_GT7D],

        [Airtel_LT1D],
        [Airtel_1D_3D],
        [Airtel_3D_7D],
        [Airtel_GT7D],

        [Hutch_LT1D],
        [Hutch_1D_3D],
        [Hutch_3D_7D],
        [Hutch_GT7D]
    )

) AS pv

ORDER BY REGION, PROVINCE;

";

            SqlCommand cmd = new SqlCommand(sql, con);

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new SummaryNetworkFaultModel
                {
                    REGION = dr["REGION"]?.ToString() ?? "",
                    PROVINCE = dr["PROVINCE"]?.ToString() ?? "",

                    Dialog_LT1D = Convert.ToInt32(dr["Dialog_LT1D"]),
                    Dialog_1D_3D = Convert.ToInt32(dr["Dialog_1D_3D"]),
                    Dialog_3D_7D = Convert.ToInt32(dr["Dialog_3D_7D"]),
                    Dialog_GT7D = Convert.ToInt32(dr["Dialog_GT7D"]),

                    Mobitel_LT1D = Convert.ToInt32(dr["Mobitel_LT1D"]),
                    Mobitel_1D_3D = Convert.ToInt32(dr["Mobitel_1D_3D"]),
                    Mobitel_3D_7D = Convert.ToInt32(dr["Mobitel_3D_7D"]),
                    Mobitel_GT7D = Convert.ToInt32(dr["Mobitel_GT7D"]),

                    Airtel_LT1D = Convert.ToInt32(dr["Airtel_LT1D"]),
                    Airtel_1D_3D = Convert.ToInt32(dr["Airtel_1D_3D"]),
                    Airtel_3D_7D = Convert.ToInt32(dr["Airtel_3D_7D"]),
                    Airtel_GT7D = Convert.ToInt32(dr["Airtel_GT7D"]),

                    Hutch_LT1D = Convert.ToInt32(dr["Hutch_LT1D"]),
                    Hutch_1D_3D = Convert.ToInt32(dr["Hutch_1D_3D"]),
                    Hutch_3D_7D = Convert.ToInt32(dr["Hutch_3D_7D"]),
                    Hutch_GT7D = Convert.ToInt32(dr["Hutch_GT7D"])
                });
            }

            return list;
        }
    }
}