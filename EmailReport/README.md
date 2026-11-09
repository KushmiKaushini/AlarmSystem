# SLT NOC - Automated Email Reporting Console Application

A standalone, high-performance .NET 10 console application and scheduled daemon for automated daily telecommunications network fault reporting across Sri Lanka Telecom (SLT).

---

## 📋 Executive Overview

This application was extracted from the SLT NOC `AlarmSystem` web platform to operate autonomously as a dedicated background worker or scheduled task. It connects to the `NewDB` / `TMS` SQL database, evaluates active network complaints against a strict **07:00 AM cutoff filter**, and generates dual-format outputs:

1. **Responsive HTML Email Reports**: Delivered directly to designated engineering distribution groups via SMTP.
2. **Structured Excel Workbooks (`.xlsx`)**: Formatted using ClosedXML with corporate headers, zebra striping, and multi-sheet summaries.

### 4 Daily Operational Report Types
1. **Type 1: Pending Regional Network Faults** — Customer data circuit faults held up at regional network engineering teams.
2. **Type 2: Pending OPMC Faults** — Outside Plant Maintenance cable and physical infrastructure repairs.
3. **Type 3: SLT NOC Fault Management** — Core network, routing, and NOC Tier 2/3 pending escalations.
4. **Type 4: Comprehensive Network Failures** — Executive summary of all physical outages (MSAN nodes, 4G base stations, and Transmission links).

---

## 🚀 Quick Start Guide

### 1. Build the Application
```bash
cd "email report"
dotnet build
```

### 2. Generate All Reports (HTML + Excel) into `docd/`
To immediately generate all 4 live HTML preview reports and ClosedXML Excel workbooks from the database without sending emails:
```bash
dotnet run -- --generate-all
```
Generated files are placed in [`docd/`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd).

---

## 💻 CLI Commands & Execution Modes

The application supports multiple execution modes suitable for interactive testing, ad-hoc exports, and scheduled automation:

### A. Preview Mode (HTML on Disk)
Generates the HTML email body and saves it locally for browser inspection:
```bash
dotnet run -- --preview 1     # Type 1: Regional Network Faults
dotnet run -- --preview 2     # Type 2: OPMC Faults
dotnet run -- --preview 3     # Type 3: SLT NOC Fault Management
dotnet run -- --preview 4     # Type 4: Comprehensive Network Failures
```

### B. Excel Export Mode (`.xlsx` on Disk)
Generates native ClosedXML Excel spreadsheets without needing Microsoft Excel installed:
```bash
dotnet run -- --export 1      # Export Type 1 Excel workbook
dotnet run -- --export 4      # Export Type 4 multi-sheet Excel workbook
dotnet run -- --export all    # Export all 4 Excel workbooks
```

### C. Immediate SMTP Dispatch Mode (Task Scheduler / On-Demand)
Queries the database and sends the email reports immediately via SMTP:
```bash
# Send a specific report type:
dotnet run -- --type 1
dotnet run -- --type 2
dotnet run -- --type 3
dotnet run -- --type 4

# Send all 4 reports sequentially:
dotnet run -- --type all
# Or shorthand:
dotnet run -- --run-now
```

### D. Continuous Scheduler Daemon Mode
Runs indefinitely as a resident background service. Automatically monitors the system clock and triggers dispatches on working days (Monday–Friday) at the configured hours (e.g. `07:00 AM`):
```bash
dotnet run
```
*Press `Ctrl + C` to stop the daemon gracefully.*

---

## ⚙️ Configuration & Secret Management

Configuration can be managed via [`appsettings.json`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/appsettings.json) or isolated in a local `.env` file (copied from [`.env.example`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/.env.example)):

```ini
# Database Connection
ConnectionStrings__NewDB=Server=localhost;Database=NewDB;Trusted_Connection=True;TrustServerCertificate=True;

# SMTP Server Parameters
EmailReporting__Smtp__Host=smtp.office365.com
EmailReporting__Smtp__Port=587
EmailReporting__Smtp__Username=noc-report@slt.com.lk
EmailReporting__Smtp__Password=YourSecurePassword
EmailReporting__Smtp__FromAddress=no-reply@slt.com.lk
EmailReporting__Smtp__EnableSsl=true

# Recipient Routing by Report Type (semicolon-separated)
EmailReporting__Recipients__1=regional.engineers@slt.com.lk;provincial.dgm@slt.com.lk
EmailReporting__Recipients__2=opmc.supervisors@slt.com.lk
EmailReporting__Recipients__3=noc.fault.mgmt@slt.com.lk
EmailReporting__Recipients__4=management.overview@slt.com.lk
EmailReporting__DefaultRecipient=network.engineer@slt.com.lk
```

---

## 🛠️ Production Automation Setup

