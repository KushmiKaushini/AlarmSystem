-- ============================================================
-- SLT NOC – Daily Email Report SQL Queries (Verified against TMS DB)
-- Database : TMS (SQL Server)
-- Updated  : 2026-10-02
-- ============================================================

-- ────────────────────────────────────────────────────────────
-- TYPE 1 : Pending Data Faults – Regional Network Teams
-- Source  : dbo.CLARITY_WO_STATUS joined with dbo.CLARITY_DATA_NW_FAULTS
-- Filter  : Active OPEN / PENDING work orders as of execution time (relative to GETDATE())
SELECT TOP 100
    ROW_NUMBER() OVER(ORDER BY ISNULL(w.STARTED_TIME, f.reportedtime) DESC) AS No,
    ISNULL(NULLIF(f.circuit_name, ''), ISNULL(NULLIF(w.CIRCUIT, ''), 'N/A')) AS DataCircuitId,
    ISNULL(NULLIF(f.customer_name, ''), ISNULL(NULLIF(w.CUSTOMER, ''), 'N/A')) AS DetailsOfCustomer,
    CONVERT(varchar(12), ISNULL(w.STARTED_TIME, f.reportedtime), 101) AS FaultDate,
    CONVERT(varchar(8),  ISNULL(w.STARTED_TIME, f.reportedtime), 108) AS FaultTime,
    ISNULL(NULLIF(w.WORK_GROUP, ''), ISNULL(NULLIF(f.workgroup, ''), 'N/A')) AS WorkOrdersHeldUpAt,
    ISNULL(NULLIF(f.description, ''), ISNULL(NULLIF(w.CIRCUIT_TYPE, ''), 'N/A')) AS Description,
    CASE
        WHEN ISNULL(w.STARTED_TIME, f.reportedtime) IS NULL THEN '00:00:00'
        WHEN DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) < 0 THEN '00:00:00'
        ELSE
            CAST(DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) / 3600 AS VARCHAR) + ':' +
            RIGHT('00' + CAST((DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) % 3600) / 60 AS VARCHAR), 2) + ':' +
            RIGHT('00' + CAST((DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) % 60 AS VARCHAR), 2)
    END AS DurationOfComplaint,
    ISNULL(NULLIF(w.REGION, ''), 'N/A') AS Region,
    ISNULL(NULLIF(w.PROVINCE, ''), 'N/A') AS Province
FROM dbo.CLARITY_WO_STATUS w
LEFT JOIN dbo.CLARITY_DATA_NW_FAULTS f ON f.problem_no = w.CLARITY_REF
WHERE (
    w.WO_STATUS IN ('OPEN', 'PENDING', 'IN PROGRESS', 'INPROGRESS', 'ASSIGNED', 'ACKNOWLEDGED', 'Acknowledged', 'Open', 'Pending', 'In Progress')
    OR (UPPER(ISNULL(w.WO_STATUS, '')) NOT IN ('CLOSED', 'RESOLVED', 'CANCELLED') AND ISNULL(w.WO_STATUS, '') != '')
    OR w.WO_STATUS IS NULL
)
AND (
    f.problem_status IS NULL
    OR UPPER(f.problem_status) NOT IN ('CLOSED', 'RESOLVED', 'CANCELLED')
)
AND (@TargetDate IS NULL OR CAST(ISNULL(w.STARTED_TIME, f.reportedtime) AS DATE) <= CAST(@TargetDate AS DATE))
ORDER BY ISNULL(w.STARTED_TIME, f.reportedtime) DESC;


-- ────────────────────────────────────────────────────────────
-- TYPE 2 : Pending OPMC Faults
-- Source  : dbo.CLARITY_WO_STATUS joined with dbo.CLARITY_DATA_NW_FAULTS,
--           dbo.LEA_MAPPING and dbo.CLARITY_COMMENTS
SELECT TOP 100
    ROW_NUMBER() OVER(ORDER BY ISNULL(w.STARTED_TIME, f.reportedtime) DESC) AS No,
    CONVERT(varchar(20), ISNULL(w.STARTED_TIME, f.reportedtime), 120) AS ReportedDate,
    ISNULL(NULLIF(f.circuit_name, ''), ISNULL(NULLIF(w.CIRCUIT, ''), 'N/A')) AS CircuitName,
    ISNULL(NULLIF(f.customer_name, ''), ISNULL(NULLIF(w.CUSTOMER, ''), 'N/A')) AS CustomerName,
    CASE
        WHEN ISNULL(w.STARTED_TIME, f.reportedtime) IS NULL THEN '00:00'
        WHEN DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) < 0 THEN '00:00'
        ELSE
            CAST(DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) / 3600 AS VARCHAR) + ':' +
            RIGHT('00' + CAST((DATEDIFF(SECOND, ISNULL(w.STARTED_TIME, f.reportedtime), GETDATE()) % 3600) / 60 AS VARCHAR), 2)
    END AS OutageTime,
    ISNULL(NULLIF(lm.LEA, ''), ISNULL(NULLIF(w.AREA, ''), 'N/A')) AS Lea,
    ISNULL(NULLIF(lm.OPMC, ''), 'N/A') AS RelevantOpmc,
    ISNULL(NULLIF(w.OPMC_NW, ''), 'N/A') AS Dgm,
    ISNULL(NULLIF(w.REGION, ''), 'N/A') AS Gm,
    ISNULL((SELECT TOP 1 cc.description
            FROM dbo.CLARITY_COMMENTS cc
            WHERE cc.docket_id = CAST(w.CLARITY_REF AS VARCHAR)
            ORDER BY cc.event_time DESC), 'No update') AS RecentUpdate
