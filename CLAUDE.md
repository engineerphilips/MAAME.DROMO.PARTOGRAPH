# CLAUDE.md - AI Assistant Guide for MAAME.DROMO.PARTOGRAPH

## Project Overview

**Project Name:** MAAME.DROMO.PARTOGRAPH
**Type:** .NET MAUI Cross-Platform Mobile Application
**Domain:** Healthcare - Obstetric Partograph Tracking System
**Target Platforms:** Android, iOS, macOS Catalyst, Windows
**Framework:** .NET 9.0
**Primary Language:** C# with XAML

### Purpose
This application is a digital partograph system for monitoring and documenting labor progress in obstetric care. It tracks vital signs, cervical dilation, fetal heart rate, contractions, and other critical maternal and fetal indicators during labor.

## Repository Structure

```
MAAME.DROMO.PARTOGRAPH/
├── .git/                           # Git version control
├── .gitattributes                  # Git configuration
├── .gitignore                      # Git ignore rules
├── MAAME.DROMO.PARTOGRAPH.sln      # Visual Studio solution file
└── MAAME.DROMO.PARTOGRAPH.APP.Droid/  # Main application project
    ├── App.xaml / App.xaml.cs      # Application entry point & lifecycle
    ├── AppShell.xaml / AppShell.xaml.cs  # Shell navigation structure
    ├── MauiProgram.cs              # Dependency injection & app configuration
    ├── GlobalUsings.cs             # Global using directives
    │
    ├── Converters/                 # XAML value converters
    │   ├── InitialsConverter.cs
    │   ├── StatusColorConverter.cs
    │   ├── FHRColorConverter.cs
    │   └── [other converters]
    │
    ├── Data/                       # Data access layer (Repositories)
    │   ├── Constants.cs            # Database path & constants
    │   ├── DeviceIdentity.cs       # Device identification
    │   ├── PatientRepository.cs
    │   ├── PartographRepository.cs
    │   ├── [measurement repositories]
    │   └── SeedDataService.cs      # Sample data seeding
    │
    ├── Helper/                     # Utility classes
    │   ├── ElapseTimeCalc.cs
    │   └── [other helpers]
    │
    ├── Models/                     # Domain models & DTOs
    │   ├── Patient.cs
    │   ├── Partograph.cs
    │   ├── BasePartographMeasurement.cs
    │   ├── Enums.cs                # Shared enumerations
    │   ├── [measurement models]
    │   └── DashboardStats.cs
    │
    ├── PageModels/                 # ViewModels (MVVM pattern)
    │   ├── HomePageModel.cs
    │   ├── PatientDetailPageModel.cs
    │   ├── PartographPageModel.cs
    │   ├── Modals/                 # Modal-specific ViewModels
    │   └── [other page models]
    │
    ├── Pages/                      # XAML Views
    │   ├── HomePage.xaml
    │   ├── LoginPage.xaml
    │   ├── PartographPage.xaml
    │   ├── Modals/                 # Modal views
    │   ├── Views/                  # Reusable view components
    │   └── [other pages]
    │
    ├── Platforms/                  # Platform-specific code
    │   ├── Android/
    │   ├── iOS/
    │   ├── MacCatalyst/
    │   └── Windows/
    │
    ├── Properties/                 # Project properties
    │   └── launchSettings.json
    │
    ├── Resources/                  # App resources
    │   ├── AppIcon/                # Application icon
    │   ├── Fonts/                  # Custom fonts
    │   ├── Images/                 # Image assets
    │   ├── Raw/                    # Raw assets
    │   └── Splash/                 # Splash screen
    │
    ├── Services/                   # Business logic services
    │   ├── AuthenticationService.cs
    │   ├── IAuthenticationService.cs
    │   ├── ModalErrorHandler.cs
    │   └── IErrorHandler.cs
    │
    ├── Utilities/                  # Utility classes
    │   ├── ProjectExtensions.cs
    │   └── TaskUtilities.cs
    │
    └── MAAME.DROMO.PARTOGRAPH.APP.Droid.csproj  # Project file
```

## Architecture & Design Patterns

### MVVM (Model-View-ViewModel) Pattern
The application follows the MVVM architectural pattern:
- **Models**: Located in `Models/` directory, represent domain entities
- **Views**: Located in `Pages/` directory, XAML-based UI definitions
- **ViewModels**: Located in `PageModels/` directory, contain presentation logic

### Repository Pattern
Data access is abstracted through repository classes in the `Data/` directory. Each entity has its own repository that handles:
- SQLite database operations
- Data validation
- CRUD operations
- Sync status management

