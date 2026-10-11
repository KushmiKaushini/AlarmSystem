# Alarm System 23062026

## Overview

This is an ASP.NET Core 10.0 MVC application designed for network operations monitoring and reporting. The system provides comprehensive fault tracking, work order management, and automated email reporting capabilities for Sri Lanka Telecom (SLT) Network Operations Center (NOC).

## Key Features

### Core Functionality

- Fault monitoring and tracking
- Work order management
- Alarm and event handling
- Historical data analysis
- Escalation management
- Network device monitoring (MSAN, CEA, 4G sites)

### Email Reporting System

The application includes a fully automated email reporting system that generates and sends four types of daily network operations reports:

1. **Type 1: Regional Network Teams** - Pending Data Faults related to Regional Network Teams (08:00 daily)
2. **Type 2: Pending OPMC Faults** - Faults pending with OPMC teams (07:00 daily)
3. **Type 3: SLT NOC Fault Management** - Pending Data Faults relating to SLT NOC – Fault Management Teams (00:15 daily)
4. **Type 4: Daily Network Failures** - Full Network Failure Summary Report (08:00 daily)

#### Email Reporting Components

- **FaultReportService**: Core logic for generating HTML email reports with dynamic SQL queries using GETDATE()
- **DailyEmailReportSchedulerService**: Background service that sends reports at scheduled times
- **EmailSenderService**: SMTP email sending implementation
- **Preview Endpoints**: Web interface to preview reports before sending
- **Configuration**: SMTP settings, recipients, and schedules configurable via appsettings.json

## Project Structure

```
AlarmSystem/
├── Controllers/           # MVC Controllers
├── Models/               # Data Models
├── Services/             # Business Logic Services
│   ├── EmailReporting/   # Email reporting services
│   ├── EmailSender/      # Email sending service
│   └── ...               # Other services
├── Views/                # Razor Views
├── sql/                  # SQL Queries
├── docs/                 # Documentation
└── wwwroot/              # Static Files
```

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- SQL Server Database (TMS, FaultDB, SMSPORTAL)
- SMTP Server Access (for email reporting)

### Configuration

1. Update connection strings in `appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "TMS": "Server=your_server;Database=TMS;Trusted_Connection=True;",
       "FaultDb": "Server=your_server;Database=FaultDB;Trusted_Connection=True;",
       "SmsDb": "Server=your_server;Database=SMSPORTAL;Trusted_Connection=True;"
     }
   }
   ```

2. Configure email reporting in `appsettings.json`:

   ```json
   {
     "EmailReporting": {
       "Enabled": true,
       "Smtp": {
         "Host": "your-smtp-host",
         "Port": 587,
         "Username": "", // Set via Azure Key Vault or environment variables in production
         "Password": "", // Set via Azure Key Vault or environment variables in production
         "EnableSsl": true,
         "FromAddress": "" // Set via Azure Key Vault or environment variables in production
       },
       "Recipients": {
         "1": "", // Configure recipients as needed
         "2": "", // Configure recipients as needed
         "3": "", // Configure recipients as needed
         "4": ""  // Configure recipients as needed
       },
       "Schedule": {
         "1": "", // Configure schedule as needed (e.g., "08:00")
         "2": "", // Configure schedule as needed (e.g., "07:00")
         "3": "", // Configure schedule as needed (e.g., "00:15")
         "4": ""  // Configure schedule as needed (e.g., "08:00")
       }
     }
   }
   ```

> **Note**: For security, all hardcoded credentials have been removed. In production, configure SMTP credentials and recipients using Azure Key Vault, environment variables, or other secure configuration providers. The application now requires Azure AD SSO for all access - there is no development bypass mechanism.

### Running the Application

```bash
cd AlarmSystem
dotnet run
```

The application will be available at <http://localhost:5200>

## Email Reporting Preview

To preview reports without sending:

- Type 1: <http://localhost:5200/preview-email?type=1>
- Type 2: <http://localhost:5200/preview-email?type=2>
- Type 3: <http://localhost:5200/preview-email?type=3>
- Type 4: <http://localhost:5200/preview-email?type=4>

You can also specify a date: <http://localhost:5200/preview-email?type=1&date=2026-10-04>

## Documentation

- Full Project Review & Technical Assessment: `docs/PROJECT_REVIEW.md`
- System Architecture Documentation: `docs/ARCHITECTURE.md`
- Database Schema Specification: `docs/DATABASE_SCHEMA.md`
- Deployment & Configuration Guide: `docs/DEPLOYMENT.md`
- Developer & Contributor Guide: `docs/DEVELOPER_GUIDE.md`
- API Reference Documentation: `docs/API_REFERENCE.md`

## Build Status

The application builds successfully with only pre-existing warnings:

- NU1902: Package vulnerability warnings (Microsoft.Identity.Abstractions, Microsoft.Identity.Web)
- CS8618: Non-nullable property warnings (existing in the codebase)

These warnings do not affect functionality and are being monitored for future updates.

## License

This project is proprietary to Sri Lanka Telecom (SLT).

---

*Last Updated: 2026-10-05*