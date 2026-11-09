# Report 02: Database Schema & SQL Specification Deep Study
## SLT NOC Automated Email Reporting Database Layer

---

## 1. Database Architecture & Data Sources

The reporting system connects to the SLT NOC operational database (`NewDB` / `TMS`) running Microsoft SQL Server. It extracts data from two primary fault domains:

1. **Customer & Circuit Trouble Tickets (Clarity Domain)**: Captures reported customer circuit failures, held-up work orders, and field repair updates.
2. **Core Network & Element Alarms (NOC Alarms Domain)**: Captures physical element outages, MSAN power/link failures, and 4G base station down alarms.

---

## 2. Core Tables Schema

### Table: `dbo.CLARITY_WO_STATUS`
Tracks the operational lifecycle of work orders associated with network problems.

| Column Name | Data Type | Nullable | Description |
| :--- | :--- | :---: | :--- |
| `CLARITY_REF` | `VARCHAR(50)` | NO | Primary trouble ticket reference (links to `problem_no`) |
| `WO_STATUS` | `VARCHAR(50)` | YES | Current state (`OPEN`, `PENDING`, `IN PROGRESS`, `ASSIGNED`, etc.) |
| `WORKGROUP` | `VARCHAR(100)` | YES | Team currently assigned (`METRO_NW`, `OPMC_KANDY`, `NOC_FM`, etc.) |
| `STARTED_TIME`| `DATETIME` | YES | Timestamp when the work order was issued or started |
| `REGION` | `VARCHAR(50)` | YES | Operational region (e.g. `WP-SOUTH`, `CP`, `SP`) |
| `PROVINCE` | `VARCHAR(50)` | YES | Administrative province |

### Table: `dbo.CLARITY_DATA_NW_FAULTS`
Contains customer and circuit details for data network complaints.

| Column Name | Data Type | Nullable | Description |
| :--- | :--- | :---: | :--- |
| `problem_no` | `VARCHAR(50)` | NO | Trouble ticket identifier |
| `data_circuit_id` | `VARCHAR(100)` | YES | Data circuit identifier (e.g. `IP-VPN-10492`, `BB-09142`) |
| `cust_name` | `VARCHAR(200)` | YES | Customer enterprise or subscriber name |
| `reportedtime`| `DATETIME` | YES | Timestamp when complaint was logged into Clarity |
| `description` | `VARCHAR(MAX)` | YES | Problem description (e.g., *Link Down*, *Packet Loss*) |
| `lea` | `VARCHAR(50)` | YES | Local Exchange Area code |

### Table: `dbo.CLARITY_COMMENTS`
Stores incremental engineer handover logs and trouble ticket updates.

| Column Name | Data Type | Nullable | Description |
| :--- | :--- | :---: | :--- |
| `CLARITY_REF` | `VARCHAR(50)` | NO | Ticket reference |
| `COMMENT_DATE`| `DATETIME` | YES | Date and time the comment was entered |
| `COMMENTS` | `VARCHAR(MAX)` | YES | Technical progress log or field team update |

### Table: `dbo.FAULTS`
Stores real-time network alarm events polled from network monitoring probes.

| Column Name | Data Type | Nullable | Description |
| :--- | :--- | :---: | :--- |
| `FAULT_ID` | `BIGINT` | NO | Unique fault identifier |
| `PLATFORM` | `VARCHAR(50)` | YES | Network platform (`MSAN`, `LTE`, `TX`, `IP-RAN`) |
| `ALARM_TYPE` | `VARCHAR(100)` | YES | Alarm classification (`Node Down`, `Link Down`, `Power Alarm`) |
| `FAULT_STATUS`| `VARCHAR(20)` | YES | State of alarm (`OPEN`, `CLEARED`, `ACK`) |
| `FAULT_TIME` | `DATETIME` | YES | Exact timestamp when node or element went down |
| `SOURCEIP` | `VARCHAR(50)` | YES | Management IP address of the element |
| `REGION` | `VARCHAR(50)` | YES | Operational region |
| `PROVINCE` | `VARCHAR(50)` | YES | Operational province |
| `NW_ENG` | `VARCHAR(100)` | YES | Responsible network engineer or zone |

---

## 3. The 07:00 AM Daily Cutoff Mechanism