### Dependency Injection
The application uses Microsoft.Extensions.DependencyInjection configured in `MauiProgram.cs`:
- **Singleton**: Repositories, services that maintain state
- **Transient**: Pages and PageModels that need fresh instances
- **TransientWithShellRoute**: Pages with navigation routes

## Key Technologies & Libraries

### Core Frameworks
- **.NET MAUI 9.0**: Cross-platform UI framework
- **Microsoft.Data.Sqlite**: SQLite database access
- **sqlite-net-pcl**: PCL SQLite wrapper
- **CommunityToolkit.Mvvm**: MVVM helpers and commands
- **CommunityToolkit.Maui**: Additional MAUI controls

### UI Components
- **Syncfusion.Maui.*** (v30.1.40): Commercial UI component suite
  - Charts, Gauges, Inputs, Picker, Popup, TabView, ProgressBar, Cards, Buttons
- **FluentIcons.Maui**: Fluent design icons
- **MauiIcons.Fluent**: Additional icon sets

### Important Notes on Dependencies
- **Syncfusion License**: Configured in MauiProgram.cs (line 17) - requires valid license
- **External Project References**:
  - `EMPEROR.COMMON` (D:\repos\source codes\SMILE\EMPEROR.COMMON\EMPEROR.COMMON.csproj)
  - `EMPEROR.PERIOD` (D:\repos\source codes\SMILE\EMPEROR.PERIOD\EMPEROR.PERIOD.csproj)
  - ⚠️ These are external dependencies that may not be available in all environments

## Database Architecture

### Database Type
SQLite database with manual schema management (no ORM migrations)

### Database Location
Defined in `Data/Constants.cs` as `Constants.DatabasePath`

### Table Naming Convention
Tables use prefix `Tbl_` (e.g., `Tbl_Patient`, `Tbl_Partograph`)

### Standard Columns (Sync Framework)
All entities include these columns for offline-first synchronization:
- `ID` (TEXT/GUID): Primary key
- `createdtime` (INTEGER): Unix timestamp in milliseconds
- `updatedtime` (INTEGER): Unix timestamp in milliseconds
- `deletedtime` (INTEGER): Soft delete timestamp
- `deviceid` (TEXT): Current device identifier
- `origindeviceid` (TEXT): Original creation device
- `syncstatus` (INTEGER): 0=pending, 1=synced, 2=conflict
- `version` (INTEGER): Local version number
- `serverversion` (INTEGER): Server version number
- `deleted` (INTEGER): Soft delete flag (0/1)
- `conflictdata` (TEXT): Conflict resolution data
- `datahash` (TEXT): Data integrity hash

### Database Triggers
Repositories use SQLite triggers for automatic timestamp management:
- `trg_[table]_insert`: Sets createdtime/updatedtime on insert
- `trg_[table]_update`: Updates updatedtime on modification

## Domain Models

### Core Entities

#### Patient
Primary entity representing a patient in labor.
- **Key Properties**: FirstName, LastName, HospitalNumber, DateOfBirth, Age, BloodGroup
- **Relations**: Has one Partograph, has many VitalSigns
- **Computed**: `Name` (full name), `HasConflict`, `NeedsSync`

#### Partograph
Represents a complete partograph record for monitoring labor.
- **Key Properties**: PatientId, AdmissionTime, MembraneStatus, GestationalAge
- **Relations**: Belongs to Patient, has many measurements

#### BasePartographMeasurement (Abstract)
Base class for time-based partograph measurements.
- **Properties**: PartographId, RecordedTime, RecordedBy, Notes
- **Derived Classes**:
  - FHR (Fetal Heart Rate)
  - Contraction
  - CervixDilatation
  - Temperature
  - BP (Blood Pressure)
  - HeadDescent
  - Moulding
  - Caput
  - AmnioticFluid
  - Urine
  - Oxytocin
  - CompanionEntry
  - PainReliefEntry
  - OralFluidEntry
  - PostureEntry

### Enumerations (Models/Enums.cs)
```csharp
CompanionType: None, No, Yes, Declined
PainReliefType: None, No, Yes, Declined
OralFluidType: None, No, Difficulty, Yes, Declined
PostureType: None, Supine, Mobile, Upright, SideLeft, SideRight
FHRDecelerationType: None, No, Early, Late, Variable
```

## Application Flow

