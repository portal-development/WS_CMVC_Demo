using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WS_CMVC_Demo.Migrations
{
    public partial class ExcludeToInclude : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExcludeProperties",
                table: "UserSubcategories",
                newName: "IncludeProperties");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IncludeProperties",
                table: "UserSubcategories",
                newName: "ExcludeProperties");
        }
    }
}
