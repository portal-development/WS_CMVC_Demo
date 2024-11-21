using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WS_CMVC_Demo.Migrations
{
    public partial class User_UserSubcategoryId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserSubcategoryId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserSubcategoryId",
                table: "AspNetUsers",
                column: "UserSubcategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserSubcategories_UserSubcategoryId",
                table: "AspNetUsers",
                column: "UserSubcategoryId",
                principalTable: "UserSubcategories",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserSubcategories_UserSubcategoryId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UserSubcategoryId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserSubcategoryId",
                table: "AspNetUsers");
        }
    }
}
