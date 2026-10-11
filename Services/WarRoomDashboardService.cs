using AlarmSystem.Services.Interfaces;
using AlarmSystem.Models;
using AlarmSystem.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class WarRoomDashboardService : IWarRoomService
    {
        private readonly SqlConnection _connection;

        public WarRoomDashboardService(SqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<WarRoomDashboardViewModel> GetDashboardAsync()
        {
            var vm = new WarRoomDashboardViewModel();

            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();

            // ==========================================================
            // NETWORK FAULT SUMMARY
            // ==========================================================

            string disasterQuery = @"
                SELECT *
                FROM disaster
            ";

            using (SqlCommand cmd = new SqlCommand(disasterQuery, _connection))
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.DisasterSummary.Add(new PlatformSummary
                    {
                        Platform = reader["Platform"]?.ToString() ?? "",

                        F2 = reader["F2"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["F2"]),

                        F3 = reader["F3"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["F3"]),

                        F4 = reader["F4"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["F4"]),

                        F5 = reader["F5"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["F5"])
                    });
                }
            }

            // ==========================================================
            // REGION SUMMARY
            // ==========================================================

                        string regionQuery = @"
                SELECT
                    REGION,
                    PROVINCE,
                    SUM(CASE WHEN PLATFORM='CEA' THEN 1 ELSE 0 END) AS CEA,
                    SUM(CASE WHEN PLATFORM='MSAN' THEN 1 ELSE 0 END) AS MSAN,
                    SUM(CASE WHEN PLATFORM='SLBN' THEN 1 ELSE 0 END) AS SLBN
                FROM FAULTS
                WHERE ALARM_TYPE='Node Down'
                    AND PLATFORM IN ('CEA','MSAN','SLBN')
                    AND sourceip NOT LIKE '77%'
                    AND sourceip NOT LIKE '58%'
                    AND FAULT_TIME>'2025-11-25'
                    AND FAULT_STATUS='OPEN'
                    AND NW_ENG!='DEFAULT'
                GROUP BY REGION, PROVINCE

                UNION ALL

                SELECT
                    'TOTAL' AS REGION,
                    '' AS PROVINCE,
                    SUM(CASE WHEN PLATFORM='CEA' THEN 1 ELSE 0 END),
                    SUM(CASE WHEN PLATFORM='MSAN' THEN 1 ELSE 0 END),
                    SUM(CASE WHEN PLATFORM='SLBN' THEN 1 ELSE 0 END)
                FROM FAULTS
                WHERE ALARM_TYPE='Node Down'
                    AND PLATFORM IN ('CEA','MSAN','SLBN')
                    AND sourceip NOT LIKE '77%'
                    AND sourceip NOT LIKE '58%'
                    AND FAULT_TIME>'2025-11-25'
                    AND FAULT_STATUS='OPEN'
                    AND NW_ENG!='DEFAULT'

                ORDER BY REGION, PROVINCE;
            ";

            using (SqlCommand cmd = new SqlCommand(regionQuery, _connection))
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.RegionSummary.Add(new RegionSummary
                    {
                        REGION = reader["REGION"]?.ToString() ?? "",
                        PROVINCE = reader["PROVINCE"]?.ToString() ?? "",

                        CEA = reader["CEA"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["CEA"]),

                        MSAN = reader["MSAN"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["MSAN"]),

                        SLBN = reader["SLBN"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["SLBN"])
                    });
                }
            }

            // ==========================================================
            // OTHER OPERATOR FAULT SUMMARY
            // ==========================================================3 A

                        string operatorQuery = @"
