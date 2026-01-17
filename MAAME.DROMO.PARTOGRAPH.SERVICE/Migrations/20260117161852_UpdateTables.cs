using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacilityID",
                table: "Tbl_Staff");

            migrationBuilder.AddColumn<Guid>(
                name: "DistrictID",
                table: "Tbl_Facility",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DistrictName",
                table: "Tbl_Facility",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Tbl_Facility",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RegionID",
                table: "Tbl_Facility",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tbl_Region",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capital = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Population = table.Column<long>(type: "bigint", nullable: false),
                    ExpectedAnnualDeliveries = table.Column<int>(type: "int", nullable: false),
                    DirectorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Region", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_District",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capital = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Population = table.Column<long>(type: "bigint", nullable: false),
                    ExpectedAnnualDeliveries = table.Column<int>(type: "int", nullable: false),
                    DirectorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_District", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_District_Tbl_Region_RegionID",
                        column: x => x.RegionID,
                        principalTable: "Tbl_Region",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_MonitoringUser",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<long>(type: "bigint", nullable: false),
                    AccessLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistrictID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DistrictName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginTime = table.Column<long>(type: "bigint", nullable: false),
                    LastLoginIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoginCount = table.Column<int>(type: "int", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedUntil = table.Column<long>(type: "bigint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    RequirePasswordChange = table.Column<bool>(type: "bit", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_MonitoringUser", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_MonitoringUser_Tbl_District_DistrictID",
                        column: x => x.DistrictID,
                        principalTable: "Tbl_District",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Tbl_MonitoringUser_Tbl_Region_RegionID",
                        column: x => x.RegionID,
                        principalTable: "Tbl_Region",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Facility_DistrictID",
                table: "Tbl_Facility",
                column: "DistrictID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Facility_RegionID",
                table: "Tbl_Facility",
                column: "RegionID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_District_Code",
                table: "Tbl_District",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_District_Name",
                table: "Tbl_District",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_District_RegionID",
                table: "Tbl_District",
                column: "RegionID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MonitoringUser_AccessLevel",
                table: "Tbl_MonitoringUser",
                column: "AccessLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MonitoringUser_DistrictID",
                table: "Tbl_MonitoringUser",
                column: "DistrictID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MonitoringUser_Email",
                table: "Tbl_MonitoringUser",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MonitoringUser_RegionID",
                table: "Tbl_MonitoringUser",
                column: "RegionID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Region_Code",
                table: "Tbl_Region",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Region_Name",
                table: "Tbl_Region",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Tbl_Facility_Tbl_District_DistrictID",
                table: "Tbl_Facility",
                column: "DistrictID",
                principalTable: "Tbl_District",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tbl_Facility_Tbl_District_DistrictID",
                table: "Tbl_Facility");

            migrationBuilder.DropTable(
                name: "Tbl_MonitoringUser");

            migrationBuilder.DropTable(
                name: "Tbl_District");

            migrationBuilder.DropTable(
                name: "Tbl_Region");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Facility_DistrictID",
                table: "Tbl_Facility");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Facility_RegionID",
                table: "Tbl_Facility");

            migrationBuilder.DropColumn(
                name: "DistrictID",
                table: "Tbl_Facility");

            migrationBuilder.DropColumn(
                name: "DistrictName",
                table: "Tbl_Facility");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Tbl_Facility");

            migrationBuilder.DropColumn(
                name: "RegionID",
                table: "Tbl_Facility");

            migrationBuilder.AddColumn<Guid>(
                name: "FacilityID",
                table: "Tbl_Staff",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
