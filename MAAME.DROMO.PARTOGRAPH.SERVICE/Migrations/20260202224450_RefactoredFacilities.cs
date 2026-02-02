using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Migrations
{
    /// <inheritdoc />
    public partial class RefactoredFacilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tbl_District_Tbl_Region_RegionID",
                table: "Tbl_District");

            migrationBuilder.DropForeignKey(
                name: "FK_Tbl_Facility_Tbl_District_DistrictID",
                table: "Tbl_Facility");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Facility_Region",
                table: "Tbl_Facility");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Facility_RegionID",
                table: "Tbl_Facility");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Tbl_Facility");

            migrationBuilder.DropColumn(
                name: "RegionID",
                table: "Tbl_Facility");

            migrationBuilder.DropColumn(
                name: "RegionName",
                table: "Tbl_District");

            migrationBuilder.AddColumn<Guid>(
                name: "FacilityID",
                table: "Tbl_MonitoringUser",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacilityName",
                table: "Tbl_MonitoringUser",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Staff_Facility",
                table: "Tbl_Staff",
                column: "Facility");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Patient_FacilityID",
                table: "Tbl_Patient",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Partograph_FacilityID",
                table: "Tbl_Partograph",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_MonitoringUser_FacilityID",
                table: "Tbl_MonitoringUser",
                column: "FacilityID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tbl_District_Tbl_Region_RegionID",
                table: "Tbl_District",
                column: "RegionID",
                principalTable: "Tbl_Region",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tbl_Facility_Tbl_District_DistrictID",
                table: "Tbl_Facility",
                column: "DistrictID",
                principalTable: "Tbl_District",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tbl_Partograph_Tbl_Facility_FacilityID",
                table: "Tbl_Partograph",
                column: "FacilityID",
                principalTable: "Tbl_Facility",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tbl_Patient_Tbl_Facility_FacilityID",
                table: "Tbl_Patient",
                column: "FacilityID",
                principalTable: "Tbl_Facility",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tbl_District_Tbl_Region_RegionID",
                table: "Tbl_District");

            migrationBuilder.DropForeignKey(
                name: "FK_Tbl_Facility_Tbl_District_DistrictID",
                table: "Tbl_Facility");

            migrationBuilder.DropForeignKey(
                name: "FK_Tbl_Partograph_Tbl_Facility_FacilityID",
                table: "Tbl_Partograph");

            migrationBuilder.DropForeignKey(
                name: "FK_Tbl_Patient_Tbl_Facility_FacilityID",
                table: "Tbl_Patient");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Staff_Facility",
                table: "Tbl_Staff");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Patient_FacilityID",
                table: "Tbl_Patient");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Partograph_FacilityID",
                table: "Tbl_Partograph");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_MonitoringUser_FacilityID",
                table: "Tbl_MonitoringUser");

            migrationBuilder.DropColumn(
                name: "FacilityID",
                table: "Tbl_MonitoringUser");

            migrationBuilder.DropColumn(
                name: "FacilityName",
                table: "Tbl_MonitoringUser");

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Tbl_Facility",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RegionID",
                table: "Tbl_Facility",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionName",
                table: "Tbl_District",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Facility_Region",
                table: "Tbl_Facility",
                column: "Region");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Facility_RegionID",
                table: "Tbl_Facility",
                column: "RegionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tbl_District_Tbl_Region_RegionID",
                table: "Tbl_District",
                column: "RegionID",
                principalTable: "Tbl_Region",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tbl_Facility_Tbl_District_DistrictID",
                table: "Tbl_Facility",
                column: "DistrictID",
                principalTable: "Tbl_District",
                principalColumn: "ID");
        }
    }
}
