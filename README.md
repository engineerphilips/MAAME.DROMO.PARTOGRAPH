# MAAME.DROMO.PARTOGRAPH

A comprehensive mobile health (mHealth) application for monitoring maternal labor and delivery using **WHO 2020 Partograph** standards. Designed for healthcare providers in low-resource settings, it enables tracking of patient progress during all four stages of labor with real-time clinical alerts, offline capability, and comprehensive reporting.

---

## Architecture

The solution is built on the .NET ecosystem and organized into four projects:

| Project | Framework | Purpose |
|---|---|---|
| `MAAME.DROMO.PARTOGRAPH.APP.Droid` | .NET MAUI (.NET 9) | Mobile application (Android/iOS) |
| `MAAME.DROMO.PARTOGRAPH.SERVICE` | ASP.NET Core 8 Web API | Backend REST API |
| `MAAME.DROMO.PARTOGRAPH.MODEL` | .NET Class Library | Shared domain models (59 entities) |
| `MAAME.DROMO.PARTOGRAPH.MONITORING` | Blazor (ASP.NET Core) | Web monitoring dashboard |

---

## Key Features

### Clinical Monitoring (Mobile App)

- Full WHO 2020 partograph tracking across all 4 stages of labor
- Fetal Heart Rate (FHR) monitoring with automated pattern analysis and deceleration detection
- Cervical dilatation plotting, head descent tracking, contraction monitoring
- Maternal vitals: blood pressure, pulse, temperature, urine analysis
- Bishop Score calculator (0-13 scale) and automated 4-level risk assessment (Low / Moderate / High / Critical)
- Real-time clinical alert engine with color-coded severity for prolonged labor, fetal distress, and vital sign abnormalities

### Patient and Outcome Management

- Patient registration with full obstetric history, demographics, and risk factors
- Birth outcome recording: delivery mode, perineal trauma, postpartum hemorrhage, complications
- Neonatal assessment: APGAR scores, birth weight classification, resuscitation, immediate newborn care
- Multi-baby support (up to 5 per delivery)
- Emergency, urgent, and routine referral system with 25+ predefined referral reasons

### Offline-First Architecture

- Full functionality without internet connectivity
- Configurable background auto-sync (5-120 minute intervals)
- Delta sync: only changed data is transmitted
- Version-based conflict detection with manual resolution UI
- Real-time connectivity monitoring with automatic retry and exponential backoff

### Reporting and Analytics

- **9 report types:** monthly delivery dashboard, maternal complications, neonatal outcomes, WHO compliance, staff performance, alert response times, birth weight/APGAR analysis, trend analytics, and offline sync status
- WHO 2020-compliant partograph PDF export
- Alert history and hourly distribution analytics

### Web Monitoring Dashboard (Blazor)

- Live labor monitoring with real-time updates via SignalR
- WHO 2020 Labor Care Guide reference
- Data quality monitoring and audit logging
- Proof of concept (POC) metrics dashboard
- User and staff management

---

## Data Layer

- **73+ database tables** managed through Entity Framework Core
- Every record includes sync metadata: timestamps, device IDs, version tracking, conflict data, and data hashes
- 60+ SQLite repositories on the mobile side for offline storage
- SQL Server backend with full migration support

### Core Data Entities

- **Patient:** demographics, obstetric history, medical conditions, risk factors
- **Partograph:** labor monitoring records linked to patients
- **Measurements:** FHR, contractions, cervical dilatation, head descent, vitals, amniotic fluid, caput, moulding, fetal position, and more
- **Outcomes:** birth outcomes, baby details, referrals
- **Analytics:** daily/monthly facility statistics, delivery outcome summaries, maternal mortality records, neonatal outcome records

---

## Tech Stack

| Layer | Technology |
|---|---|
| Mobile UI | XAML + MVVM, Syncfusion Maui components, CommunityToolkit.Maui |
| Mobile Storage | SQLite (sqlite-net-pcl) |
| Backend API | ASP.NET Core 8, Entity Framework Core 8, Swagger |
| Server Database | SQL Server |
| Realtime | SignalR hubs |
| PDF Generation | Syncfusion PDF |
| Authentication | JWT Bearer tokens |
| Cloud | Azure Identity, Application Insights |
| Web Dashboard | Blazor with Razor components |

---

## Project Structure

```
MAAME.DROMO.PARTOGRAPH/
├── MAAME.DROMO.PARTOGRAPH.APP.Droid/   # Mobile application
│   ├── Pages/                          # 124 XAML UI pages
│   ├── PageModels/                     # 70+ MVVM ViewModels
│   ├── Data/                           # 60+ SQLite repositories
│   ├── Services/                       # 20+ business services
│   ├── Utilities/                      # Helper classes
│   ├── Resources/                      # Images, fonts, colors
│   └── MauiProgram.cs                  # DI configuration
├── MAAME.DROMO.PARTOGRAPH.SERVICE/     # Backend API
│   ├── Controllers/                    # 11 API controllers
│   ├── Services/                       # Analytics, PDF, Robson, Auth
│   ├── Data/                           # EF Core DbContext (73 DbSets)
│   └── Migrations/                     # Database migrations
├── MAAME.DROMO.PARTOGRAPH.MODEL/       # Shared domain models
│   └── 59 C# entity classes
├── MAAME.DROMO.PARTOGRAPH.MONITORING/  # Web dashboard
│   ├── Components/                     # Blazor Razor components
│   ├── Pages/                          # Dashboard pages
│   ├── Services/                       # Monitoring services
│   └── Hubs/                           # SignalR real-time hubs
└── MAAME.DROMO.PARTOGRAPH.sln          # Solution file
```

---

## Clinical Standards

The application implements the following WHO clinical standards:

- **WHO 2020 Partograph:** complete partograph grid with time axis, alert lines, action lines, and transfer lines
- **Bishop Score:** 13-point cervical scoring system for labor readiness assessment
- **Robson Classification:** WHO 10-group classification system for cesarean section analysis
- **APGAR Scoring:** standardized neonatal assessment at 1 and 5 minutes

---

## Target Deployment

The application is built for primary and secondary healthcare facilities in low-resource settings where reliable internet may not always be available:

- **Mobile app** runs on Android devices (21.0+) at the point of care
- **Backend API** can be deployed on-premise or in the cloud (SQL Server)
- **Web dashboard** provides supervisors and administrators with centralized monitoring

---

## Platform Support

| Platform | Minimum Version |
|---|---|
| Android | 21.0 (Lollipop) |
| iOS | 15.0 |
| macOS | Catalyst |
| Windows | 10.0.17763.0 |