FROM dbo.CLARITY_WO_STATUS w
LEFT JOIN dbo.CLARITY_DATA_NW_FAULTS f ON f.problem_no = w.CLARITY_REF
LEFT JOIN dbo.LEA_MAPPING lm ON lm.RTOM = w.AREA
WHERE (
    w.WO_STATUS IN ('OPEN', 'PENDING', 'IN PROGRESS', 'INPROGRESS', 'ASSIGNED', 'ACKNOWLEDGED', 'Acknowledged', 'Open', 'Pending', 'In Progress')
    OR (UPPER(ISNULL(w.WO_STATUS, '')) NOT IN ('CLOSED', 'RESOLVED', 'CANCELLED') AND ISNULL(w.WO_STATUS, '') != '')
    OR w.WO_STATUS IS NULL
)
AND (w.OPMC_NW = 'OPMC' OR lm.OPMC IS NOT NULL OR w.AREA IS NOT NULL)
AND (@TargetDate IS NULL OR CAST(ISNULL(w.STARTED_TIME, f.reportedtime) AS DATE) <= CAST(@TargetDate AS DATE))
ORDER BY ISNULL(w.STARTED_TIME, f.reportedtime) DESC;


-- ────────────────────────────────────────────────────────────
-- TYPE 3 : Pending SLT NOC Faults – Fault Management Teams
-- Source  : dbo.CLARITY_DATA_NW_FAULTS
SELECT TOP 100
    ROW_NUMBER() OVER(ORDER BY f.reportedtime DESC) AS No,
    ISNULL(NULLIF(f.circuit_name, ''), 'N/A') AS DataCircuitId,
    ISNULL(NULLIF(f.customer_name, ''), 'N/A') AS DetailsOfCustomer,
    CONVERT(varchar(12), f.reportedtime, 101) AS FaultDate,
    CONVERT(varchar(8),  f.reportedtime, 108) AS FaultTime,
    ISNULL(NULLIF(f.workgroup, ''), 'N/A') AS WorkOrdersHeldUpAt,
    ISNULL(NULLIF(f.description, ''), 'N/A') AS Description,
    CASE
        WHEN f.reportedtime IS NULL THEN '00:00:00'
        WHEN DATEDIFF(SECOND, f.reportedtime, GETDATE()) < 0 THEN '00:00:00'
        ELSE
            CAST(DATEDIFF(SECOND, f.reportedtime, GETDATE()) / 3600 AS VARCHAR) + ':' +
            RIGHT('00' + CAST((DATEDIFF(SECOND, f.reportedtime, GETDATE()) % 3600) / 60 AS VARCHAR), 2) + ':' +
            RIGHT('00' + CAST((DATEDIFF(SECOND, f.reportedtime, GETDATE()) % 3600) / 60 AS VARCHAR), 2)
    END AS DurationOfComplaint
FROM dbo.CLARITY_DATA_NW_FAULTS f
WHERE (
    f.problem_status IN ('OPEN', 'PENDING', 'IN PROGRESS', 'INPROGRESS', 'ASSIGNED', 'ACKNOWLEDGED', 'Acknowledged', 'Open', 'Pending', 'In Progress')
    OR (UPPER(ISNULL(f.problem_status, '')) NOT IN ('CLOSED', 'RESOLVED', 'CANCELLED') AND ISNULL(f.problem_status, '') != '')
    OR f.problem_status IS NULL
)
AND (@TargetDate IS NULL OR CAST(f.reportedtime AS DATE) <= CAST(@TargetDate AS DATE))
ORDER BY f.reportedtime DESC;