### 1. Application Startup (App.xaml.cs)
```csharp
App.CreateWindow()
  ├─> Check authentication status (Preferences)
  ├─> If authenticated → Navigate to AppShell
  ├─> If not authenticated → Show LoginPage
  └─> OnStart() → InitializeDatabase() → SeedDataService (first run)
```

### 2. Dependency Injection Setup (MauiProgram.cs)
```
CreateMauiApp()
  ├─> Register Syncfusion license
  ├─> Configure MAUI with CommunityToolkit & Syncfusion
  ├─> Register repositories (Singleton)
  ├─> Register services (Singleton/Transient)
  ├─> Register PageModels (Singleton/Transient)
  └─> Register Pages with Shell routes
```

### 3. Authentication Flow
```
LoginPage → AuthenticationService
  ├─> Validate credentials
  ├─> Save staff info to Preferences
  ├─> Set Constants.Staff
  └─> Navigate to AppShell (main interface)
```

### 4. Navigation Structure (AppShell.xaml)
The app uses Shell navigation with tabs:
- Home/Dashboard
- Pending Patients
- Active Patients (in labor)
- Completed Patients
- Analytics/Reports
- Settings

## Development Conventions

### Naming Conventions

#### Files & Classes
- **Pages**: `[Name]Page.xaml` / `[Name]Page.xaml.cs`
- **PageModels**: `[Name]PageModel.cs`
- **Repositories**: `[Entity]Repository.cs`
- **Services**: `[Name]Service.cs` with `I[Name]Service.cs` interface
- **Converters**: `[Purpose]Converter.cs`
- **Models**: `[Entity].cs`

#### Code Style
- **PascalCase**: Classes, methods, properties, public fields
- **camelCase**: Private fields (with underscore prefix `_fieldName`)
- **UPPERCASE**: Constants

### XAML Conventions
- Use `x:Name` for elements that need code-behind access
- Bind to PageModel properties using `{Binding PropertyName}`
- Use value converters for display transformations
- Leverage Syncfusion controls for complex UI components

### Repository Pattern Implementation
```csharp
public class [Entity]Repository
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;

    // Constructor with DI
    public [Entity]Repository(ILogger<[Entity]Repository> logger) { }

    // Lazy initialization
    private async Task Init() {
        // Create table with triggers
    }

    // CRUD operations
    public async Task<[Entity]> GetByIdAsync(Guid id)
    public async Task<List<[Entity]>> GetAllAsync()
    public async Task SaveAsync([Entity] entity)
    public async Task DeleteAsync(Guid id)
}
```

## Common Development Tasks

### Adding a New Page
1. Create XAML page in `Pages/`: `NewPage.xaml`
2. Create PageModel in `PageModels/`: `NewPageModel.cs`
3. Register in `MauiProgram.cs`:
   ```csharp
   builder.Services.AddSingleton<NewPageModel>();
   builder.Services.AddTransient<NewPage>();
   // OR with route:
   builder.Services.AddTransientWithShellRoute<NewPage, NewPageModel>("routename");
   ```
4. Add route to `AppShell.xaml` if needed

### Adding a New Measurement Type
1. Create model in `Models/` inheriting from `BasePartographMeasurement`
2. Create repository in `Data/` following repository pattern
3. Create modal view in `Pages/Modals/`
4. Create modal PageModel in `PageModels/Modals/`
5. Register repository and modal in `MauiProgram.cs`
6. Add to partograph entry UI

### Working with SQLite Database
- Database path: `Constants.DatabasePath`
- Use parameterized queries to prevent SQL injection
- Include sync columns in all tables
- Create indexes for frequently queried columns
- Use triggers for automatic timestamp management
- Handle connection lifecycle properly (using/await using)

### Handling Navigation
```csharp
// Shell navigation (with routes)
await Shell.Current.GoToAsync("//routename");
await Shell.Current.GoToAsync($"routename?id={id}");

// Modal presentation
await Navigation.PushModalAsync(new MyModalPage());

// Modal dismissal
await Navigation.PopModalAsync();
```

## Building & Running

### Prerequisites
- Visual Studio 2022 17.8+ or Visual Studio Code with C# Dev Kit
- .NET 9.0 SDK
- Android SDK (for Android development)
- Xcode (for iOS/macOS development)
- Windows 10 SDK 19041+ (for Windows development)

### Build Commands
```bash
# Restore dependencies
dotnet restore

# Build for specific platform
dotnet build -f net9.0-android
dotnet build -f net9.0-ios
dotnet build -f net9.0-maccatalyst
dotnet build -f net9.0-windows10.0.19041.0

# Run on Android
dotnet build -t:Run -f net9.0-android

# Clean
dotnet clean
```

