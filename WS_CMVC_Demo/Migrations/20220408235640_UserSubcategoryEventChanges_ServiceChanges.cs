using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WS_CMVC_Demo.Migrations
{
    public partial class UserSubcategoryEventChanges_ServiceChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventUsers_Events_EventId",
                table: "EventUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_EventUsers_UserSubcategoryEvents_EventId_UserSubcategoryId",
                table: "EventUsers");

            migrationBuilder.DropTable(
                name: "UserSubcategoryPackages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSubcategoryEvents",
                table: "UserSubcategoryEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventUsers",
                table: "EventUsers");

            migrationBuilder.DropIndex(
                name: "IX_EventUsers_EventId_UserSubcategoryId",
                table: "EventUsers");

            migrationBuilder.RenameColumn(
                name: "UserSubcategoryId",
                table: "EventUsers",
                newName: "UserSubcategoryEventId");

            //Дописано мной вручную
            migrationBuilder.DropColumn(
                name: "EventId",
                 table: "EventUsers"
                );
            //////

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserSubcategoryEvents",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishDate",
                table: "UserPackageServices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "UserPackageServices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Cost",
                table: "Quotas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinimalDaysCount",
                table: "PackageServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            ///Обработано выше
            //migrationBuilder.AlterColumn<int>(
            //    name: "EventId",
            //    table: "EventUsers",
            //    type: "int",
            //    nullable: true,
            //    oldClrType: typeof(int),
            //    oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSubcategoryEvents",
                table: "UserSubcategoryEvents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventUsers",
                table: "EventUsers",
                columns: new[] { "UserId", "UserSubcategoryEventId" });

            migrationBuilder.CreateTable(
                name: "UserSubcategoryEventPackages",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    UserSubcategoryEventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubcategoryEventPackages", x => new { x.PackageId, x.UserSubcategoryEventId });
                    table.ForeignKey(
                        name: "FK_UserSubcategoryEventPackages_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubcategoryEventPackages_UserSubcategoryEvents_UserSubcategoryEventId",
                        column: x => x.UserSubcategoryEventId,
                        principalTable: "UserSubcategoryEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubcategoryEvents_EventId",
                table: "UserSubcategoryEvents",
                column: "EventId");

            ///Обработано выше
            //migrationBuilder.CreateIndex(
            //    name: "IX_EventUsers_EventId",
            //    table: "EventUsers",
            //    column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventUsers_UserSubcategoryEventId",
                table: "EventUsers",
                column: "UserSubcategoryEventId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubcategoryEventPackages_UserSubcategoryEventId",
                table: "UserSubcategoryEventPackages",
                column: "UserSubcategoryEventId");

            ///Обработано выше
            //migrationBuilder.AddForeignKey(
            //    name: "FK_EventUsers_Events_EventId",
            //    table: "EventUsers",
            //    column: "EventId",
            //    principalTable: "Events",
            //    principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventUsers_UserSubcategoryEvents_UserSubcategoryEventId",
                table: "EventUsers",
                column: "UserSubcategoryEventId",
                principalTable: "UserSubcategoryEvents",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ///Обработано выше
            //migrationBuilder.DropForeignKey(
            //    name: "FK_EventUsers_Events_EventId",
            //    table: "EventUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_EventUsers_UserSubcategoryEvents_UserSubcategoryEventId",
                table: "EventUsers");

            migrationBuilder.DropTable(
                name: "UserSubcategoryEventPackages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSubcategoryEvents",
                table: "UserSubcategoryEvents");

            migrationBuilder.DropIndex(
                name: "IX_UserSubcategoryEvents_EventId",
                table: "UserSubcategoryEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventUsers",
                table: "EventUsers");

            ///Обработано выше
            //migrationBuilder.DropIndex(
            //    name: "IX_EventUsers_EventId",
            //    table: "EventUsers");

            //migrationBuilder.DropIndex(
            //    name: "IX_EventUsers_UserSubcategoryEventId",
            //    table: "EventUsers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserSubcategoryEvents");

            migrationBuilder.DropColumn(
                name: "FinishDate",
                table: "UserPackageServices");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "UserPackageServices");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Quotas");

            migrationBuilder.DropColumn(
                name: "MinimalDaysCount",
                table: "PackageServices");

            migrationBuilder.RenameColumn(
                name: "UserSubcategoryEventId",
                table: "EventUsers",
                newName: "UserSubcategoryId");

            //Добавлено вручную, создаем EventID
            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "EventUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSubcategoryEvents",
                table: "UserSubcategoryEvents",
                columns: new[] { "EventId", "UserSubcategoryId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventUsers",
                table: "EventUsers",
                columns: new[] { "UserId", "EventId", "UserSubcategoryId" });

            migrationBuilder.CreateTable(
                name: "UserSubcategoryPackages",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    UserSubcategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubcategoryPackages", x => new { x.PackageId, x.UserSubcategoryId });
                    table.ForeignKey(
                        name: "FK_UserSubcategoryPackages_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubcategoryPackages_UserSubcategories_UserSubcategoryId",
                        column: x => x.UserSubcategoryId,
                        principalTable: "UserSubcategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventUsers_EventId_UserSubcategoryId",
                table: "EventUsers",
                columns: new[] { "EventId", "UserSubcategoryId" });

            //Добавлено вручную, возвращаем индекс по EventId

            migrationBuilder.CreateIndex(
                name: "IX_UserSubcategoryPackages_UserSubcategoryId",
                table: "UserSubcategoryPackages",
                column: "UserSubcategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventUsers_Events_EventId",
                table: "EventUsers",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventUsers_UserSubcategoryEvents_EventId_UserSubcategoryId",
                table: "EventUsers",
                columns: new[] { "EventId", "UserSubcategoryId" },
                principalTable: "UserSubcategoryEvents",
                principalColumns: new[] { "EventId", "UserSubcategoryId" });
        }
    }
}
