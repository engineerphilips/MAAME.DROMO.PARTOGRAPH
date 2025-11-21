# Database Migration Guide: SQLite to MSSQL Server

## Overview
This guide explains how to complete the migration from SQLite to Microsoft SQL Server.

## Changes Made

### 1. NuGet Packages Updated
- **Removed**: `Microsoft.EntityFrameworkCore.Sqlite`
- **Added**: `Microsoft.EntityFrameworkCore.SqlServer`

### 2. Connection Strings Updated

#### Production (`appsettings.json`)
```json
"DefaultConnection": "Server=localhost;Database=PartographDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
```

#### Development (`appsettings.Development.json`)
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PartographDb;Trusted_Connection=True;MultipleActiveResultSets=true"
```

### 3. Program.cs Updated
- Changed `options.UseSqlite()` to `options.UseSqlServer()`
- Changed `Database.EnsureCreated()` to `Database.Migrate()` for proper migration support

## Steps to Complete Migration

### Step 1: Restore NuGet Packages
```bash
cd MAAME.DROMO.PARTOGRAPH.SERVICE
dotnet restore
```

### Step 2: Create Initial Migration
```bash
dotnet ef migrations add InitialCreate
```

This will create a `Migrations` folder with the initial database schema.

### Step 3: Update Connection String (if needed)
Update the connection string in `appsettings.json` or `appsettings.Development.json` to match your SQL Server instance:

**For Local SQL Server:**
```json
"Server=localhost;Database=PartographDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
```

**For SQL Server with Authentication:**
```json
"Server=your-server;Database=PartographDb;User Id=your-user;Password=your-password;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

**For Azure SQL Database:**
```json
"Server=tcp:your-server.database.windows.net,1433;Database=PartographDb;User Id=your-user;Password=your-password;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### Step 4: Apply Migration to Database
```bash
dotnet ef database update
```

This command will:
- Create the `PartographDb` database if it doesn't exist
- Apply all pending migrations
- Create all tables, indexes, and relationships

### Step 5: Verify Migration
Run the application:
```bash
dotnet run
```

Navigate to the Swagger UI (usually `https://localhost:5001/swagger`) to verify the API is working.

## Database Schema

The migration will create the following tables:

### Core Tables
- **Patients** - Patient information with sync support
- **Partographs** - Partograph records linked to patients
- **Staff** - Healthcare staff/user accounts

### Measurement Tables (20 types)
- FHRs, Contractions, CervixDilatations, HeadDescents
- BPs, Temperatures, AmnioticFluids, Urines
- Caputs, Mouldings, FetalPositions
- PainReliefEntries, PostureEntries, OralFluidEntries
- IVFluidEntries, MedicationEntries, Oxytocins
- CompanionEntries, AssessmentPlanEntries, MedicalNotes

### Supporting Tables
- VitalSigns, Projects, ProjectTasks
- Categories, Tags, ProjectsTags

## Key Features

### Sync Support
All tables include comprehensive sync columns:
- `CreatedTime`, `UpdatedTime`, `DeletedTime`
- `DeviceId`, `OriginDeviceId`
- `SyncStatus`, `ServerVersion`, `Version`
- `ConflictData`, `DataHash`

### Indexes
Strategic indexes for:
- Sync operations (`UpdatedTime`, `SyncStatus`, `ServerVersion`)
- Foreign key relationships
- Temporal queries
- Unique constraints (Email, Tag names)

### Relationships
- Patient → Partographs (One-to-Many, Cascade Delete)
- Partograph → Measurements (One-to-Many, Cascade Delete)
- Patient → MedicalNotes (One-to-Many, Cascade Delete)

## Troubleshooting

### Error: "Cannot connect to SQL Server"
- Ensure SQL Server is running
- Verify the connection string is correct
- Check firewall settings
- For LocalDB, ensure it's installed: `sqllocaldb info`

### Error: "Login failed for user"
- Verify credentials in connection string
- Ensure the SQL Server user has appropriate permissions
- For Windows Authentication, ensure the app pool identity has access

### Error: Migration already applied
```bash
dotnet ef database update 0
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Migration from Existing SQLite Data

If you have existing data in SQLite that needs to be migrated:

1. Keep the old SQLite database file
2. Create a data migration script or tool
3. Export data from SQLite and import to SQL Server
4. Alternatively, use a third-party tool like:
   - SQL Server Migration Assistant
   - DB Browser for SQLite with export functionality
   - Custom C# migration utility

## Additional Resources

- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [SQL Server Connection Strings](https://www.connectionstrings.com/sql-server/)
- [EF Core SQL Server Provider](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/)
