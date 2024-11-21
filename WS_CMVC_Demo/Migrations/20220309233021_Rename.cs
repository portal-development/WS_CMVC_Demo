using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WS_CMVC_Demo.Migrations
{
    public partial class Rename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ReristeredUserId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ReristeredUserId",
                table: "AspNetUsers",
                newName: "RegisteredUserId");

            migrationBuilder.RenameColumn(
                name: "ReristeredHimself",
                table: "AspNetUsers",
                newName: "RegisteredHimself");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_ReristeredUserId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_RegisteredUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_RegisteredUserId",
                table: "AspNetUsers",
                column: "RegisteredUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_RegisteredUserId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "RegisteredUserId",
                table: "AspNetUsers",
                newName: "ReristeredUserId");

            migrationBuilder.RenameColumn(
                name: "RegisteredHimself",
                table: "AspNetUsers",
                newName: "ReristeredHimself");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_RegisteredUserId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_ReristeredUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ReristeredUserId",
                table: "AspNetUsers",
                column: "ReristeredUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