All 4 daily reports are governed by a strict **07:00 AM cutoff rule**. This ensures that morning handover meetings across all regions evaluate an exact, reproducible snapshot:

### Cutoff Algorithm
```csharp
// In FaultReportService.cs
var target = targetDate ?? await GetLatestDataDateAsync() ?? DateTime.Today;
var cutOffTime = target.Date.AddHours(7); // Exact cutoff: YYYY-MM-DD 07:00:00.000
```

### SQL Filter Application
```sql
-- Evaluates reported time against the cutoff parameter
WHERE (
    (w.STARTED_TIME IS NOT NULL AND w.STARTED_TIME <= @CutOffTime)
    OR (f.reportedtime IS NOT NULL AND f.reportedtime <= @CutOffTime)
)
```

---

## 4. HQ & Internal Test Circuit Exclusion Rule

To prevent headquarter test beds, lab equipment, and internal demo circuits from cluttering operational reports, an explicit exclusion clause is enforced across all queries:

```sql
AND (
    ISNULL(w.REGION, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS', 'HQ-NOC', 'IDC HQ NOC')
    AND ISNULL(w.PROVINCE, '') NOT LIKE '%HQ%'
    AND ISNULL(w.NW_ENG, '') NOT LIKE '%HQ%'
)
```

---

## 5. SQL Queries by Report Type

### Type 1: Regional Network Teams Query
```sql
SELECT TOP 100
    ISNULL(f.data_circuit_id, 'N/A') AS DataCircuitId,
    ISNULL(f.cust_name, 'N/A') AS DetailsOfCustomer,
    CONVERT(VARCHAR(10), COALESCE(w.STARTED_TIME, f.reportedtime), 120) AS FaultDate,
    CONVERT(VARCHAR(5), COALESCE(w.STARTED_TIME, f.reportedtime), 108) AS FaultTime,
    ISNULL(w.WORKGROUP, 'UNASSIGNED') AS WorkOrdersHeldUpAt,
    ISNULL(f.description, 'No Description') AS Description,
    DATEDIFF(HOUR, COALESCE(w.STARTED_TIME, f.reportedtime), @CutOffTime) AS DurationHours,
    ISNULL(w.REGION, 'N/A') AS Region,
    ISNULL(w.PROVINCE, 'N/A') AS Province
FROM dbo.CLARITY_WO_STATUS w
LEFT JOIN dbo.CLARITY_DATA_NW_FAULTS f ON f.problem_no = w.CLARITY_REF
WHERE (w.WO_STATUS IN ('OPEN', 'PENDING', 'IN PROGRESS', 'ASSIGNED', 'ACKNOWLEDGED'))
  AND (w.WORKGROUP LIKE '%NW%' OR w.WORKGROUP LIKE '%REGIONAL%' OR w.WORKGROUP LIKE '%METRO%')
  AND (w.WORKGROUP NOT LIKE '%OPMC%' AND w.WORKGROUP NOT LIKE '%NOC%')
  AND (COALESCE(w.STARTED_TIME, f.reportedtime) <= @CutOffTime)
  AND (ISNULL(w.REGION, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS', 'HQ-NOC', 'IDC HQ NOC'))
ORDER BY COALESCE(w.STARTED_TIME, f.reportedtime) ASC;
```

### Type 2: OPMC Teams Query
```sql
SELECT TOP 100
    CONVERT(VARCHAR(10), COALESCE(f.reportedtime, w.STARTED_TIME), 120) AS ReportedDate,
    ISNULL(f.data_circuit_id, 'N/A') AS CircuitName,
    ISNULL(f.cust_name, 'N/A') AS Customer,
    CONVERT(VARCHAR(5), COALESCE(f.reportedtime, w.STARTED_TIME), 108) AS OutageTime,
    ISNULL(f.lea, 'N/A') AS Lea,
    ISNULL(w.WORKGROUP, 'OPMC') AS RelevantOpmc,
    ISNULL(w.REGION, 'N/A') AS Dgm,
    ISNULL(w.PROVINCE, 'N/A') AS Gm,
    ISNULL(f.description, 'Pending Repair') AS Description
FROM dbo.CLARITY_WO_STATUS w
LEFT JOIN dbo.CLARITY_DATA_NW_FAULTS f ON f.problem_no = w.CLARITY_REF
WHERE (w.WO_STATUS IN ('OPEN', 'PENDING', 'IN PROGRESS', 'ASSIGNED', 'ACKNOWLEDGED'))
  AND (w.WORKGROUP LIKE '%OPMC%' OR w.WORKGROUP LIKE '%OUTSIDE PLANT%' OR w.WORKGROUP LIKE '%CABLE%')
  AND (COALESCE(f.reportedtime, w.STARTED_TIME) <= @CutOffTime)
  AND (ISNULL(w.REGION, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS', 'HQ-NOC', 'IDC HQ NOC'))
ORDER BY COALESCE(f.reportedtime, w.STARTED_TIME) ASC;
```

