using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WS_CMVC_Demo.Migrations
{
    public partial class PackageEventUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "UserPackageServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserPackageServices_EventId",
                table: "UserPackageServices",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPackageServices_Events_EventId",
                table: "UserPackageServices",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPackageServices_Events_EventId",
                table: "UserPackageServices");

            migrationBuilder.DropIndex(
                name: "IX_UserPackageServices_EventId",
                table: "UserPackageServices");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "UserPackageServices");
        }
    }
}