### Windows Task Scheduler (Recommended Enterprise Setup)

1. **Publish the Release Binary**:
   ```powershell
   dotnet publish -c Release -o "C:\SLT_Services\EmailReport"
   ```

2. **Register the Task in PowerShell (Run as Administrator)**:
   ```powershell
   $Action = New-ScheduledTaskAction -Execute "C:\SLT_Services\EmailReport\EmailReport.exe" -Argument "--type all" -WorkingDirectory "C:\SLT_Services\EmailReport"
   $Trigger = New-ScheduledTaskTrigger -Weekly -DaysOfWeek Monday, Tuesday, Wednesday, Thursday, Friday -At 07:00am
   $Settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -RestartCount 3 -RestartInterval (New-TimeSpan -Minutes 5)

   Register-ScheduledTask -TaskName "SLT_NOC_DailyEmailReport" -Action $Action -Trigger $Trigger -Settings $Settings -Description "Dispatches daily 07:00 AM SLT NOC fault reports"
   ```

---

## 📚 Technical Documentation & Live Artifacts (`docd/`)

All generated reports and in-depth engineering studies are archived in [`docd/`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd):

| Document / Artifact | Description |
| :--- | :--- |
| [`00-EXECUTIVE-SUMMARY-AND-REPORTS-INDEX.md`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/00-EXECUTIVE-SUMMARY-AND-REPORTS-INDEX.md) | Executive summary and artifact navigation index |
| [`01-SYSTEM-ARCHITECTURE-AND-DECOUPLING.md`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/01-SYSTEM-ARCHITECTURE-AND-DECOUPLING.md) | Architectural study of the decoupling from `AlarmSystem` |
| [`02-DATABASE-SCHEMA-AND-SQL-SPECIFICATION.md`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/02-DATABASE-SCHEMA-AND-SQL-SPECIFICATION.md) | SQL tables, queries, indexing, and 07:00 AM cutoff rules |
| [`03-REPORT-TYPES-DEEP-STUDY.md`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/03-REPORT-TYPES-DEEP-STUDY.md) | Detailed specifications and columns for Types 1–4 |
| [`04-HTML-AND-EXCEL-ENGINES.md`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/04-HTML-AND-EXCEL-ENGINES.md) | Inline CSS HTML email engine & ClosedXML Excel builder |
| [`05-AUTOMATION-SCHEDULER-AND-DEPLOYMENT.md`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/05-AUTOMATION-SCHEDULER-AND-DEPLOYMENT.md) | Background worker logic, deduplication & deployment guide |
| [`06-CONFIGURATION-SECURITY-AND-SMTP.md`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/06-CONFIGURATION-SECURITY-AND-SMTP.md) | Configuration parameters, SMTP TLS/SSL, & secret safety |
| `Report_Type1_Regional_Network_Faults.html` | Live generated Type 1 HTML report |
| `Pending_Regional_Network_Faults_*.xlsx` | Live generated Type 1 ClosedXML spreadsheet |
| `Report_Type2_OPMC_Faults.html` | Live generated Type 2 HTML report |
| `Pending_OPMC_Faults_*.xlsx` | Live generated Type 2 ClosedXML spreadsheet |
| `Report_Type3_SLT_NOC_Fault_Management.html` | Live generated Type 3 HTML report |
| `Pending_NOC_Fault_Management_*.xlsx` | Live generated Type 3 ClosedXML spreadsheet |
| `Report_Type4_Comprehensive_Network_Failures.html`| Live generated Type 4 HTML report |
| `Pending_Network_Failures_*.xlsx` | Live generated Type 4 ClosedXML spreadsheet |

---

## 📁 Directory Structure

```
email report/
├── EmailReport.csproj          # .NET 10 Console project file
├── Program.cs                  # CLI parser, DI container, and runner
├── appsettings.json            # Database & SMTP configuration
├── .env.example                # Sample environment secrets template
├── README.md                   # This documentation file
├── Configuration/
│   └── EmailReportingOptions.cs# Options class mapping configuration
├── Controllers/
│   └── DailyEmailReportController.cs # ClosedXML Excel workbook builders
├── Models/
│   ├── EmailReport/            # Query row models and DTOs (Types 1-4)
│   └── PendingFaultReport/     # Legacy compatibility DTO
├── Services/
│   ├── EmailReporting/         # Background scheduler hosted service
│   ├── EmailSender/            # SMTP transport client implementation
│   └── FaultReportService/     # SQL extraction and HTML template engine
├── docd/                       # Generated live HTML/Excel reports & technical docs
└── sql/
    └── EmailReport_Queries.sql # Standalone reference SQL scripts
```

---

## 🔒 License

Proprietary and confidential to Sri Lanka Telecom (SLT).
