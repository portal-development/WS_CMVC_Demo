﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WS_CMVC_Demo.Migrations
{
    public partial class AutoPackage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoPackage",
                table: "UserSubcategories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoPackage",
                table: "UserSubcategories");
        }
    }
}
