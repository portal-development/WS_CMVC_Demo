﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WS_CMVC_Demo.Migrations
{
    public partial class PackageDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Packages",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Packages");
        }
    }
}
