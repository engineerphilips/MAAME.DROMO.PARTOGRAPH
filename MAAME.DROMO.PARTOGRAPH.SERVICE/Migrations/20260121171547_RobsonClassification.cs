using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Migrations
{
    /// <inheritdoc />
    public partial class RobsonClassification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LaborOnset",
                table: "Tbl_Partograph",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Tbl_RobsonClassification",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartographID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Parity = table.Column<int>(type: "int", nullable: false),
                    HasPreviousCesarean = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfPreviousCesareans = table.Column<int>(type: "int", nullable: false),
                    GestationalAgeWeeks = table.Column<int>(type: "int", nullable: false),
                    LaborOnset = table.Column<int>(type: "int", nullable: false),
                    FetalPresentation = table.Column<int>(type: "int", nullable: false),
                    NumberOfFetuses = table.Column<int>(type: "int", nullable: false),
                    Group = table.Column<int>(type: "int", nullable: false),
                    DeliveryMode = table.Column<int>(type: "int", nullable: false),
                    CesareanIndication = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CesareanType = table.Column<int>(type: "int", nullable: true),
                    ClassifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassifiedByStaffID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAutoClassified = table.Column<bool>(type: "bit", nullable: false),
                    ValidationStatus = table.Column<int>(type: "int", nullable: false),
                    OverrideReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedTime = table.Column<long>(type: "bigint", nullable: false),
                    DeletedTime = table.Column<long>(type: "bigint", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ServerVersion = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ConflictData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_RobsonClassification", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tbl_RobsonClassification_Tbl_Partograph_PartographID",
                        column: x => x.PartographID,
                        principalTable: "Tbl_Partograph",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_RobsonClassification_ClassifiedAt",
                table: "Tbl_RobsonClassification",
                column: "ClassifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_RobsonClassification_Group",
                table: "Tbl_RobsonClassification",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_RobsonClassification_Group_ClassifiedAt",
                table: "Tbl_RobsonClassification",
                columns: new[] { "Group", "ClassifiedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_RobsonClassification_Group_DeliveryMode",
                table: "Tbl_RobsonClassification",
                columns: new[] { "Group", "DeliveryMode" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_RobsonClassification_PartographID",
                table: "Tbl_RobsonClassification",
                column: "PartographID");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_RobsonClassification_ServerVersion",
                table: "Tbl_RobsonClassification",
                column: "ServerVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_RobsonClassification_SyncStatus",
                table: "Tbl_RobsonClassification",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_RobsonClassification_UpdatedTime",
                table: "Tbl_RobsonClassification",
                column: "UpdatedTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tbl_RobsonClassification");

            migrationBuilder.DropColumn(
                name: "LaborOnset",
                table: "Tbl_Partograph");
        }
    }
}