### Type 3: SLT NOC Fault Management Query
```sql
SELECT TOP 100
    ISNULL(f.data_circuit_id, 'N/A') AS DataCircuitId,
    ISNULL(f.cust_name, 'N/A') AS DetailsOfCustomer,
    CONVERT(VARCHAR(10), f.reportedtime, 120) AS ReportedDate,
    CONVERT(VARCHAR(5), f.reportedtime, 108) AS ReportedTime,
    ISNULL(w.WORKGROUP, 'NOC-FM') AS WorkGroup,
    ISNULL(f.description, 'Fault Under Investigation') AS Description,
    ISNULL(w.WO_STATUS, 'OPEN') AS Status,
    ISNULL(w.PROVINCE, 'N/A') AS Province
FROM dbo.CLARITY_DATA_NW_FAULTS f
LEFT JOIN dbo.CLARITY_WO_STATUS w ON w.CLARITY_REF = f.problem_no
WHERE (f.problem_status IN ('OPEN', 'PENDING', 'IN PROGRESS', 'ASSIGNED', 'ACKNOWLEDGED')
    OR w.WO_STATUS IN ('OPEN', 'PENDING', 'IN PROGRESS', 'ASSIGNED', 'ACKNOWLEDGED'))
  AND (w.WORKGROUP LIKE '%NOC%' OR w.WORKGROUP LIKE '%FAULT MANAGEMENT%' OR w.WORKGROUP LIKE '%FM%')
  AND (f.reportedtime <= @CutOffTime)
ORDER BY f.reportedtime ASC;
```

### Type 4: Comprehensive Network Failures Query (TMS Alarms)
```sql
-- 1. MSAN Node Down
SELECT f.NODE_NAME, f.IP, f.REGION, f.PROVINCE, f.FAULT_TIME, f.DURATION
FROM dbo.FAULTS f
WHERE f.FAULT_STATUS = 'OPEN'
  AND f.PLATFORM = 'MSAN'
  AND (f.ALARM_TYPE = 'Node Down' OR f.ALARM_TYPE LIKE '%Node%')
  AND f.FAULT_TIME <= @CutOffTime
  AND (ISNULL(f.REGION, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS', 'HQ-NOC', 'IDC HQ NOC'));

-- 2. SLT 4G Sites Down
SELECT f.NODE_NAME, f.IP, f.REGION, f.PROVINCE, f.FAULT_TIME, f.DURATION
FROM dbo.FAULTS f
WHERE f.FAULT_STATUS = 'OPEN'
  AND f.PLATFORM IN ('LTE', '4G', '5G', 'MOBILE', 'RAN', 'SLT 4G')
  AND f.FAULT_TIME <= @CutOffTime
  AND (ISNULL(f.REGION, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS', 'HQ-NOC', 'IDC HQ NOC'));

-- 3. Transmission Critical Links Down
SELECT f.NODE_NAME, f.IP, f.REGION, f.PROVINCE, f.FAULT_TIME, f.DURATION
FROM dbo.FAULTS f
WHERE f.FAULT_STATUS = 'OPEN'
  AND f.PLATFORM IN ('TX', 'TRANSMISSION', 'DWDM', 'SDH', 'SLBN')
  AND f.FAULT_TIME <= @CutOffTime
  AND (ISNULL(f.REGION, '') NOT IN ('HQ', 'CENTRAL', 'HEADQUARTERS', 'HQ-NOC', 'IDC HQ NOC'));
```
