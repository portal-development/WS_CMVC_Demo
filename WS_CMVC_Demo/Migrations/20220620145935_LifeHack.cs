using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WS_CMVC_Demo.Migrations
{
    public partial class LifeHack : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_HotelOptions_HotelOptionId",
                table: "Services");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "HotelOptions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_HotelOptions_HotelOptionId",
                table: "Services",
                column: "HotelOptionId",
                principalTable: "HotelOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_HotelOptions_HotelOptionId",
                table: "Services");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "HotelOptions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_HotelOptions_HotelOptionId",
                table: "Services",
                column: "HotelOptionId",
                principalTable: "HotelOptions",
                principalColumn: "Id");
        }
    }
}
