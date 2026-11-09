# SLT NOC Automated Email Reporting System
## Executive Summary & Reports Documentation Index

---

## 📌 Executive Summary

The **SLT NOC Automated Email Reporting System** is a mission-critical operational reporting engine designed to deliver scheduled, high-fidelity network fault snapshots to SLT engineering, maintenance, and management teams. 

Originally integrated directly into the `AlarmSystem` web platform, this reporting system has now been **completely decoupled into an autonomous, standalone .NET 10 Console Application** located in:
`D:\SLT_Projects\Project 03\AlarmSystem_23062026\email report`

This decoupling achieves three key operational objectives:
1. **Zero Impact on Web Application Performance**: Eliminates continuous background worker thread contention and database load from the main SLT NOC web monitoring platform.
2. **Deterministic Schedule Execution**: Allows the automation engine to run via Windows Task Scheduler or as a resilient background Windows Service, ensuring exact 07:00 AM morning handover report delivery.
3. **Multi-Format Distribution**: Automatically generates and delivers both **responsive corporate HTML email bodies** and **structured ClosedXML Excel workbooks (`.xlsx`)** from a single command or schedule trigger.

---

## 📂 Live Operational Report Artifacts Generated

The following live report artifacts were queried from the active `NewDB`/`TMS` database and generated into this folder:

| Report ID | Report Name | Target Audience | Active Records (Snapshot) | Generated Files |
| :--- | :--- | :--- | :---: | :--- |
| **Type 1** | **Pending Regional Network Faults** | Regional Network Engineers & Regional Heads | **8 Faults** | [`Report_Type1_Regional_Network_Faults.html`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/Report_Type1_Regional_Network_Faults.html)<br>[`Pending_Regional_Network_Faults_20261019.xlsx`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/Pending_Regional_Network_Faults_20261019.xlsx) |
| **Type 2** | **Pending OPMC Faults** | Outside Plant Maintenance Centers & Cable Teams | **4 Faults** | [`Report_Type2_OPMC_Faults.html`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/Report_Type2_OPMC_Faults.html)<br>[`Pending_OPMC_Faults_20261019.xlsx`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/Pending_OPMC_Faults_20261019.xlsx) |
| **Type 3** | **SLT NOC Fault Management** | NOC Tier 2/3, Core, and Fault Management Teams | **55 Faults** | [`Report_Type3_SLT_NOC_Fault_Management.html`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/Report_Type3_SLT_NOC_Fault_Management.html)<br>[`Pending_NOC_Fault_Management_20261019.xlsx`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/Pending_NOC_Fault_Management_20261019.xlsx) |
| **Type 4** | **Comprehensive Network Failures** | Senior Executives, DGMs, GMs, & NOC Management | **12 Outages**<br>(2 MSAN, 10 4G Sites, 0 TX) | [`Report_Type4_Comprehensive_Network_Failures.html`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/Report_Type4_Comprehensive_Network_Failures.html)<br>[`Pending_Network_Failures_20261019.xlsx`](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/Pending_Network_Failures_20261019.xlsx) |

---

## 📚 Technical Documentation Index

Detailed architectural and functional specifications are organized into the following reports:

1. [**`01-SYSTEM-ARCHITECTURE-AND-DECOUPLING.md`**](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/01-SYSTEM-ARCHITECTURE-AND-DECOUPLING.md)  
   Complete architectural overview of the standalone application, dependency injection setup, and how it was cleanly separated from the monolithic web application.

2. [**`02-DATABASE-SCHEMA-AND-SQL-SPECIFICATION.md`**](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/02-DATABASE-SCHEMA-AND-SQL-SPECIFICATION.md)  
   Exhaustive analysis of the SQL queries, table schemas (`CLARITY_WO_STATUS`, `CLARITY_DATA_NW_FAULTS`, `FAULTS`), indexing recommendations, HQ exclusion rules, and the strict **07:00 AM cutoff filter**.

3. [**`03-REPORT-TYPES-DEEP-STUDY.md`**](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/03-REPORT-TYPES-DEEP-STUDY.md)  
   Detailed breakdown of all 4 operational reports: business rules, target recipients, field definitions, and summary aggregation matrices.

4. [**`04-HTML-AND-EXCEL-ENGINES.md`**](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/04-HTML-AND-EXCEL-ENGINES.md)  
   Deep dive into the dual-output rendering engine: inline CSS styling for high-compatibility HTML emails, and ClosedXML workbook builders for Excel spreadsheets.

5. [**`05-AUTOMATION-SCHEDULER-AND-DEPLOYMENT.md`**](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/05-AUTOMATION-SCHEDULER-AND-DEPLOYMENT.md)  
   Complete operational manual for automating report dispatches: CLI flags, Windows Task Scheduler scripts, Windows Service setup, and duplicate dispatch prevention algorithms.

6. [**`06-CONFIGURATION-SECURITY-AND-SMTP.md`**](file:///D:/SLT_Projects/Project%2003/AlarmSystem_23062026/email%20report/docd/06-CONFIGURATION-SECURITY-AND-SMTP.md)  
   Configuration guide covering `appsettings.json`, Office 365 / Exchange SMTP settings, TLS/SSL configuration, and `.env` credential isolation.
