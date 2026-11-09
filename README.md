# Alarm System 23062026 - SLT NOC Network Monitoring Platform

## Overview

**AlarmSystem** is an ASP.NET Core 10.0 MVC enterprise web application designed for the Sri Lanka Telecom (SLT) Network Operations Center (NOC). The platform provides comprehensive live alarm monitoring, trouble ticket tracking, work order management, and network device analytics across Sri Lanka's telecommunications infrastructure.

> **Note on Decoupling**: The automated email reporting background engine has been completely extracted from this web project and migrated to an independent standalone console application located in `../email report`. This eliminates background database contention and ensures high-performance, non-blocking web operations.

---

## Key Features

- **Live Fault & Alarm Monitoring**: Real-time alarm aggregation from network probes (`NewDB` / `TMS`).
- **Work Order Management**: Seamless tracking of Clarity work orders, technician assignments, and escalation dockets.
- **Node & Element Outages**: Comprehensive status views for MSAN nodes, CEA ports, and SLT 4G/LTE base stations.
- **War Room Dashboard**: High-level network operations summary designed for NOC video wall displays.
- **Historical Analysis**: Long-term trend analysis by Regional Engineer, Province, and Network Platform.
- **Starlink Integration**: Dedicated API integration for monitoring remote Starlink satellite terminals.

---

## Project Structure

```
AlarmSystem/
├── Controllers/           # ASP.NET Core MVC Controllers
├── Models/                # Data Transfer Objects & Domain Entities
├── Services/              # Business Logic & SQL Integration Services
│   ├── Implementations/   # Core service implementations
│   └── Interfaces/        # Service interfaces
├── ViewModels/            # UI-bound view models
├── Views/                 # Razor Views (Bootstrap 5 + SLT Corporate Theme)
├── sql/                   # Migration scripts & SQL reference queries
├── docs/                  # System documentation & remediation plans
└── wwwroot/               # Static assets (CSS, JS, images)
```

---

## Getting Started

### Prerequisites

- **.NET 10.0 SDK**
- **Microsoft SQL Server** (Databases: `NewDB`, `TMS`, `FaultDB`, `SMSPORTAL`)
- **Azure AD App Registration** (for enterprise Single Sign-On)

### Configuration

Update connection strings and authentication parameters in `appsettings.json` or `.env`:

```json
{
  "ConnectionStrings": {
    "NewDB": "Server=localhost;Database=NewDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "TMS": "Server=localhost;Database=NewDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "FaultDb": "Server=localhost;Database=FaultDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "SmsDb": "Server=localhost;Database=SMSPORTAL;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5250"
      }
    }
  }
}
```

### Running the Web Application

```bash
cd AlarmSystem
dotnet run
```

The web portal will be accessible at: `http://localhost:5250`

---

## Automated Email Reporting Subsystem

The email reporting automation is maintained as a separate, autonomous console application:

- **Location**: `../email report/`
- **Project File**: `../email report/EmailReport.csproj`
- **Documentation**: See `../email report/README.md` and `../email report/docd/` for architecture, SQL specifications, and Windows Task Scheduler deployment guides.

---

## Documentation

Comprehensive engineering documentation is available in the `docs/` directory:
- `docs/01-PROJECT-OVERVIEW.md` - Full platform architecture and module overview
- `docs/02-DATABASE-SCHEMA.md` - Database tables, relationships, and stored procedures
- `docs/03-API-ENDPOINTS-AND-ROUTES.md` - MVC controllers and route dictionary
- `docs/04-PROJECT-STATUS-AND-BUGS.md` - Known issues, bug resolutions, and status logs
- `docs/05-SETUP-AND-DEPLOYMENT.md` - Production deployment and IIS hosting guide
- `docs/07-REMEDIATION-AND-FIXING-PLAN.md` - Step-by-step remediation plan
- `docs/08-CHANGE-REFERENCE-REPORT.md` - Complete change reference guide (what, where, and how)
- `docs/09-SIDEBAR-NAVIGATION-TREE.md` - UI sidebar navigation hierarchy & 4-section architecture

---

## License

This software and related documentation are proprietary and confidential to Sri Lanka Telecom (SLT).
