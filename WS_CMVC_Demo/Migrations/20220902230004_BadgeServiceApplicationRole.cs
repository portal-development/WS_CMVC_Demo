using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WS_CMVC_Demo.Migrations
{
    public partial class BadgeServiceApplicationRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "RecommendedEndTime",
                table: "BadgeServices",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "RecommendedStartTime",
                table: "BadgeServices",
                type: "time",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BadgeServiceApplicationRoles",
                columns: table => new
                {
                    BadgeServiceId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeServiceApplicationRoles", x => new { x.BadgeServiceId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_BadgeServiceApplicationRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BadgeServiceApplicationRoles_BadgeServices_BadgeServiceId",
                        column: x => x.BadgeServiceId,
                        principalTable: "BadgeServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BadgeServiceApplicationRoles_RoleId",
                table: "BadgeServiceApplicationRoles",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BadgeServiceApplicationRoles");

            migrationBuilder.DropColumn(
                name: "RecommendedEndTime",
                table: "BadgeServices");

            migrationBuilder.DropColumn(
                name: "RecommendedStartTime",
                table: "BadgeServices");
        }
    }
}
