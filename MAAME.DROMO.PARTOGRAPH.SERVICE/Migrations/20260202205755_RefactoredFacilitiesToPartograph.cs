using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Migrations
{
    /// <inheritdoc />
    public partial class RefactoredFacilitiesToPartograph : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistrictName",
                table: "Tbl_Facility");

            migrationBuilder.DropColumn(
                name: "RecordedBy",
                table: "Tbl_BishopScore");

            migrationBuilder.AddColumn<Guid>(
                name: "FacilityID",
                table: "Tbl_Patient",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FacilityID",
                table: "Tbl_Partograph",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacilityID",
                table: "Tbl_Patient");

            migrationBuilder.DropColumn(
                name: "FacilityID",
                table: "Tbl_Partograph");

            migrationBuilder.AddColumn<string>(
                name: "DistrictName",
                table: "Tbl_Facility",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecordedBy",
                table: "Tbl_BishopScore",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
