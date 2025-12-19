using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tbl_AlertSummary",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PatientID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AlertDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    AlertType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlertCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlertSeverity = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AlertMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlertSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalParameter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MeasuredValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NormalRange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThresholdViolated = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Acknowledged = table.Column<bool>(type: "bit", nullable: false),
                    AcknowledgeTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionTaken = table.Column<bool>(type: "bit", nullable: false),
                    ActionDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponseTimeMinutes = table.Column<int>(type: "int", nullable: true),
                    Resolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutcomeAssessment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_AlertSummary", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ComplicationAnalytics",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurrenceDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    ComplicationType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ComplicationCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PPH = table.Column<bool>(type: "bit", nullable: false),
                    PPHBloodLossMl = table.Column<int>(type: "int", nullable: true),
                    PreeclampsiaSevere = table.Column<bool>(type: "bit", nullable: false),
                    Eclampsia = table.Column<bool>(type: "bit", nullable: false),
                    Sepsis = table.Column<bool>(type: "bit", nullable: false),
                    ObstructedLabor = table.Column<bool>(type: "bit", nullable: false),
                    UterineRupture = table.Column<bool>(type: "bit", nullable: false),
                    FetalDistress = table.Column<bool>(type: "bit", nullable: false),
                    CordProlapse = table.Column<bool>(type: "bit", nullable: false),
                    ShoulderDystocia = table.Column<bool>(type: "bit", nullable: false),
                    PlacentalAbruption = table.Column<bool>(type: "bit", nullable: false),
                    DetectionMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimeToResponseMinutes = table.Column<int>(type: "int", nullable: true),
                    ManagementActions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SurgicalInterventionRequired = table.Column<bool>(type: "bit", nullable: false),
                    BloodTransfusionRequired = table.Column<bool>(type: "bit", nullable: false),
                    ICUAdmissionRequired = table.Column<bool>(type: "bit", nullable: false),
                    MaternalOutcome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FetalOutcome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Preventable = table.Column<bool>(type: "bit", nullable: false),
                    PreventionNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ComplicationAnalytics", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_DailyFacilityStats",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FacilityCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    WeekOfYear = table.Column<int>(type: "int", nullable: false),
                    TotalAdmissions = table.Column<int>(type: "int", nullable: false),
                    LaborAdmissions = table.Column<int>(type: "int", nullable: false),
                    AntenatalAdmissions = table.Column<int>(type: "int", nullable: false),
                    TotalDeliveries = table.Column<int>(type: "int", nullable: false),
                    SpontaneousVaginalDeliveries = table.Column<int>(type: "int", nullable: false),
                    AssistedVaginalDeliveries = table.Column<int>(type: "int", nullable: false),
                    CaesareanSections = table.Column<int>(type: "int", nullable: false),
                    EmergencyCaesareans = table.Column<int>(type: "int", nullable: false),
                    ElectiveCaesareans = table.Column<int>(type: "int", nullable: false),
                    LiveBirths = table.Column<int>(type: "int", nullable: false),
                    Stillbirths = table.Column<int>(type: "int", nullable: false),
                    FreshStillbirths = table.Column<int>(type: "int", nullable: false),
                    MaceratedStillbirths = table.Column<int>(type: "int", nullable: false),
                    NeonatalDeaths = table.Column<int>(type: "int", nullable: false),
                    EarlyNeonatalDeaths = table.Column<int>(type: "int", nullable: false),
                    MaternalDeaths = table.Column<int>(type: "int", nullable: false),
                    MaternalNearMiss = table.Column<int>(type: "int", nullable: false),
                    TotalReferralsIn = table.Column<int>(type: "int", nullable: false),
                    TotalReferralsOut = table.Column<int>(type: "int", nullable: false),
                    EmergencyReferrals = table.Column<int>(type: "int", nullable: false),
                    PPHCases = table.Column<int>(type: "int", nullable: false),
                    EclampsiaPreeclampsiaCases = table.Column<int>(type: "int", nullable: false),
                    ObstructedLaborCases = table.Column<int>(type: "int", nullable: false),
                    ProlongedLaborCases = table.Column<int>(type: "int", nullable: false),
                    FetalDistressCases = table.Column<int>(type: "int", nullable: false),
                    PartographsUsed = table.Column<int>(type: "int", nullable: false),
                    PartographsCompleted = table.Column<int>(type: "int", nullable: false),
                    AverageLaborDurationHours = table.Column<double>(type: "float", nullable: false),
                    TotalStaffOnDuty = table.Column<int>(type: "int", nullable: false),
                    RecordsWithCompleteData = table.Column<int>(type: "int", nullable: false),
                    DataCompletenessPercent = table.Column<double>(type: "float", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_DailyFacilityStats", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_DeliveryOutcomeSummary",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PatientID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientAge = table.Column<int>(type: "int", nullable: true),
                    Gravida = table.Column<int>(type: "int", nullable: true),
                    Parity = table.Column<int>(type: "int", nullable: true),
                    GestationalAgeWeeks = table.Column<int>(type: "int", nullable: true),
                    GestationalCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdmissionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LaborDurationHours = table.Column<double>(type: "float", nullable: true),
                    LaborStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstStagePhase = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProlongedLabor = table.Column<bool>(type: "bit", nullable: false),
                    AugmentationUsed = table.Column<bool>(type: "bit", nullable: false),
                    DeliveryMode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DeliveryModeCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstrumentUsed = table.Column<bool>(type: "bit", nullable: false),
                    InstrumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaesareanIndication = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaternalOutcome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaternalDeath = table.Column<bool>(type: "bit", nullable: false),
                    MaternalDeathCause = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PPHOccurred = table.Column<bool>(type: "bit", nullable: false),
                    EstimatedBloodLossMl = table.Column<int>(type: "int", nullable: true),
                    BloodTransfusionGiven = table.Column<bool>(type: "bit", nullable: false),
                    MaternalComplications = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfBabies = table.Column<int>(type: "int", nullable: false),
                    NeonatalOutcome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LiveBirth = table.Column<bool>(type: "bit", nullable: false),
                    Stillbirth = table.Column<bool>(type: "bit", nullable: false),
                    NeonatalDeath = table.Column<bool>(type: "bit", nullable: false),
                    BirthWeight = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    BirthWeightCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    APGAR1Min = table.Column<int>(type: "int", nullable: true),
                    APGAR5Min = table.Column<int>(type: "int", nullable: true),
                    ResuscitationRequired = table.Column<bool>(type: "bit", nullable: false),
                    AdmittedToNICU = table.Column<bool>(type: "bit", nullable: false),
                    PartographUsed = table.Column<bool>(type: "bit", nullable: false),
                    PartographComplete = table.Column<bool>(type: "bit", nullable: false),
                    ActiveManagement3rdStage = table.Column<bool>(type: "bit", nullable: false),
                    EarlyBreastfeeding = table.Column<bool>(type: "bit", nullable: false),
                    SkinToSkinContact = table.Column<bool>(type: "bit", nullable: false),
                    CompanionPresent = table.Column<bool>(type: "bit", nullable: false),
                    RiskFactors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HighRisk = table.Column<bool>(type: "bit", nullable: false),
                    WasReferred = table.Column<bool>(type: "bit", nullable: false),
                    ReferralReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferralUrgency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttendantType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryAttendantID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_DeliveryOutcomeSummary", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Facility",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    GHPostGPS = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Facility", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_FacilityPerformanceSnapshot",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FacilityCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FacilityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OverallPerformanceScore = table.Column<double>(type: "float", nullable: false),
                    QualityOfCareScore = table.Column<double>(type: "float", nullable: false),
                    SafetyScore = table.Column<double>(type: "float", nullable: false),
                    EfficiencyScore = table.Column<double>(type: "float", nullable: false),
                    PatientExperienceScore = table.Column<double>(type: "float", nullable: false),
                    CaesareanRate = table.Column<double>(type: "float", nullable: false),
                    StillbirthRate = table.Column<double>(type: "float", nullable: false),
                    NeonatalMortalityRate = table.Column<double>(type: "float", nullable: false),
                    MaternalMortalityRatio = table.Column<double>(type: "float", nullable: false),
                    PartographUtilizationRate = table.Column<double>(type: "float", nullable: false),
                    ReferralRate = table.Column<double>(type: "float", nullable: false),
                    WHOComplianceScore = table.Column<double>(type: "float", nullable: false),
                    TargetsMet = table.Column<int>(type: "int", nullable: false),
                    TotalTargets = table.Column<int>(type: "int", nullable: false),
                    OverallTrend = table.Column<int>(type: "int", nullable: false),
                    QualityTrend = table.Column<int>(type: "int", nullable: false),
                    SafetyTrend = table.Column<int>(type: "int", nullable: false),
                    ActiveAlertsCount = table.Column<int>(type: "int", nullable: false),
                    CriticalAlerts = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecommendedActions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionalAverage = table.Column<double>(type: "float", nullable: false),
                    NationalAverage = table.Column<double>(type: "float", nullable: false),
                    RankInRegion = table.Column<int>(type: "int", nullable: false),
                    TotalFacilitiesInRegion = table.Column<int>(type: "int", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_FacilityPerformanceSnapshot", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_LaborProgressAnalytics",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LaborStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    TotalLaborDurationHours = table.Column<double>(type: "float", nullable: true),
                    FirstStageDurationHours = table.Column<double>(type: "float", nullable: true),
                    LatentPhaseDurationHours = table.Column<double>(type: "float", nullable: true),
                    ActivePhaseDurationHours = table.Column<double>(type: "float", nullable: true),
                    SecondStageDurationMinutes = table.Column<double>(type: "float", nullable: true),
                    ThirdStageDurationMinutes = table.Column<double>(type: "float", nullable: true),
                    LaborProgressPattern = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CrossedActionLine = table.Column<bool>(type: "bit", nullable: false),
                    CrossedAlertLine = table.Column<bool>(type: "bit", nullable: false),
                    AlertLineCrossTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActionLineCrossTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InitialDilationCm = table.Column<int>(type: "int", nullable: false),
                    ActivePhaseOnsetTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CervicalDilationRateCmPerHour = table.Column<double>(type: "float", nullable: true),
                    InitialStation = table.Column<int>(type: "int", nullable: false),
                    DescentPattern = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FHRMeasurementCount = table.Column<int>(type: "int", nullable: false),
                    FHRAbnormalCount = table.Column<int>(type: "int", nullable: false),
                    AverageFHR = table.Column<double>(type: "float", nullable: true),
                    FHRDecelerations = table.Column<bool>(type: "bit", nullable: false),
                    FHRTachycardia = table.Column<bool>(type: "bit", nullable: false),
                    FHRBradycardia = table.Column<bool>(type: "bit", nullable: false),
                    ContractionMeasurementCount = table.Column<int>(type: "int", nullable: false),
                    AverageContractionFrequency = table.Column<double>(type: "float", nullable: true),
                    AverageContractionDuration = table.Column<double>(type: "float", nullable: true),
                    Tachysystole = table.Column<bool>(type: "bit", nullable: false),
                    HyperstimulationOccurred = table.Column<bool>(type: "bit", nullable: false),
                    AugmentationUsed = table.Column<bool>(type: "bit", nullable: false),
                    AugmentationStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AmniotomyPerformed = table.Column<bool>(type: "bit", nullable: false),
                    AmniotomyTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReasonForIntervention = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WHOCompliantMonitoring = table.Column<bool>(type: "bit", nullable: false),
                    MonitoringCompliancePercent = table.Column<double>(type: "float", nullable: true),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_LaborProgressAnalytics", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_MaternalMortalityRecord",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PatientID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BirthOutcomeID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeathDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Parity = table.Column<int>(type: "int", nullable: true),
                    GestationalAge = table.Column<int>(type: "int", nullable: true),
                    PrimaryCause = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecondaryCause = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContributingFactors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ICDCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceOfDeath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimingOfDeath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoursPostpartum = table.Column<int>(type: "int", nullable: true),
                    DirectObstetricCause = table.Column<bool>(type: "bit", nullable: false),
                    DirectCauseCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IndirectCauseCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Delay1SeekingCare = table.Column<bool>(type: "bit", nullable: false),
                    Delay1Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Delay2ReachingCare = table.Column<bool>(type: "bit", nullable: false),
                    Delay2Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Delay3ReceivingCare = table.Column<bool>(type: "bit", nullable: false),
                    Delay3Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreventabilityAssessment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreventableFactors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecommendedActions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MDRCompleted = table.Column<bool>(type: "bit", nullable: false),
                    MDRDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MDRFindings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_MaternalMortalityRecord", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_MonthlyFacilityStats",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FacilityCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    MonthName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalDeliveries = table.Column<int>(type: "int", nullable: false),
                    SVDCount = table.Column<int>(type: "int", nullable: false),
                    AssistedDeliveryCount = table.Column<int>(type: "int", nullable: false),
                    CaesareanCount = table.Column<int>(type: "int", nullable: false),
                    CaesareanRate = table.Column<double>(type: "float", nullable: false),
                    TotalBirths = table.Column<int>(type: "int", nullable: false),
                    LiveBirthCount = table.Column<int>(type: "int", nullable: false),
                    StillbirthCount = table.Column<int>(type: "int", nullable: false),
                    StillbirthRate = table.Column<double>(type: "float", nullable: false),
                    NeonatalDeathCount = table.Column<int>(type: "int", nullable: false),
                    NeonatalMortalityRate = table.Column<double>(type: "float", nullable: false),
                    PerinatalMortalityRate = table.Column<double>(type: "float", nullable: false),
                    MaternalDeathCount = table.Column<int>(type: "int", nullable: false),
                    MaternalMortalityRatio = table.Column<double>(type: "float", nullable: false),
                    MaternalNearMissCount = table.Column<int>(type: "int", nullable: false),
                    TotalBabiesBorn = table.Column<int>(type: "int", nullable: false),
                    LowBirthWeightCount = table.Column<int>(type: "int", nullable: false),
                    VeryLowBirthWeightCount = table.Column<int>(type: "int", nullable: false),
                    PretermBirthCount = table.Column<int>(type: "int", nullable: false),
                    AverageBirthWeight = table.Column<double>(type: "float", nullable: false),
                    LowBirthWeightRate = table.Column<double>(type: "float", nullable: false),
                    PPHCount = table.Column<int>(type: "int", nullable: false),
                    SeverePreeclampsiaCount = table.Column<int>(type: "int", nullable: false),
                    EclampsiaCount = table.Column<int>(type: "int", nullable: false),
                    SepsisCount = table.Column<int>(type: "int", nullable: false),
                    ObstructedLaborCount = table.Column<int>(type: "int", nullable: false),
                    ReferralsInCount = table.Column<int>(type: "int", nullable: false),
                    ReferralsOutCount = table.Column<int>(type: "int", nullable: false),
                    ReferralRate = table.Column<double>(type: "float", nullable: false),
                    PartographUtilizationRate = table.Column<double>(type: "float", nullable: false),
                    ActiveManagement3rdStageRate = table.Column<double>(type: "float", nullable: false),
                    EarlyBreastfeedingRate = table.Column<double>(type: "float", nullable: false),
                    SkinToSkinRate = table.Column<double>(type: "float", nullable: false),
                    APGARScoreLessThan7At5MinCount = table.Column<int>(type: "int", nullable: false),
                    ResuscitationRate = table.Column<double>(type: "float", nullable: false),
                    CaesareanRateTarget = table.Column<double>(type: "float", nullable: false),
                    CaesareanRateWithinTarget = table.Column<bool>(type: "bit", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_MonthlyFacilityStats", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_NeonatalOutcomeRecord",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BabyDetailsID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BirthDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    OutcomeType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DeathDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HoursAfterBirth = table.Column<int>(type: "int", nullable: true),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BirthWeight = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    BirthWeightCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GestationalAge = table.Column<int>(type: "int", nullable: true),
                    GestationalCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    APGAR1 = table.Column<int>(type: "int", nullable: true),
                    APGAR5 = table.Column<int>(type: "int", nullable: true),
                    LowAPGAR = table.Column<bool>(type: "bit", nullable: false),
                    PrimaryCause = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ICDCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CauseCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProlongedLabor = table.Column<bool>(type: "bit", nullable: false),
                    MeconiumStained = table.Column<bool>(type: "bit", nullable: false),
                    CordProlapse = table.Column<bool>(type: "bit", nullable: false),
                    ShoulderDystocia = table.Column<bool>(type: "bit", nullable: false),
                    ResuscitationAttempted = table.Column<bool>(type: "bit", nullable: false),
                    ResuscitationSteps = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResuscitationDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    DelayedCordClamping = table.Column<bool>(type: "bit", nullable: false),
                    ImmediateSkinToSkin = table.Column<bool>(type: "bit", nullable: false),
                    EarlyBreastfeeding = table.Column<bool>(type: "bit", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_NeonatalOutcomeRecord", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Patient",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HospitalNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    BloodGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmergencyContactRelationship = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: true),
                    Height = table.Column<double>(type: "float", nullable: true),
                    HasPreviousCSection = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfPreviousCsections = table.Column<int>(type: "int", nullable: true),
                    LiveBirths = table.Column<int>(type: "int", nullable: true),
                    Stillbirths = table.Column<int>(type: "int", nullable: true),
                    NeonatalDeaths = table.Column<int>(type: "int", nullable: true),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Patient", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_ReferralAnalytics",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferralID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferralDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    SourceFacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceFacilityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceFacilityLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceRegion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinationFacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DestinationFacilityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinationFacilityLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinationRegion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferralType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Urgency = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PrimaryReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecondaryReasons = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransportMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransportDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    SkilledAttendantAccompanied = table.Column<bool>(type: "bit", nullable: false),
                    PreReferralInterventions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IVLineEstablished = table.Column<bool>(type: "bit", nullable: false),
                    PartographSent = table.Column<bool>(type: "bit", nullable: false),
                    ReferralStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutcomeAtDestination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaternalOutcome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NeonatalOutcome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DecisionToReferralMinutes = table.Column<int>(type: "int", nullable: true),
                    DestinationNotifiedBeforeArrival = table.Column<bool>(type: "bit", nullable: false),
                    FeedbackReceived = table.Column<bool>(type: "bit", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_ReferralAnalytics", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Staff",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StaffID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Facility = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Staff", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Partograph",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gravida = table.Column<int>(type: "int", nullable: false),
                    Parity = table.Column<int>(type: "int", nullable: false),
                    Abortion = table.Column<int>(type: "int", nullable: false),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LastMenstrualDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CurrentPhase = table.Column<int>(type: "int", nullable: false),
                    LaborStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SecondStageStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThirdStageStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FourthStageStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RupturedMembraneTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CervicalDilationOnAdmission = table.Column<int>(type: "int", nullable: true),
                    MembraneStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LiquorStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskScore = table.Column<int>(type: "int", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Complications = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Partograph", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_Partograph_Tbl_Patient_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Tbl_Patient",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_AmnioticFluid",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MeconiumStaining = table.Column<bool>(type: "bit", nullable: false),
                    MeconiumGrade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Consistency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Odor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RuptureStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RuptureTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RuptureMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RuptureLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfirmedRupture = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmationMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FluidVolume = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedVolumeMl = table.Column<int>(type: "int", nullable: true),
                    PoolingInVagina = table.Column<bool>(type: "bit", nullable: false),
                    MeconiumFirstNotedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MeconiumThickParticulate = table.Column<bool>(type: "bit", nullable: false),
                    NeonatalTeamAlerted = table.Column<bool>(type: "bit", nullable: false),
                    NeonatalTeamAlertTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProlongedRupture = table.Column<bool>(type: "bit", nullable: false),
                    HoursSinceRupture = table.Column<int>(type: "int", nullable: true),
                    MaternalFever = table.Column<bool>(type: "bit", nullable: false),
                    MaternalTachycardia = table.Column<bool>(type: "bit", nullable: false),
                    FetalTachycardia = table.Column<bool>(type: "bit", nullable: false),
                    UterineTenderness = table.Column<bool>(type: "bit", nullable: false),
                    BloodSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActiveBleeding = table.Column<bool>(type: "bit", nullable: false),
                    BleedingAmount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CordProlapse = table.Column<bool>(type: "bit", nullable: false),
                    CordPresentation = table.Column<bool>(type: "bit", nullable: false),
                    CordComplicationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AntibioticsIndicated = table.Column<bool>(type: "bit", nullable: false),
                    AmnioinfusionConsidered = table.Column<bool>(type: "bit", nullable: false),
                    ExpeditedDeliveryNeeded = table.Column<bool>(type: "bit", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_AmnioticFluid", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_AmnioticFluid_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Assessment",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaborProgress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LaborPhase = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FetalWellbeing = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaternalCondition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskFactors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Complications = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedDelivery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiresIntervention = table.Column<bool>(type: "bit", nullable: false),
                    InterventionRequired = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrgencyLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeniorReviewRequired = table.Column<bool>(type: "bit", nullable: false),
                    ConsultantInformed = table.Column<bool>(type: "bit", nullable: false),
                    AssessmentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextAssessment = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClinicalDecision = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Assessment", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_Assessment_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_BirthOutcome",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecordedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaternalStatus = table.Column<int>(type: "int", nullable: false),
                    MaternalDeathTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaternalDeathCause = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaternalDeathCircumstances = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryMode = table.Column<int>(type: "int", nullable: false),
                    DeliveryModeDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumberOfBabies = table.Column<int>(type: "int", nullable: false),
                    PerinealStatus = table.Column<int>(type: "int", nullable: false),
                    PerinealDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlacentaDeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlacentaComplete = table.Column<bool>(type: "bit", nullable: false),
                    EstimatedBloodLoss = table.Column<int>(type: "int", nullable: false),
                    MaternalComplications = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostpartumHemorrhage = table.Column<bool>(type: "bit", nullable: false),
                    Eclampsia = table.Column<bool>(type: "bit", nullable: false),
                    SepticShock = table.Column<bool>(type: "bit", nullable: false),
                    ObstructedLabor = table.Column<bool>(type: "bit", nullable: false),
                    RupturedUterus = table.Column<bool>(type: "bit", nullable: false),
                    OxytocinGiven = table.Column<bool>(type: "bit", nullable: false),
                    AntibioticsGiven = table.Column<bool>(type: "bit", nullable: false),
                    BloodTransfusionGiven = table.Column<bool>(type: "bit", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OxytocinGivenPostDelivery = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_BirthOutcome", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_BirthOutcome_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_BishopScore",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Dilation = table.Column<int>(type: "int", nullable: false),
                    Effacement = table.Column<int>(type: "int", nullable: false),
                    Consistency = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Station = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    DilationCm = table.Column<int>(type: "int", nullable: true),
                    EffacementPercent = table.Column<int>(type: "int", nullable: true),
                    CervicalConsistency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CervicalPosition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StationValue = table.Column<int>(type: "int", nullable: true),
                    Interpretation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FavorableForDelivery = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_BishopScore", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_BishopScore_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_BP",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Systolic = table.Column<int>(type: "int", nullable: false),
                    Diastolic = table.Column<int>(type: "int", nullable: false),
                    Pulse = table.Column<int>(type: "int", nullable: false),
                    MaternalPosition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CuffSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepeatMeasurement = table.Column<bool>(type: "bit", nullable: false),
                    IrregularPulse = table.Column<bool>(type: "bit", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BPCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SevereHypertension = table.Column<bool>(type: "bit", nullable: false),
                    PreeclampsiaRange = table.Column<bool>(type: "bit", nullable: false),
                    FirstElevatedBPTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsecutiveElevatedReadings = table.Column<int>(type: "int", nullable: true),
                    SecondSystolic = table.Column<int>(type: "int", nullable: true),
                    SecondDiastolic = table.Column<int>(type: "int", nullable: true),
                    SecondReadingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThirdSystolic = table.Column<int>(type: "int", nullable: true),
                    ThirdDiastolic = table.Column<int>(type: "int", nullable: true),
                    ThirdReadingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PulseRhythm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PulseVolume = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PulseCharacter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PulseDeficit = table.Column<bool>(type: "bit", nullable: false),
                    Hypotension = table.Column<bool>(type: "bit", nullable: false),
                    HypotensionCause = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PosturalHypotension = table.Column<bool>(type: "bit", nullable: false),
                    PosturalDrop = table.Column<int>(type: "int", nullable: true),
                    NewOnsetHypertension = table.Column<bool>(type: "bit", nullable: false),
                    KnownHypertension = table.Column<bool>(type: "bit", nullable: false),
                    OnAntihypertensives = table.Column<bool>(type: "bit", nullable: false),
                    AntihypertensiveMedication = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastAntihypertensiveDose = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Headache = table.Column<bool>(type: "bit", nullable: false),
                    VisualDisturbances = table.Column<bool>(type: "bit", nullable: false),
                    EpigastricPain = table.Column<bool>(type: "bit", nullable: false),
                    Hyperreflexia = table.Column<bool>(type: "bit", nullable: false),
                    Edema = table.Column<bool>(type: "bit", nullable: false),
                    EmergencyProtocolActivated = table.Column<bool>(type: "bit", nullable: false),
                    AntihypertensiveGiven = table.Column<bool>(type: "bit", nullable: false),
                    AntihypertensiveGivenTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MagnesiumSulfateGiven = table.Column<bool>(type: "bit", nullable: false),
                    IVFluidsGiven = table.Column<bool>(type: "bit", nullable: false),
                    PositionChanged = table.Column<bool>(type: "bit", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_BP", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_BP_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Caput",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Degree = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Consistency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Increasing = table.Column<bool>(type: "bit", nullable: false),
                    Decreasing = table.Column<bool>(type: "bit", nullable: false),
                    Stable = table.Column<bool>(type: "bit", nullable: false),
                    ProgressionRate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstDetectedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationHours = table.Column<int>(type: "int", nullable: true),
                    MouldingPresent = table.Column<bool>(type: "bit", nullable: false),
                    MouldingDegree = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SuggestsObstruction = table.Column<bool>(type: "bit", nullable: false),
                    SuggestionProlongedLabor = table.Column<bool>(type: "bit", nullable: false),
                    ChangeFromPrevious = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Caput", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_Caput_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_CervixDilatation",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DilatationCm = table.Column<int>(type: "int", nullable: false),
                    EffacementPercent = table.Column<int>(type: "int", nullable: false),
                    Consistency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationToHead = table.Column<bool>(type: "bit", nullable: false),
                    CervicalEdema = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MembraneStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CervicalLip = table.Column<bool>(type: "bit", nullable: false),
                    DilatationRateCmPerHour = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ProgressionRate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CrossedActionLine = table.Column<bool>(type: "bit", nullable: false),
                    CrossedAlertLine = table.Column<bool>(type: "bit", nullable: false),
                    ActionLineCrossedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CervicalLengthCm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ExaminerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExamDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    DifficultExam = table.Column<bool>(type: "bit", nullable: false),
                    ExamDifficulty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CervicalThickness = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnteriorCervicalLip = table.Column<bool>(type: "bit", nullable: false),
                    PosteriorCervicalLip = table.Column<bool>(type: "bit", nullable: false),
                    CervicalDilatationPattern = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StationRelativeToPelvicSpines = table.Column<int>(type: "int", nullable: true),
                    PresentingPartPosition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PresentingPartWellApplied = table.Column<bool>(type: "bit", nullable: false),
                    MembranesBulging = table.Column<bool>(type: "bit", nullable: false),
                    ForewatersPresent = table.Column<bool>(type: "bit", nullable: false),
                    HindwatersPresent = table.Column<bool>(type: "bit", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProlongedLatentPhase = table.Column<bool>(type: "bit", nullable: false),
                    ProtractedActivePhase = table.Column<bool>(type: "bit", nullable: false),
                    ArrestedDilatation = table.Column<bool>(type: "bit", nullable: false),
                    PrecipitousLabor = table.Column<bool>(type: "bit", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_CervixDilatation", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_CervixDilatation_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Companion",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Companion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanionPresent = table.Column<bool>(type: "bit", nullable: false),
                    CompanionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfCompanions = table.Column<int>(type: "int", nullable: false),
                    CompanionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanionRelationship = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    ContinuousPresence = table.Column<bool>(type: "bit", nullable: false),
                    ParticipationLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportActivities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientRequestedCompanion = table.Column<bool>(type: "bit", nullable: false),
                    PatientDeclinedCompanion = table.Column<bool>(type: "bit", nullable: false),
                    ReasonForNoCompanion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StaffOrientedCompanion = table.Column<bool>(type: "bit", nullable: false),
                    CompanionInvolvedInDecisions = table.Column<bool>(type: "bit", nullable: false),
                    LanguageBarrier = table.Column<bool>(type: "bit", nullable: false),
                    InterpreterRequired = table.Column<bool>(type: "bit", nullable: false),
                    CulturalPractices = table.Column<bool>(type: "bit", nullable: false),
                    CulturalPracticesDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Companion", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_Companion_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Contraction",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FrequencyPer10Min = table.Column<int>(type: "int", nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    Strength = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Regularity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PalpableAtRest = table.Column<bool>(type: "bit", nullable: false),
                    Coordinated = table.Column<bool>(type: "bit", nullable: false),
                    EffectOnCervix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntensityMmHg = table.Column<int>(type: "int", nullable: true),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContractionPattern = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tachysystole = table.Column<bool>(type: "bit", nullable: false),
                    Hyperstimulation = table.Column<bool>(type: "bit", nullable: false),
                    TachysystoleStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TachysystoleDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    IntensityAssessment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IndentableDuringContraction = table.Column<bool>(type: "bit", nullable: false),
                    UterusRelaxesBetweenContractions = table.Column<bool>(type: "bit", nullable: false),
                    RelaxationTimeSeconds = table.Column<int>(type: "int", nullable: true),
                    RestingToneMmHg = table.Column<int>(type: "int", nullable: true),
                    PeakPressureMmHg = table.Column<int>(type: "int", nullable: true),
                    MontevideUnits = table.Column<int>(type: "int", nullable: true),
                    ShortestDurationSeconds = table.Column<int>(type: "int", nullable: true),
                    LongestDurationSeconds = table.Column<int>(type: "int", nullable: true),
                    AverageDurationSeconds = table.Column<int>(type: "int", nullable: true),
                    ProlongedContractions = table.Column<bool>(type: "bit", nullable: false),
                    ProlongedContractionCount = table.Column<int>(type: "int", nullable: true),
                    FrequencyTrend = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IrregularFrequency = table.Column<bool>(type: "bit", nullable: false),
                    AverageIntervalMinutes = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MaternalCopingLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaternalExhaustion = table.Column<bool>(type: "bit", nullable: false),
                    PainLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OnOxytocin = table.Column<bool>(type: "bit", nullable: false),
                    OxytocinEffect = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OxytocinAdjustmentNeeded = table.Column<bool>(type: "bit", nullable: false),
                    SuggestedOxytocinAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterventionRequired = table.Column<bool>(type: "bit", nullable: false),
                    InterventionTaken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterventionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OxytocinStopped = table.Column<bool>(type: "bit", nullable: false),
                    OxytocinReduced = table.Column<bool>(type: "bit", nullable: false),
                    TocolyticsGiven = table.Column<bool>(type: "bit", nullable: false),
                    HypertonicUterus = table.Column<bool>(type: "bit", nullable: false),
                    UterineRuptureRisk = table.Column<bool>(type: "bit", nullable: false),
                    FHRCompromise = table.Column<bool>(type: "bit", nullable: false),
                    EmergencyConsultRequired = table.Column<bool>(type: "bit", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Contraction", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_Contraction_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_FetalPosition",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Lie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Presentation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PresentingPart = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Variety = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Flexion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Engaged = table.Column<bool>(type: "bit", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssessmentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RotationProgress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_FetalPosition", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_FetalPosition_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_FHR",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rate = table.Column<int>(type: "int", nullable: true),
                    Deceleration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DecelerationDurationSeconds = table.Column<int>(type: "int", nullable: false),
                    Variability = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accelerations = table.Column<bool>(type: "bit", nullable: false),
                    Pattern = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonitoringMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaselineRate = table.Column<int>(type: "int", nullable: true),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VariabilityBpm = table.Column<int>(type: "int", nullable: true),
                    VariabilityTrend = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SinusoidalPattern = table.Column<bool>(type: "bit", nullable: false),
                    SaltatorPattern = table.Column<bool>(type: "bit", nullable: false),
                    AccelerationCount = table.Column<int>(type: "int", nullable: true),
                    AccelerationPeakBpm = table.Column<int>(type: "int", nullable: true),
                    AccelerationDurationSeconds = table.Column<int>(type: "int", nullable: true),
                    DecelerationNadirBpm = table.Column<int>(type: "int", nullable: true),
                    DecelerationRecovery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DecelerationAmplitudeBpm = table.Column<int>(type: "int", nullable: true),
                    DecelerationTiming = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProlongedBradycardia = table.Column<bool>(type: "bit", nullable: false),
                    BradycardiaStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BradycardiaDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    Tachycardia = table.Column<bool>(type: "bit", nullable: false),
                    TachycardiaStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TachycardiaDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    CTGClassification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReactiveNST = table.Column<bool>(type: "bit", nullable: false),
                    LastReactiveTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaternalPosition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DuringContraction = table.Column<bool>(type: "bit", nullable: false),
                    BetweenContractions = table.Column<bool>(type: "bit", nullable: false),
                    InterventionRequired = table.Column<bool>(type: "bit", nullable: false),
                    InterventionTaken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterventionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChangeInPosition = table.Column<bool>(type: "bit", nullable: false),
                    OxygenAdministered = table.Column<bool>(type: "bit", nullable: false),
                    IVFluidsIncreased = table.Column<bool>(type: "bit", nullable: false),
                    EmergencyConsultRequired = table.Column<bool>(type: "bit", nullable: false),
                    ConsultReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsultTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PrepareForEmergencyDelivery = table.Column<bool>(type: "bit", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_FHR", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_FHR_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_FourthStageVitals",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FundalHeight = table.Column<int>(type: "int", nullable: false),
                    FundalHeightNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BleedingStatus = table.Column<int>(type: "int", nullable: false),
                    EstimatedBloodLossMl = table.Column<int>(type: "int", nullable: true),
                    ClotsPresent = table.Column<bool>(type: "bit", nullable: false),
                    BleedingNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UterineStatus = table.Column<int>(type: "int", nullable: false),
                    UterineMassage = table.Column<bool>(type: "bit", nullable: false),
                    UterineNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BladderStatus = table.Column<int>(type: "int", nullable: false),
                    CatheterizationRequired = table.Column<bool>(type: "bit", nullable: false),
                    BladderNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PPHRisk = table.Column<bool>(type: "bit", nullable: false),
                    PPHProtocolActivated = table.Column<bool>(type: "bit", nullable: false),
                    UterotonicGiven = table.Column<bool>(type: "bit", nullable: false),
                    UterotonicType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UterotonicTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PerinealPainControlled = table.Column<bool>(type: "bit", nullable: false),
                    PerinealSwelling = table.Column<bool>(type: "bit", nullable: false),
                    PerinealHematoma = table.Column<bool>(type: "bit", nullable: false),
                    MaternalComfortable = table.Column<bool>(type: "bit", nullable: false),
                    BondingInitiated = table.Column<bool>(type: "bit", nullable: false),
                    BreastfeedingInitiated = table.Column<bool>(type: "bit", nullable: false),
                    SkinToSkinContact = table.Column<bool>(type: "bit", nullable: false),
                    AssociatedBPId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssociatedTemperatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequiresAttention = table.Column<bool>(type: "bit", nullable: false),
                    AlertMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_FourthStageVitals", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_FourthStageVitals_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_HeadDescent",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Station = table.Column<int>(type: "int", nullable: false),
                    PalpableAbdominally = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Engaged = table.Column<bool>(type: "bit", nullable: false),
                    Synclitism = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Flexion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VisibleAtIntroitus = table.Column<bool>(type: "bit", nullable: false),
                    Crowning = table.Column<bool>(type: "bit", nullable: false),
                    Rotation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescentRate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescentRegression = table.Column<bool>(type: "bit", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_HeadDescent", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_HeadDescent_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_IVFluid",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FluidType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VolumeInfused = table.Column<int>(type: "int", nullable: false),
                    RateMlPerHour = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    Additives = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditiveConcentration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditiveDose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IVSite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiteHealthy = table.Column<bool>(type: "bit", nullable: false),
                    SiteCondition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhlebitisScore = table.Column<int>(type: "int", nullable: false),
                    LastSiteAssessment = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastDressingChange = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CannelaInsertionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Indication = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RunningTotalInput = table.Column<int>(type: "int", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_IVFluid", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_IVFluid_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_MedicalNote",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoteType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsImportant = table.Column<bool>(type: "bit", nullable: false),
                    UrgencyLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WHOSection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkedMeasurableType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkedMeasurableID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LinkedMeasurableTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequiresReview = table.Column<bool>(type: "bit", nullable: false),
                    RequiresFollowUp = table.Column<bool>(type: "bit", nullable: false),
                    ReviewedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewOutcome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Escalated = table.Column<bool>(type: "bit", nullable: false),
                    EscalatedTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EscalationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EscalationReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IncludeInHandover = table.Column<bool>(type: "bit", nullable: false),
                    CommunicatedToPatient = table.Column<bool>(type: "bit", nullable: false),
                    CommunicatedToCompanion = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferenceDocument = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_MedicalNote", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_MedicalNote_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tbl_MedicalNote_Tbl_Patient_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Tbl_Patient",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Medication",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Route = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdministrationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdministeredBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WitnessedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Indication = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrescribedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdverseReaction = table.Column<bool>(type: "bit", nullable: false),
                    AdverseReactionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdverseReactionSeverity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdverseReactionDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefusedByPatient = table.Column<bool>(type: "bit", nullable: false),
                    RefusalReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Medication", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_Medication_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Moulding",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Degree = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SuturesOverlapping = table.Column<bool>(type: "bit", nullable: false),
                    Reducible = table.Column<bool>(type: "bit", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SagittalSuture = table.Column<bool>(type: "bit", nullable: false),
                    CoronalSuture = table.Column<bool>(type: "bit", nullable: false),
                    LambdoidSuture = table.Column<bool>(type: "bit", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Increasing = table.Column<bool>(type: "bit", nullable: false),
                    Reducing = table.Column<bool>(type: "bit", nullable: false),
                    Stable = table.Column<bool>(type: "bit", nullable: false),
                    ProgressionRate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstDetectedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationHours = table.Column<int>(type: "int", nullable: true),
                    CaputPresent = table.Column<bool>(type: "bit", nullable: false),
                    CaputDegree = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SuggestsObstruction = table.Column<bool>(type: "bit", nullable: false),
                    SuggestsCPD = table.Column<bool>(type: "bit", nullable: false),
                    ChangeFromPrevious = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Moulding", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_Moulding_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_OralFluid",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OralFluid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FluidType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AmountMl = table.Column<int>(type: "int", nullable: false),
                    RunningTotalOralIntake = table.Column<int>(type: "int", nullable: false),
                    Tolerated = table.Column<bool>(type: "bit", nullable: false),
                    Vomiting = table.Column<bool>(type: "bit", nullable: false),
                    Nausea = table.Column<bool>(type: "bit", nullable: false),
                    VomitingEpisodes = table.Column<int>(type: "int", nullable: true),
                    VomitContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FoodOffered = table.Column<bool>(type: "bit", nullable: false),
                    FoodConsumed = table.Column<bool>(type: "bit", nullable: false),
                    FoodType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NBM = table.Column<bool>(type: "bit", nullable: false),
                    NBMReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Restrictions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RestrictionReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientRequestedFluids = table.Column<bool>(type: "bit", nullable: false),
                    PatientDeclinedFluids = table.Column<bool>(type: "bit", nullable: false),
                    AspirationRiskAssessed = table.Column<bool>(type: "bit", nullable: false),
                    AspirationRiskLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_OralFluid", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_OralFluid_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Oxytocin",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InUse = table.Column<bool>(type: "bit", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StopTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DoseMUnitsPerMin = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    TotalVolumeInfused = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ConcentrationMUnitsPerMl = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    InfusionRateMlPerHour = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Indication = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContraindicationsChecked = table.Column<bool>(type: "bit", nullable: false),
                    ContraindicationsPresent = table.Column<bool>(type: "bit", nullable: false),
                    ContraindicationDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoseTitration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeToNextIncrease = table.Column<int>(type: "int", nullable: false),
                    MaxDoseReached = table.Column<bool>(type: "bit", nullable: false),
                    StoppedReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Oxytocin", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_Oxytocin_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_PainReliefEntry",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PainRelief = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PainScoreBefore = table.Column<int>(type: "int", nullable: true),
                    PainScoreAfter = table.Column<int>(type: "int", nullable: true),
                    PainAssessmentTool = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PainReliefMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NonPharmacologicalMethods = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdministeredTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdministeredBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Route = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Effectiveness = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeToEffectMinutes = table.Column<int>(type: "int", nullable: true),
                    DurationOfEffectHours = table.Column<int>(type: "int", nullable: true),
                    SideEffects = table.Column<bool>(type: "bit", nullable: false),
                    SideEffectsDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContinuousMonitoringRequired = table.Column<bool>(type: "bit", nullable: false),
                    BladderCareRequired = table.Column<bool>(type: "bit", nullable: false),
                    LastTopUpTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TopUpCount = table.Column<int>(type: "int", nullable: false),
                    ContraindicationsChecked = table.Column<bool>(type: "bit", nullable: false),
                    ContraindicationsPresent = table.Column<bool>(type: "bit", nullable: false),
                    ContraindicationDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InformedConsentObtained = table.Column<bool>(type: "bit", nullable: false),
                    PatientPreference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_PainReliefEntry", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_PainReliefEntry_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_PartographDiagnosis",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiagnosisType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ICDCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ICDDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OnsetTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationHours = table.Column<int>(type: "int", nullable: true),
                    OnsetType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalEvidence = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportingFindings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkedMeasurableIDs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkedMeasurableTypes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiagnosedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiagnosedByRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfidenceLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManagementPlan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManagementAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiresEscalation = table.Column<bool>(type: "bit", nullable: false),
                    EscalatedTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EscalationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequiresReview = table.Column<bool>(type: "bit", nullable: false),
                    ReviewTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewOutcome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResolvedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiscussedWithPatient = table.Column<bool>(type: "bit", nullable: false),
                    DiscussedWithCompanion = table.Column<bool>(type: "bit", nullable: false),
                    PatientUnderstanding = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_PartographDiagnosis", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_PartographDiagnosis_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_PartographRiskFactor",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_PartographRiskFactor", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_PartographRiskFactor_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Plan",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagementPlan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedDeliveryMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterventionsPlanned = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PainReliefPlan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AugmentationPlan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContinuousMonitoringRequired = table.Column<bool>(type: "bit", nullable: false),
                    NextReviewTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransferRequired = table.Column<bool>(type: "bit", nullable: false),
                    TransferDestination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamMembersInvolved = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsentRequired = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientInformed = table.Column<bool>(type: "bit", nullable: false),
                    PatientAgreement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientConcerns = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Plan", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_Plan_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Posture",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Posture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostureCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EffectOnLabor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EffectOnPain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EffectOnContractions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientChoice = table.Column<bool>(type: "bit", nullable: false),
                    MedicallyIndicated = table.Column<bool>(type: "bit", nullable: false),
                    MobileAndActive = table.Column<bool>(type: "bit", nullable: false),
                    RestrictedMobility = table.Column<bool>(type: "bit", nullable: false),
                    MobilityRestriction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportEquipment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Posture", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_Posture_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Referral",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferralTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferralType = table.Column<int>(type: "int", nullable: false),
                    Urgency = table.Column<int>(type: "int", nullable: false),
                    ReferringFacilityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferringFacilityLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferringPhysician = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferringPhysicianContact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinationFacilityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinationFacilityLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinationFacilityContact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinationAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinationNotified = table.Column<bool>(type: "bit", nullable: false),
                    DestinationNotificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DestinationContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProlongedLabor = table.Column<bool>(type: "bit", nullable: false),
                    ObstructedLabor = table.Column<bool>(type: "bit", nullable: false),
                    FoetalDistress = table.Column<bool>(type: "bit", nullable: false),
                    AntepartumHemorrhage = table.Column<bool>(type: "bit", nullable: false),
                    PostpartumHemorrhage = table.Column<bool>(type: "bit", nullable: false),
                    SeverePreeclampsia = table.Column<bool>(type: "bit", nullable: false),
                    Eclampsia = table.Column<bool>(type: "bit", nullable: false),
                    SepticShock = table.Column<bool>(type: "bit", nullable: false),
                    RupturedUterus = table.Column<bool>(type: "bit", nullable: false),
                    AbnormalPresentation = table.Column<bool>(type: "bit", nullable: false),
                    CordProlapse = table.Column<bool>(type: "bit", nullable: false),
                    PlacentaPrevia = table.Column<bool>(type: "bit", nullable: false),
                    PlacentalAbruption = table.Column<bool>(type: "bit", nullable: false),
                    NeonatalAsphyxia = table.Column<bool>(type: "bit", nullable: false),
                    PrematurityComplications = table.Column<bool>(type: "bit", nullable: false),
                    LowBirthWeight = table.Column<bool>(type: "bit", nullable: false),
                    RespiratoryDistress = table.Column<bool>(type: "bit", nullable: false),
                    CongenitalAbnormalities = table.Column<bool>(type: "bit", nullable: false),
                    NeonatalSepsis = table.Column<bool>(type: "bit", nullable: false),
                    BirthInjuries = table.Column<bool>(type: "bit", nullable: false),
                    LackOfResources = table.Column<bool>(type: "bit", nullable: false),
                    RequiresCaesareanSection = table.Column<bool>(type: "bit", nullable: false),
                    RequiresBloodTransfusion = table.Column<bool>(type: "bit", nullable: false),
                    RequiresSpecializedCare = table.Column<bool>(type: "bit", nullable: false),
                    OtherReasons = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryDiagnosis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalSummary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaternalCondition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaternalPulse = table.Column<int>(type: "int", nullable: true),
                    MaternalBPSystolic = table.Column<int>(type: "int", nullable: true),
                    MaternalBPDiastolic = table.Column<int>(type: "int", nullable: true),
                    MaternalTemperature = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                    MaternalConsciousness = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FetalHeartRate = table.Column<int>(type: "int", nullable: true),
                    FetalCondition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfBabiesBeingReferred = table.Column<int>(type: "int", nullable: true),
                    NeonatalCondition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CervicalDilationAtReferral = table.Column<int>(type: "int", nullable: true),
                    MembranesRuptured = table.Column<bool>(type: "bit", nullable: false),
                    MembraneRuptureTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LiquorColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterventionsPerformed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicationsGiven = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IVFluidsGiven = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BloodSamplesTaken = table.Column<bool>(type: "bit", nullable: false),
                    InvestigationsPerformed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransportMode = table.Column<int>(type: "int", nullable: false),
                    TransportDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SkillfulAttendantAccompanying = table.Column<bool>(type: "bit", nullable: false),
                    AccompanyingStaffName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccompanyingStaffDesignation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartographSent = table.Column<bool>(type: "bit", nullable: false),
                    IVLineInsitu = table.Column<bool>(type: "bit", nullable: false),
                    CatheterInsitu = table.Column<bool>(type: "bit", nullable: false),
                    OxygenProvided = table.Column<bool>(type: "bit", nullable: false),
                    EquipmentSent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AcceptedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OutcomeNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeedbackReceived = table.Column<bool>(type: "bit", nullable: false),
                    FeedbackDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferralLetterPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferralFormGenerated = table.Column<bool>(type: "bit", nullable: false),
                    FormGenerationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Referral", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_Referral_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Temperature",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemperatureCelsius = table.Column<float>(type: "real", nullable: false),
                    MeasurementSite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeverDurationHours = table.Column<int>(type: "int", nullable: true),
                    ChillsPresent = table.Column<bool>(type: "bit", nullable: false),
                    AssociatedSymptoms = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepeatedMeasurement = table.Column<bool>(type: "bit", nullable: false),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeverCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntrapartumFever = table.Column<bool>(type: "bit", nullable: false),
                    FeverOnsetTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PeakTemperature = table.Column<float>(type: "real", nullable: true),
                    PeakTemperatureTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SecondTemperature = table.Column<float>(type: "real", nullable: true),
                    SecondReadingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThirdTemperature = table.Column<float>(type: "real", nullable: true),
                    ThirdReadingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChoriamnionitisRisk = table.Column<bool>(type: "bit", nullable: false),
                    ProlongedRupture = table.Column<bool>(type: "bit", nullable: false),
                    HoursSinceRupture = table.Column<int>(type: "int", nullable: true),
                    MaternalTachycardia = table.Column<bool>(type: "bit", nullable: false),
                    FetalTachycardia = table.Column<bool>(type: "bit", nullable: false),
                    UterineTenderness = table.Column<bool>(type: "bit", nullable: false),
                    OffensiveLiquor = table.Column<bool>(type: "bit", nullable: false),
                    RigorPresent = table.Column<bool>(type: "bit", nullable: false),
                    Sweating = table.Column<bool>(type: "bit", nullable: false),
                    Headache = table.Column<bool>(type: "bit", nullable: false),
                    MyalgiaArthralgia = table.Column<bool>(type: "bit", nullable: false),
                    SepsisScreeningDone = table.Column<bool>(type: "bit", nullable: false),
                    SepsisScreeningTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SepsisRiskLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QSOFAPositive = table.Column<bool>(type: "bit", nullable: false),
                    QSOFAScore = table.Column<int>(type: "int", nullable: true),
                    AntipyreticsGiven = table.Column<bool>(type: "bit", nullable: false),
                    AntipyreticType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AntipyreticGivenTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CulturesObtained = table.Column<bool>(type: "bit", nullable: false),
                    AntibioticsStarted = table.Column<bool>(type: "bit", nullable: false),
                    AntibioticsStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IVFluidsGiven = table.Column<bool>(type: "bit", nullable: false),
                    CoolingMeasures = table.Column<bool>(type: "bit", nullable: false),
                    IncreasedMonitoring = table.Column<bool>(type: "bit", nullable: false),
                    MonitoringIntervalMinutes = table.Column<int>(type: "int", nullable: true),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Temperature", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_Temperature_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Urine",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutputMl = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Protein = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ketones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Glucose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecificGravity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VoidingMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BladderPalpable = table.Column<bool>(type: "bit", nullable: false),
                    LastVoided = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClinicalAlert = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VoidingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimeSinceLastVoidMinutes = table.Column<int>(type: "int", nullable: true),
                    CumulativeOutputMl = table.Column<int>(type: "int", nullable: true),
                    HourlyOutputRate = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Oliguria = table.Column<bool>(type: "bit", nullable: false),
                    Anuria = table.Column<bool>(type: "bit", nullable: false),
                    ConsecutiveOliguriaHours = table.Column<int>(type: "int", nullable: true),
                    Clarity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hematuria = table.Column<bool>(type: "bit", nullable: false),
                    Concentrated = table.Column<bool>(type: "bit", nullable: false),
                    Dilute = table.Column<bool>(type: "bit", nullable: false),
                    Odor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BloodDipstick = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeukocytesDipstick = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NitritesDipstick = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PHLevel = table.Column<float>(type: "real", nullable: true),
                    BladderFullness = table.Column<bool>(type: "bit", nullable: false),
                    BladderFullnessLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DifficultVoiding = table.Column<bool>(type: "bit", nullable: false),
                    UrinaryRetention = table.Column<bool>(type: "bit", nullable: false),
                    CatheterizationIndicated = table.Column<bool>(type: "bit", nullable: false),
                    LastCatheterizationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CatheterType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProteinuriaNewOnset = table.Column<bool>(type: "bit", nullable: false),
                    ProteinuriaWorsening = table.Column<bool>(type: "bit", nullable: false),
                    FirstProteinDetectedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LaboratorySampleSent = table.Column<bool>(type: "bit", nullable: false),
                    ProteinCreatinineRatio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignsOfDehydration = table.Column<bool>(type: "bit", nullable: false),
                    ProlongedLabor = table.Column<bool>(type: "bit", nullable: false),
                    IncreasedKetoneTrend = table.Column<bool>(type: "bit", nullable: false),
                    FirstKetoneDetectedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalOralIntakeMl = table.Column<int>(type: "int", nullable: true),
                    TotalIVIntakeMl = table.Column<int>(type: "int", nullable: true),
                    FluidBalanceMl = table.Column<int>(type: "int", nullable: true),
                    EncourageOralFluids = table.Column<bool>(type: "bit", nullable: false),
                    IVFluidsStarted = table.Column<bool>(type: "bit", nullable: false),
                    CatheterizationPerformed = table.Column<bool>(type: "bit", nullable: false),
                    NephrologyConsultRequired = table.Column<bool>(type: "bit", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Urine", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_Urine_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_BabyDetails",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BirthOutcomeID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BabyNumber = table.Column<int>(type: "int", nullable: false),
                    BabyTag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BirthTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sex = table.Column<int>(type: "int", nullable: false),
                    VitalStatus = table.Column<int>(type: "int", nullable: false),
                    DeathTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeathCause = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StillbirthMacerated = table.Column<bool>(type: "bit", nullable: false),
                    BirthWeight = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Length = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    HeadCircumference = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ChestCircumference = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Apgar1Min = table.Column<int>(type: "int", nullable: true),
                    Apgar5Min = table.Column<int>(type: "int", nullable: true),
                    Apgar1HeartRate = table.Column<int>(type: "int", nullable: true),
                    Apgar1RespiratoryEffort = table.Column<int>(type: "int", nullable: true),
                    Apgar1MuscleTone = table.Column<int>(type: "int", nullable: true),
                    Apgar1ReflexIrritability = table.Column<int>(type: "int", nullable: true),
                    Apgar1Color = table.Column<int>(type: "int", nullable: true),
                    Apgar5HeartRate = table.Column<int>(type: "int", nullable: true),
                    Apgar5RespiratoryEffort = table.Column<int>(type: "int", nullable: true),
                    Apgar5MuscleTone = table.Column<int>(type: "int", nullable: true),
                    Apgar5ReflexIrritability = table.Column<int>(type: "int", nullable: true),
                    Apgar5Color = table.Column<int>(type: "int", nullable: true),
                    ResuscitationRequired = table.Column<bool>(type: "bit", nullable: false),
                    ResuscitationSteps = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResuscitationDuration = table.Column<int>(type: "int", nullable: true),
                    OxygenGiven = table.Column<bool>(type: "bit", nullable: false),
                    IntubationPerformed = table.Column<bool>(type: "bit", nullable: false),
                    ChestCompressionsGiven = table.Column<bool>(type: "bit", nullable: false),
                    MedicationsGiven = table.Column<bool>(type: "bit", nullable: false),
                    MedicationDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EarlyBreastfeedingInitiated = table.Column<bool>(type: "bit", nullable: false),
                    DelayedCordClamping = table.Column<bool>(type: "bit", nullable: false),
                    CordClampingTime = table.Column<int>(type: "int", nullable: true),
                    VitaminKGiven = table.Column<bool>(type: "bit", nullable: false),
                    EyeProphylaxisGiven = table.Column<bool>(type: "bit", nullable: false),
                    HepatitisBVaccineGiven = table.Column<bool>(type: "bit", nullable: false),
                    FirstTemperature = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                    KangarooMotherCare = table.Column<bool>(type: "bit", nullable: false),
                    WeightClassification = table.Column<int>(type: "int", nullable: false),
                    GestationalClassification = table.Column<int>(type: "int", nullable: false),
                    CongenitalAbnormalitiesPresent = table.Column<bool>(type: "bit", nullable: false),
                    CongenitalAbnormalitiesDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BirthInjuriesPresent = table.Column<bool>(type: "bit", nullable: false),
                    BirthInjuriesDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiresSpecialCare = table.Column<bool>(type: "bit", nullable: false),
                    SpecialCareReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdmittedToNICU = table.Column<bool>(type: "bit", nullable: false),
                    NICUAdmissionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FeedingMethod = table.Column<int>(type: "int", nullable: false),
                    FeedingNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AsphyxiaNeonatorum = table.Column<bool>(type: "bit", nullable: false),
                    RespiratorydistressSyndrome = table.Column<bool>(type: "bit", nullable: false),
                    Sepsis = table.Column<bool>(type: "bit", nullable: false),
                    Jaundice = table.Column<bool>(type: "bit", nullable: false),
                    Hypothermia = table.Column<bool>(type: "bit", nullable: false),
                    Hypoglycemia = table.Column<bool>(type: "bit", nullable: false),
                    OtherComplications = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Handler = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImmediateCry = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_BabyDetails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_BabyDetails_Tbl_BirthOutcome_BirthOutcomeID",
                        column: x => x.BirthOutcomeID,
                        principalTable: "Tbl_BirthOutcome",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    //table.ForeignKey(
                    //    name: "FK_Tbl_BabyDetails_Tbl_Partograph_PartographID",
                    //    column: x => x.PartographID,
                    //    principalTable: "Tbl_Partograph",
                    //    principalColumn: "ID",
                    //    onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AlertSummary_AlertDateTime",
                table: "Tbl_AlertSummary",
                column: "AlertDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AlertSummary_AlertSeverity",
                table: "Tbl_AlertSummary",
                column: "AlertSeverity");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AlertSummary_FacilityID",
                table: "Tbl_AlertSummary",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AlertSummary_PartographID",
                table: "Tbl_AlertSummary",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AlertSummary_Resolved",
                table: "Tbl_AlertSummary",
                column: "Resolved");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AmnioticFluid_PartographID",
                table: "Tbl_AmnioticFluid",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AmnioticFluid_ServerVersion",
                table: "Tbl_AmnioticFluid",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AmnioticFluid_SyncStatus",
                table: "Tbl_AmnioticFluid",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AmnioticFluid_Time",
                table: "Tbl_AmnioticFluid",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_AmnioticFluid_UpdatedTime",
                table: "Tbl_AmnioticFluid",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Assessment_PartographID",
                table: "Tbl_Assessment",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Assessment_ServerVersion",
                table: "Tbl_Assessment",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Assessment_SyncStatus",
                table: "Tbl_Assessment",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Assessment_Time",
                table: "Tbl_Assessment",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Assessment_UpdatedTime",
                table: "Tbl_Assessment",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BabyDetails_BirthOutcomeID",
                table: "Tbl_BabyDetails",
                column: "BirthOutcomeID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BabyDetails_BirthTime",
                table: "Tbl_BabyDetails",
                column: "BirthTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BabyDetails_PartographID",
                table: "Tbl_BabyDetails",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BabyDetails_ServerVersion",
                table: "Tbl_BabyDetails",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BabyDetails_SyncStatus",
                table: "Tbl_BabyDetails",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BabyDetails_UpdatedTime",
                table: "Tbl_BabyDetails",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BirthOutcome_PartographID",
                table: "Tbl_BirthOutcome",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BirthOutcome_RecordedTime",
                table: "Tbl_BirthOutcome",
                column: "RecordedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BirthOutcome_ServerVersion",
                table: "Tbl_BirthOutcome",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BirthOutcome_SyncStatus",
                table: "Tbl_BirthOutcome",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BirthOutcome_UpdatedTime",
                table: "Tbl_BirthOutcome",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BishopScore_PartographID",
                table: "Tbl_BishopScore",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BishopScore_ServerVersion",
                table: "Tbl_BishopScore",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BishopScore_SyncStatus",
                table: "Tbl_BishopScore",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BishopScore_Time",
                table: "Tbl_BishopScore",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BishopScore_UpdatedTime",
                table: "Tbl_BishopScore",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BP_PartographID",
                table: "Tbl_BP",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BP_ServerVersion",
                table: "Tbl_BP",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BP_SyncStatus",
                table: "Tbl_BP",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BP_Time",
                table: "Tbl_BP",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_BP_UpdatedTime",
                table: "Tbl_BP",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Caput_PartographID",
                table: "Tbl_Caput",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Caput_ServerVersion",
                table: "Tbl_Caput",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Caput_SyncStatus",
                table: "Tbl_Caput",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Caput_Time",
                table: "Tbl_Caput",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Caput_UpdatedTime",
                table: "Tbl_Caput",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CervixDilatation_PartographID",
                table: "Tbl_CervixDilatation",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CervixDilatation_ServerVersion",
                table: "Tbl_CervixDilatation",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CervixDilatation_SyncStatus",
                table: "Tbl_CervixDilatation",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CervixDilatation_Time",
                table: "Tbl_CervixDilatation",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_CervixDilatation_UpdatedTime",
                table: "Tbl_CervixDilatation",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Companion_PartographID",
                table: "Tbl_Companion",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Companion_ServerVersion",
                table: "Tbl_Companion",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Companion_SyncStatus",
                table: "Tbl_Companion",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Companion_Time",
                table: "Tbl_Companion",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Companion_UpdatedTime",
                table: "Tbl_Companion",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ComplicationAnalytics_ComplicationType",
                table: "Tbl_ComplicationAnalytics",
                column: "ComplicationType");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ComplicationAnalytics_FacilityID",
                table: "Tbl_ComplicationAnalytics",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ComplicationAnalytics_OccurrenceDateTime",
                table: "Tbl_ComplicationAnalytics",
                column: "OccurrenceDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ComplicationAnalytics_PartographID",
                table: "Tbl_ComplicationAnalytics",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ComplicationAnalytics_Severity",
                table: "Tbl_ComplicationAnalytics",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Contraction_PartographID",
                table: "Tbl_Contraction",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Contraction_ServerVersion",
                table: "Tbl_Contraction",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Contraction_SyncStatus",
                table: "Tbl_Contraction",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Contraction_Time",
                table: "Tbl_Contraction",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Contraction_UpdatedTime",
                table: "Tbl_Contraction",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_DailyFacilityStats_Date",
                table: "Tbl_DailyFacilityStats",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_DailyFacilityStats_FacilityID",
                table: "Tbl_DailyFacilityStats",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_DailyFacilityStats_FacilityID_Date",
                table: "Tbl_DailyFacilityStats",
                columns: new[] { "FacilityID", "Date" },
                unique: true,
                filter: "[FacilityID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_DailyFacilityStats_Year_Month_Day",
                table: "Tbl_DailyFacilityStats",
                columns: new[] { "Year", "Month", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_DeliveryOutcomeSummary_DeliveryMode",
                table: "Tbl_DeliveryOutcomeSummary",
                column: "DeliveryMode");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_DeliveryOutcomeSummary_DeliveryTime",
                table: "Tbl_DeliveryOutcomeSummary",
                column: "DeliveryTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_DeliveryOutcomeSummary_FacilityID",
                table: "Tbl_DeliveryOutcomeSummary",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_DeliveryOutcomeSummary_PartographID",
                table: "Tbl_DeliveryOutcomeSummary",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_DeliveryOutcomeSummary_PatientID",
                table: "Tbl_DeliveryOutcomeSummary",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Facility_Code",
                table: "Tbl_Facility",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Facility_Region",
                table: "Tbl_Facility",
                column: "Region");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Facility_SyncStatus",
                table: "Tbl_Facility",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Facility_UpdatedTime",
                table: "Tbl_Facility",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FacilityPerformanceSnapshot_FacilityID",
                table: "Tbl_FacilityPerformanceSnapshot",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FacilityPerformanceSnapshot_PeriodType",
                table: "Tbl_FacilityPerformanceSnapshot",
                column: "PeriodType");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FacilityPerformanceSnapshot_Region",
                table: "Tbl_FacilityPerformanceSnapshot",
                column: "Region");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FacilityPerformanceSnapshot_SnapshotDate",
                table: "Tbl_FacilityPerformanceSnapshot",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FetalPosition_PartographID",
                table: "Tbl_FetalPosition",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FetalPosition_ServerVersion",
                table: "Tbl_FetalPosition",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FetalPosition_SyncStatus",
                table: "Tbl_FetalPosition",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FetalPosition_Time",
                table: "Tbl_FetalPosition",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FetalPosition_UpdatedTime",
                table: "Tbl_FetalPosition",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FHR_PartographID",
                table: "Tbl_FHR",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FHR_ServerVersion",
                table: "Tbl_FHR",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FHR_SyncStatus",
                table: "Tbl_FHR",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FHR_Time",
                table: "Tbl_FHR",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FHR_UpdatedTime",
                table: "Tbl_FHR",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FourthStageVitals_PartographID",
                table: "Tbl_FourthStageVitals",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FourthStageVitals_ServerVersion",
                table: "Tbl_FourthStageVitals",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FourthStageVitals_SyncStatus",
                table: "Tbl_FourthStageVitals",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FourthStageVitals_Time",
                table: "Tbl_FourthStageVitals",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_FourthStageVitals_UpdatedTime",
                table: "Tbl_FourthStageVitals",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_HeadDescent_PartographID",
                table: "Tbl_HeadDescent",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_HeadDescent_ServerVersion",
                table: "Tbl_HeadDescent",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_HeadDescent_SyncStatus",
                table: "Tbl_HeadDescent",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_HeadDescent_Time",
                table: "Tbl_HeadDescent",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_HeadDescent_UpdatedTime",
                table: "Tbl_HeadDescent",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_IVFluid_PartographID",
                table: "Tbl_IVFluid",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_IVFluid_ServerVersion",
                table: "Tbl_IVFluid",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_IVFluid_SyncStatus",
                table: "Tbl_IVFluid",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_IVFluid_Time",
                table: "Tbl_IVFluid",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_IVFluid_UpdatedTime",
                table: "Tbl_IVFluid",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_LaborProgressAnalytics_FacilityID",
                table: "Tbl_LaborProgressAnalytics",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_LaborProgressAnalytics_LaborProgressPattern",
                table: "Tbl_LaborProgressAnalytics",
                column: "LaborProgressPattern");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_LaborProgressAnalytics_LaborStartTime",
                table: "Tbl_LaborProgressAnalytics",
                column: "LaborStartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_LaborProgressAnalytics_PartographID",
                table: "Tbl_LaborProgressAnalytics",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MaternalMortalityRecord_DeathDateTime",
                table: "Tbl_MaternalMortalityRecord",
                column: "DeathDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MaternalMortalityRecord_DirectObstetricCause",
                table: "Tbl_MaternalMortalityRecord",
                column: "DirectObstetricCause");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MaternalMortalityRecord_FacilityID",
                table: "Tbl_MaternalMortalityRecord",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MaternalMortalityRecord_Year_Month",
                table: "Tbl_MaternalMortalityRecord",
                columns: new[] { "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MedicalNote_PartographID",
                table: "Tbl_MedicalNote",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MedicalNote_PatientID",
                table: "Tbl_MedicalNote",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MedicalNote_ServerVersion",
                table: "Tbl_MedicalNote",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MedicalNote_SyncStatus",
                table: "Tbl_MedicalNote",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MedicalNote_Time",
                table: "Tbl_MedicalNote",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MedicalNote_UpdatedTime",
                table: "Tbl_MedicalNote",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Medication_PartographID",
                table: "Tbl_Medication",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Medication_ServerVersion",
                table: "Tbl_Medication",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Medication_SyncStatus",
                table: "Tbl_Medication",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Medication_Time",
                table: "Tbl_Medication",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Medication_UpdatedTime",
                table: "Tbl_Medication",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MonthlyFacilityStats_FacilityID",
                table: "Tbl_MonthlyFacilityStats",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MonthlyFacilityStats_FacilityID_Year_Month",
                table: "Tbl_MonthlyFacilityStats",
                columns: new[] { "FacilityID", "Year", "Month" },
                unique: true,
                filter: "[FacilityID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MonthlyFacilityStats_Year_Month",
                table: "Tbl_MonthlyFacilityStats",
                columns: new[] { "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Moulding_PartographID",
                table: "Tbl_Moulding",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Moulding_ServerVersion",
                table: "Tbl_Moulding",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Moulding_SyncStatus",
                table: "Tbl_Moulding",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Moulding_Time",
                table: "Tbl_Moulding",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Moulding_UpdatedTime",
                table: "Tbl_Moulding",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_NeonatalOutcomeRecord_BirthDateTime",
                table: "Tbl_NeonatalOutcomeRecord",
                column: "BirthDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_NeonatalOutcomeRecord_FacilityID",
                table: "Tbl_NeonatalOutcomeRecord",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_NeonatalOutcomeRecord_OutcomeType",
                table: "Tbl_NeonatalOutcomeRecord",
                column: "OutcomeType");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_NeonatalOutcomeRecord_Year_Month",
                table: "Tbl_NeonatalOutcomeRecord",
                columns: new[] { "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OralFluid_PartographID",
                table: "Tbl_OralFluid",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OralFluid_ServerVersion",
                table: "Tbl_OralFluid",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OralFluid_SyncStatus",
                table: "Tbl_OralFluid",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OralFluid_Time",
                table: "Tbl_OralFluid",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_OralFluid_UpdatedTime",
                table: "Tbl_OralFluid",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Oxytocin_PartographID",
                table: "Tbl_Oxytocin",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Oxytocin_ServerVersion",
                table: "Tbl_Oxytocin",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Oxytocin_SyncStatus",
                table: "Tbl_Oxytocin",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Oxytocin_Time",
                table: "Tbl_Oxytocin",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Oxytocin_UpdatedTime",
                table: "Tbl_Oxytocin",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PainReliefEntry_PartographID",
                table: "Tbl_PainReliefEntry",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PainReliefEntry_ServerVersion",
                table: "Tbl_PainReliefEntry",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PainReliefEntry_SyncStatus",
                table: "Tbl_PainReliefEntry",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PainReliefEntry_Time",
                table: "Tbl_PainReliefEntry",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PainReliefEntry_UpdatedTime",
                table: "Tbl_PainReliefEntry",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Partograph_PatientID",
                table: "Tbl_Partograph",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Partograph_ServerVersion",
                table: "Tbl_Partograph",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Partograph_SyncStatus",
                table: "Tbl_Partograph",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Partograph_UpdatedTime",
                table: "Tbl_Partograph",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartographDiagnosis_PartographID",
                table: "Tbl_PartographDiagnosis",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartographDiagnosis_ServerVersion",
                table: "Tbl_PartographDiagnosis",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartographDiagnosis_SyncStatus",
                table: "Tbl_PartographDiagnosis",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartographDiagnosis_Time",
                table: "Tbl_PartographDiagnosis",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartographDiagnosis_UpdatedTime",
                table: "Tbl_PartographDiagnosis",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartographRiskFactor_PartographID",
                table: "Tbl_PartographRiskFactor",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartographRiskFactor_ServerVersion",
                table: "Tbl_PartographRiskFactor",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartographRiskFactor_SyncStatus",
                table: "Tbl_PartographRiskFactor",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartographRiskFactor_Time",
                table: "Tbl_PartographRiskFactor",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PartographRiskFactor_UpdatedTime",
                table: "Tbl_PartographRiskFactor",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Patient_DeviceId",
                table: "Tbl_Patient",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Patient_ServerVersion",
                table: "Tbl_Patient",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Patient_SyncStatus",
                table: "Tbl_Patient",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Patient_UpdatedTime",
                table: "Tbl_Patient",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Plan_PartographID",
                table: "Tbl_Plan",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Plan_ServerVersion",
                table: "Tbl_Plan",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Plan_SyncStatus",
                table: "Tbl_Plan",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Plan_Time",
                table: "Tbl_Plan",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Plan_UpdatedTime",
                table: "Tbl_Plan",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Posture_PartographID",
                table: "Tbl_Posture",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Posture_ServerVersion",
                table: "Tbl_Posture",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Posture_SyncStatus",
                table: "Tbl_Posture",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Posture_Time",
                table: "Tbl_Posture",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Posture_UpdatedTime",
                table: "Tbl_Posture",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Referral_PartographID",
                table: "Tbl_Referral",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Referral_ReferralTime",
                table: "Tbl_Referral",
                column: "ReferralTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Referral_ServerVersion",
                table: "Tbl_Referral",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Referral_Status",
                table: "Tbl_Referral",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Referral_SyncStatus",
                table: "Tbl_Referral",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Referral_UpdatedTime",
                table: "Tbl_Referral",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ReferralAnalytics_DestinationFacilityID",
                table: "Tbl_ReferralAnalytics",
                column: "DestinationFacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ReferralAnalytics_ReferralDateTime",
                table: "Tbl_ReferralAnalytics",
                column: "ReferralDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ReferralAnalytics_ReferralID",
                table: "Tbl_ReferralAnalytics",
                column: "ReferralID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ReferralAnalytics_SourceFacilityID",
                table: "Tbl_ReferralAnalytics",
                column: "SourceFacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_ReferralAnalytics_Urgency",
                table: "Tbl_ReferralAnalytics",
                column: "Urgency");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Staff_Email",
                table: "Tbl_Staff",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Staff_StaffID",
                table: "Tbl_Staff",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Staff_SyncStatus",
                table: "Tbl_Staff",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Staff_UpdatedTime",
                table: "Tbl_Staff",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Temperature_PartographID",
                table: "Tbl_Temperature",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Temperature_ServerVersion",
                table: "Tbl_Temperature",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Temperature_SyncStatus",
                table: "Tbl_Temperature",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Temperature_Time",
                table: "Tbl_Temperature",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Temperature_UpdatedTime",
                table: "Tbl_Temperature",
                column: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Urine_PartographID",
                table: "Tbl_Urine",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Urine_ServerVersion",
                table: "Tbl_Urine",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Urine_SyncStatus",
                table: "Tbl_Urine",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Urine_Time",
                table: "Tbl_Urine",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Urine_UpdatedTime",
                table: "Tbl_Urine",
                column: "UpdatedTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tbl_AlertSummary");

            migrationBuilder.DropTable(
                name: "Tbl_AmnioticFluid");

            migrationBuilder.DropTable(
                name: "Tbl_Assessment");

            migrationBuilder.DropTable(
                name: "Tbl_BabyDetails");

            migrationBuilder.DropTable(
                name: "Tbl_BishopScore");

            migrationBuilder.DropTable(
                name: "Tbl_BP");

            migrationBuilder.DropTable(
                name: "Tbl_Caput");

            migrationBuilder.DropTable(
                name: "Tbl_CervixDilatation");

            migrationBuilder.DropTable(
                name: "Tbl_Companion");

            migrationBuilder.DropTable(
                name: "Tbl_ComplicationAnalytics");

            migrationBuilder.DropTable(
                name: "Tbl_Contraction");

            migrationBuilder.DropTable(
                name: "Tbl_DailyFacilityStats");

            migrationBuilder.DropTable(
                name: "Tbl_DeliveryOutcomeSummary");

            migrationBuilder.DropTable(
                name: "Tbl_Facility");

            migrationBuilder.DropTable(
                name: "Tbl_FacilityPerformanceSnapshot");

            migrationBuilder.DropTable(
                name: "Tbl_FetalPosition");

            migrationBuilder.DropTable(
                name: "Tbl_FHR");

            migrationBuilder.DropTable(
                name: "Tbl_FourthStageVitals");

            migrationBuilder.DropTable(
                name: "Tbl_HeadDescent");

            migrationBuilder.DropTable(
                name: "Tbl_IVFluid");

            migrationBuilder.DropTable(
                name: "Tbl_LaborProgressAnalytics");

            migrationBuilder.DropTable(
                name: "Tbl_MaternalMortalityRecord");

            migrationBuilder.DropTable(
                name: "Tbl_MedicalNote");

            migrationBuilder.DropTable(
                name: "Tbl_Medication");

            migrationBuilder.DropTable(
                name: "Tbl_MonthlyFacilityStats");

            migrationBuilder.DropTable(
                name: "Tbl_Moulding");

            migrationBuilder.DropTable(
                name: "Tbl_NeonatalOutcomeRecord");

            migrationBuilder.DropTable(
                name: "Tbl_OralFluid");

            migrationBuilder.DropTable(
                name: "Tbl_Oxytocin");

            migrationBuilder.DropTable(
                name: "Tbl_PainReliefEntry");

            migrationBuilder.DropTable(
                name: "Tbl_PartographDiagnosis");

            migrationBuilder.DropTable(
                name: "Tbl_PartographRiskFactor");

            migrationBuilder.DropTable(
                name: "Tbl_Plan");

            migrationBuilder.DropTable(
                name: "Tbl_Posture");

            migrationBuilder.DropTable(
                name: "Tbl_Referral");

            migrationBuilder.DropTable(
                name: "Tbl_ReferralAnalytics");

            migrationBuilder.DropTable(
                name: "Tbl_Staff");

            migrationBuilder.DropTable(
                name: "Tbl_Temperature");

            migrationBuilder.DropTable(
                name: "Tbl_Urine");

            migrationBuilder.DropTable(
                name: "Tbl_BirthOutcome");

            migrationBuilder.DropTable(
                name: "Tbl_Partograph");

            migrationBuilder.DropTable(
                name: "Tbl_Patient");
        }
    }
}