### Platform-Specific Versions
- **Android**: Minimum API 21 (Android 5.0)
- **iOS**: Minimum iOS 15.0
- **macOS Catalyst**: Minimum macOS 15.0
- **Windows**: Minimum Windows 10 build 17763

## Testing Considerations

### Test Data
- Use `SeedDataService.LoadSamplePartographData()` for sample data
- Database reinitialization: Remove "DatabaseInitialized" preference

### Authentication Testing
- Credentials stored in Preferences
- Clear preferences to test fresh login:
  ```csharp
  Preferences.Clear();
  ```

## Security Considerations

### Data Protection
- Patient data is sensitive PHI (Protected Health Information)
- Implement proper authentication and authorization
- Consider encrypting SQLite database
- Sanitize all user inputs
- Use parameterized queries for database operations

### Syncfusion License
- License key is hardcoded in MauiProgram.cs
- Consider moving to secure configuration
- Do not commit license keys to public repositories

## Known Issues & Technical Debt

### External Dependencies
- Project references to `EMPEROR.COMMON` and `EMPEROR.PERIOD` are hardcoded with absolute paths
- These may not be available in all development environments
- Consider moving to NuGet packages or relative paths

### Commented Code
- `MauiProgram.cs` contains significant commented-out code (lines 106-141)
- Should be cleaned up or removed

### Excluded Files
Multiple files are excluded from compilation in the .csproj:
- Old Category/Project/Task-related code (commented as "Keep existing repositories for compatibility during migration")
- Some modal implementations are removed but referenced

### Migration Status
- Application appears to be mid-migration from a project management system to a partograph system
- Some legacy code remains (Category, Project, Task models/repositories)

## Git Workflow

### Branch Naming
- Feature branches: `claude/claude-md-[session-id]`
- Current development branch: `claude/claude-md-mi8thurpy7qdce8z-0115ZbNXJbyy5aFGZaCVtMoR`

### Commit Guidelines
- Write clear, descriptive commit messages
- Focus on "why" rather than "what"
- Use conventional commit format when possible:
  - `feat:` new features
  - `fix:` bug fixes
  - `refactor:` code refactoring
  - `docs:` documentation changes
  - `test:` test additions/changes

### Push Commands
```bash
# Always push with upstream tracking
git push -u origin <branch-name>

# Retry on network failures (exponential backoff)
# The branch MUST start with 'claude/' and end with session ID
```

## AI Assistant Guidelines

### When Making Changes
1. **Always read files before editing** - Use Read tool before Edit/Write
2. **Maintain consistency** - Follow existing patterns and conventions
3. **Respect MVVM** - Keep logic in PageModels, not code-behind
4. **Test database changes** - Verify SQLite schema modifications carefully
5. **Consider all platforms** - Changes may affect Android, iOS, macOS, and Windows

### Common Patterns to Follow
- Use CommunityToolkit.Mvvm `[ObservableProperty]` for bindable properties
- Use `[RelayCommand]` for command bindings
- Implement INotifyPropertyChanged through ObservableObject base class
- Use async/await for all database operations
- Handle exceptions with try-catch and log errors

### File Location Preferences
- **New measurements**: Add to `Models/` and create repository in `Data/`
- **New UI**: Pages in `Pages/`, ViewModels in `PageModels/`
- **Reusable logic**: Add to `Services/` or `Utilities/`
- **XAML helpers**: Add to `Converters/` if UI-specific

### Things to Avoid
- Don't modify Syncfusion license configuration
- Don't change database sync columns without understanding sync framework
- Don't remove SQL triggers without updating repository code
- Don't add large files to git (check .gitignore)
- Don't commit database files (.mdf, .ldf, .ndf)

## Useful Resources

### Documentation
- [.NET MAUI Docs](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Syncfusion MAUI Controls](https://help.syncfusion.com/maui/introduction/overview)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [SQLite Documentation](https://www.sqlite.org/docs.html)

### Code Examples
- Refer to existing repositories for patterns
- Check `SeedDataService.cs` for sample data creation
- Review `PatientRepository.cs` for complete repository implementation
- See `PartographPageModel.cs` for complex ViewModel example

## Contact & Support

For questions about this codebase:
1. Review this CLAUDE.md document
2. Check existing code for similar patterns
3. Refer to inline comments in complex sections
4. Consult the development team

---

**Last Updated**: 2025-11-21
**Document Version**: 1.0
**For**: AI assistants working with this codebase
