using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WS_CMVC_Demo.Migrations
{
    public partial class Badge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BadgeColorId",
                table: "UserSubcategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BadgeTextColorId",
                table: "UserSubcategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleForPrint",
                table: "UserSubcategories",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BadgeServiceId",
                table: "Services",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BadgeColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColorGraph = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeColors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BadgeServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IcoUrl = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PeriodType = table.Column<byte>(type: "tinyint", nullable: false),
                    PeriodTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BadgeServiceCheckups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BadgeServiceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeServiceCheckups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BadgeServiceCheckups_AspNetUsers_CreateUserId",
                        column: x => x.CreateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BadgeServiceCheckups_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BadgeServiceCheckups_BadgeServices_BadgeServiceId",
                        column: x => x.BadgeServiceId,
                        principalTable: "BadgeServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubcategories_BadgeColorId",
                table: "UserSubcategories",
                column: "BadgeColorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubcategories_BadgeTextColorId",
                table: "UserSubcategories",
                column: "BadgeTextColorId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_BadgeServiceId",
                table: "Services",
                column: "BadgeServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BadgeServiceCheckups_BadgeServiceId",
                table: "BadgeServiceCheckups",
                column: "BadgeServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BadgeServiceCheckups_CreateUserId",
                table: "BadgeServiceCheckups",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BadgeServiceCheckups_UserId",
                table: "BadgeServiceCheckups",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_BadgeServices_BadgeServiceId",
                table: "Services",
                column: "BadgeServiceId",
                principalTable: "BadgeServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubcategories_BadgeColors_BadgeColorId",
                table: "UserSubcategories",
                column: "BadgeColorId",
                principalTable: "BadgeColors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubcategories_BadgeColors_BadgeTextColorId",
                table: "UserSubcategories",
                column: "BadgeTextColorId",
                principalTable: "BadgeColors",
                principalColumn: "Id");

            migrationBuilder.Sql(@"INSERT [dbo].[BadgeServices] ([IcoUrl], [PeriodType], [PeriodTime], [Title], [Order]) VALUES (N'/img/ico/packet.svg', 1, NULL, N'ПАКЕТ С РАЗДАТОЧНЫМИ МАТЕРИАЛАМИ', 10),(N'/img/ico/ticket.svg', 1, NULL, N'ПРОХОД НА ОТКРЫТИЕ И ЗАКРЫТИЕ ЧЕМПИОНАТА', 20),(N'/img/ico/dinner.svg', 2, CAST(N'13:00:00' AS Time), N'УЖИН', 30),(N'/img/ico/lunch.svg', 2, CAST(N'13:00:00' AS Time), N'ОБЕД', 40),(N'/img/ico/bus.svg', 0, NULL, N'ИСПОЛЬЗОВАНИЕ ТРАНСПОРТА', 50),(N'/img/ico/vip-dinner.svg', 2, CAST(N'01:00:00' AS Time), N'ДОСТУП В VIP-РЕСТОРАН', 60),(N'/img/ico/note.svg', 0, NULL, N'ВОЗМОЖНОСТЬ ПОСЕЩЕНИЯ МЕРОПРИЯТИЙ ДЕЛОВОЙ ПРОГРАММЫ', 70),(N'/img/ico/press.svg', 0, NULL, N'ДОСТУП В ПРЕСС-ЦЕНТР', 80)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_BadgeServices_BadgeServiceId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubcategories_BadgeColors_BadgeColorId",
                table: "UserSubcategories");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubcategories_BadgeColors_BadgeTextColorId",
                table: "UserSubcategories");

            migrationBuilder.DropTable(
                name: "BadgeColors");

            migrationBuilder.DropTable(
                name: "BadgeServiceCheckups");

            migrationBuilder.DropTable(
                name: "BadgeServices");

            migrationBuilder.DropIndex(
                name: "IX_UserSubcategories_BadgeColorId",
                table: "UserSubcategories");

            migrationBuilder.DropIndex(
                name: "IX_UserSubcategories_BadgeTextColorId",
                table: "UserSubcategories");

            migrationBuilder.DropIndex(
                name: "IX_Services_BadgeServiceId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "BadgeColorId",
                table: "UserSubcategories");

            migrationBuilder.DropColumn(
                name: "BadgeTextColorId",
                table: "UserSubcategories");

            migrationBuilder.DropColumn(
                name: "TitleForPrint",
                table: "UserSubcategories");

            migrationBuilder.DropColumn(
                name: "BadgeServiceId",
                table: "Services");
        }
    }
}