WITH fault_counts AS
(
    SELECT
        CASE
            WHEN c.CUSR_NAME LIKE '%Dialog%' THEN 'Dialog'
            WHEN c.CUSR_NAME LIKE '%Mobitel%' THEN 'Mobitel'
            WHEN c.CUSR_NAME LIKE '%Hutch%' THEN 'Hutch'
            WHEN c.CUSR_NAME LIKE '%Airtel%' THEN 'Airtel'
        END AS customer_group,

        COUNT(*) AS total_count

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
    ISNULL([Dialog],0)  AS Dialog,
    ISNULL([Mobitel],0) AS Mobitel,
    ISNULL([Hutch],0)   AS Hutch,
    ISNULL([Airtel],0)  AS Airtel

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
) AS p;
";

            using (SqlCommand cmd = new SqlCommand(operatorQuery, _connection))
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.OperatorSummary.Add(new OperatorSummary
                    {
                        Dialog = reader["Dialog"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["Dialog"]),

                        Mobitel = reader["Mobitel"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["Mobitel"]),

                        Hutch = reader["Hutch"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["Hutch"]),

                        Airtel = reader["Airtel"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["Airtel"])
                    });
                }
            }

            // ==========================================================
            // OPERATOR REGION SUMMARY
            // ==========================================================

            string operatorRegionQuery = @"
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
            WHEN DATEDIFF(HOUR, STARTED_TIME, GETDATE()) < 24 THEN 'LT1D'
            WHEN DATEDIFF(HOUR, STARTED_TIME, GETDATE()) >= 24
                 AND DATEDIFF(HOUR, STARTED_TIME, GETDATE()) < 72 THEN '1D_3D'
            WHEN DATEDIFF(HOUR, STARTED_TIME, GETDATE()) >= 72
                 AND DATEDIFF(HOUR, STARTED_TIME, GETDATE()) < 168 THEN '3D_7D'
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

    ISNULL([Dialog_LT1D],0) AS Dialog_LT1D,
    ISNULL([Dialog_1D_3D],0) AS Dialog_1D_3D,
    ISNULL([Dialog_3D_7D],0) AS Dialog_3D_7D,
    ISNULL([Dialog_GT7D],0) AS Dialog_GT7D,

    ISNULL([Mobitel_LT1D],0) AS Mobitel_LT1D,
    ISNULL([Mobitel_1D_3D],0) AS Mobitel_1D_3D,
    ISNULL([Mobitel_3D_7D],0) AS Mobitel_3D_7D,
    ISNULL([Mobitel_GT7D],0) AS Mobitel_GT7D,

    ISNULL([Airtel_LT1D],0) AS Airtel_LT1D,
    ISNULL([Airtel_1D_3D],0) AS Airtel_1D_3D,
    ISNULL([Airtel_3D_7D],0) AS Airtel_3D_7D,
    ISNULL([Airtel_GT7D],0) AS Airtel_GT7D,

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

            using (SqlCommand cmd = new SqlCommand(operatorRegionQuery, _connection))
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.OperatorRegionSummary.Add(new OperatorRegionSummary
                    {
                        REGION = reader["REGION"]?.ToString() ?? "",
                        PROVINCE = reader["PROVINCE"]?.ToString() ?? "",

                        Dialog_LT1D = reader["Dialog_LT1D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Dialog_LT1D"]),
                        Dialog_1D_3D = reader["Dialog_1D_3D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Dialog_1D_3D"]),
                        Dialog_3D_7D = reader["Dialog_3D_7D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Dialog_3D_7D"]),
                        Dialog_GT7D = reader["Dialog_GT7D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Dialog_GT7D"]),

                        Mobitel_LT1D = reader["Mobitel_LT1D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Mobitel_LT1D"]),
                        Mobitel_1D_3D = reader["Mobitel_1D_3D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Mobitel_1D_3D"]),
                        Mobitel_3D_7D = reader["Mobitel_3D_7D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Mobitel_3D_7D"]),
                        Mobitel_GT7D = reader["Mobitel_GT7D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Mobitel_GT7D"]),

                        Airtel_LT1D = reader["Airtel_LT1D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Airtel_LT1D"]),
                        Airtel_1D_3D = reader["Airtel_1D_3D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Airtel_1D_3D"]),
                        Airtel_3D_7D = reader["Airtel_3D_7D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Airtel_3D_7D"]),
                        Airtel_GT7D = reader["Airtel_GT7D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Airtel_GT7D"]),

                        Hutch_LT1D = reader["Hutch_LT1D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Hutch_LT1D"]),
                        Hutch_1D_3D = reader["Hutch_1D_3D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Hutch_1D_3D"]),
                        Hutch_3D_7D = reader["Hutch_3D_7D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Hutch_3D_7D"]),
                        Hutch_GT7D = reader["Hutch_GT7D"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Hutch_GT7D"])
                    });
                }
            }

            if (_connection.State == ConnectionState.Open)
                await _connection.CloseAsync();

            return vm;
        }
    }
}