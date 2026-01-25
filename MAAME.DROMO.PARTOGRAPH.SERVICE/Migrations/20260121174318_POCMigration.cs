using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Migrations
{
    /// <inheritdoc />
    public partial class POCMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tbl_POCBaseline",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DistrictID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BaselinePeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaselinePeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataSource = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BaselineComplicationRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BaselinePPHCases = table.Column<int>(type: "int", nullable: false),
                    BaselineObstructedLaborCases = table.Column<int>(type: "int", nullable: false),
                    BaselineBirthAsphyxiaCases = table.Column<int>(type: "int", nullable: false),
                    BaselineTotalDeliveries = table.Column<int>(type: "int", nullable: false),
                    BaselineAverageTimeToReferralMinutes = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    BaselineTotalReferrals = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_POCBaseline", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_POCProgress",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DistrictID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DistrictName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RegionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalHealthcareWorkers = table.Column<int>(type: "int", nullable: false),
                    ActivePartographUsers = table.Column<int>(type: "int", nullable: false),
                    AdoptionRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AdoptionTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSurveyResponses = table.Column<int>(type: "int", nullable: false),
                    AverageSatisfactionScore = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    SatisfactionTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EaseOfUseAverage = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    WorkflowImpactAverage = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    PerceivedBenefitsAverage = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    TotalEmergencyCases = table.Column<int>(type: "int", nullable: false),
                    EmergenciesReportedWithin30Min = table.Column<int>(type: "int", nullable: false),
                    RealTimeReportingRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ReportingTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageReportingTimeMinutes = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    TotalDeliveries = table.Column<int>(type: "int", nullable: false),
                    TotalComplications = table.Column<int>(type: "int", nullable: false),
                    ComplicationRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BaselineComplicationRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ComplicationReductionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ComplicationReductionTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PPHCases = table.Column<int>(type: "int", nullable: false),
                    ObstructedLaborCases = table.Column<int>(type: "int", nullable: false),
                    BirthAsphyxiaCases = table.Column<int>(type: "int", nullable: false),
                    EclampsiaCases = table.Column<int>(type: "int", nullable: false),
                    OtherMajorComplications = table.Column<int>(type: "int", nullable: false),
                    TotalEmergencyReferrals = table.Column<int>(type: "int", nullable: false),
                    AverageTimeToReferralMinutes = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    BaselineTimeToReferralMinutes = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    ResponseTimeReductionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ResponseTimeReductionTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TargetsMet = table.Column<int>(type: "int", nullable: false),
                    TotalTargets = table.Column<int>(type: "int", nullable: false),
                    OverallPOCProgress = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_POCProgress", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_UserActionLog",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StaffName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StaffRole = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    ActionDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ActionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AppVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_UserActionLog", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_UserSurvey",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TargetResponseCount = table.Column<int>(type: "int", nullable: false),
                    QuestionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_UserSurvey", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_SurveyResponse",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SurveyID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacilityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StaffName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StaffRole = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FacilityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FacilityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OverallSatisfactionScore = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    EaseOfUseScore = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    WorkflowImpactScore = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    PerceivedBenefitsScore = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    TrainingAdequacyScore = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    RecommendationScore = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    AnswersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalComments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImprovementSuggestions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletionTimeSeconds = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_SurveyResponse", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_SurveyResponse_Tbl_UserSurvey_SurveyID",
                        column: x => x.SurveyID,
                        principalTable: "Tbl_UserSurvey",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_POCBaseline_DistrictID",
                table: "Tbl_POCBaseline",
                column: "DistrictID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_POCBaseline_FacilityID",
                table: "Tbl_POCBaseline",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_POCBaseline_IsApproved",
                table: "Tbl_POCBaseline",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_POCBaseline_RegionID",
                table: "Tbl_POCBaseline",
                column: "RegionID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_POCProgress_DistrictID",
                table: "Tbl_POCProgress",
                column: "DistrictID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_POCProgress_FacilityID",
                table: "Tbl_POCProgress",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_POCProgress_PeriodType",
                table: "Tbl_POCProgress",
                column: "PeriodType");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_POCProgress_RegionID",
                table: "Tbl_POCProgress",
                column: "RegionID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_POCProgress_SnapshotDate",
                table: "Tbl_POCProgress",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_POCProgress_SnapshotDate_PeriodType",
                table: "Tbl_POCProgress",
                columns: new[] { "SnapshotDate", "PeriodType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SurveyResponse_FacilityID",
                table: "Tbl_SurveyResponse",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SurveyResponse_OverallSatisfactionScore",
                table: "Tbl_SurveyResponse",
                column: "OverallSatisfactionScore");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SurveyResponse_StaffID",
                table: "Tbl_SurveyResponse",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SurveyResponse_SubmittedAt",
                table: "Tbl_SurveyResponse",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_SurveyResponse_SurveyID",
                table: "Tbl_SurveyResponse",
                column: "SurveyID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserActionLog_ActionTime",
                table: "Tbl_UserActionLog",
                column: "ActionTime");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserActionLog_ActionType",
                table: "Tbl_UserActionLog",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserActionLog_FacilityID",
                table: "Tbl_UserActionLog",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserActionLog_StaffID",
                table: "Tbl_UserActionLog",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserActionLog_StaffID_ActionTime",
                table: "Tbl_UserActionLog",
                columns: new[] { "StaffID", "ActionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserSurvey_IsActive",
                table: "Tbl_UserSurvey",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserSurvey_StartDate",
                table: "Tbl_UserSurvey",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_UserSurvey_Type",
                table: "Tbl_UserSurvey",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tbl_POCBaseline");

            migrationBuilder.DropTable(
                name: "Tbl_POCProgress");

            migrationBuilder.DropTable(
                name: "Tbl_SurveyResponse");

            migrationBuilder.DropTable(
                name: "Tbl_UserActionLog");

            migrationBuilder.DropTable(
                name: "Tbl_UserSurvey");
        }
    }
}
