using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WS_CMVC_Demo.Migrations
{
    public partial class EventUserReletionsFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_EventUsers_Events_EventId",
            //    table: "EventUsers");

            //migrationBuilder.DropIndex(
            //    name: "IX_EventUsers_EventId",
            //    table: "EventUsers");

            //migrationBuilder.DropColumn(
            //    name: "EventId",
            //    table: "EventUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<int>(
            //    name: "EventId",
            //    table: "EventUsers",
            //    type: "int",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_EventUsers_EventId",
            //    table: "EventUsers",
            //    column: "EventId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_EventUsers_Events_EventId",
            //    table: "EventUsers",
            //    column: "EventId",
            //    principalTable: "Events",
            //    principalColumn: "Id");
        }
    }
}
