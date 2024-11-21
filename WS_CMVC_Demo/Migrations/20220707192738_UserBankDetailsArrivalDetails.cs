using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WS_CMVC_Demo.Migrations
{
    public partial class UserBankDetailsArrivalDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivalDateTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArrivalDetails",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankDetails",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureDateTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartureDetails",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGeneralBankDetails",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalDateTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ArrivalDetails",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BankDetails",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DepartureDateTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DepartureDetails",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsGeneralBankDetails",
                table: "AspNetUsers");
        }
    }
}
