using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WS_CMVC_Demo.Migrations
{
    public partial class CountryRegionCompetence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExcludeProperties",
                table: "UserSubcategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompetenceId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RussiaSubjectId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserCompetences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCompetences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCountries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCountries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRussiaSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRussiaSubjects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompetenceId",
                table: "AspNetUsers",
                column: "CompetenceId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CountryId",
                table: "AspNetUsers",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_RussiaSubjectId",
                table: "AspNetUsers",
                column: "RussiaSubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserCompetences_CompetenceId",
                table: "AspNetUsers",
                column: "CompetenceId",
                principalTable: "UserCompetences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserCountries_CountryId",
                table: "AspNetUsers",
                column: "CountryId",
                principalTable: "UserCountries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserRussiaSubjects_RussiaSubjectId",
                table: "AspNetUsers",
                column: "RussiaSubjectId",
                principalTable: "UserRussiaSubjects",
                principalColumn: "Id");

            migrationBuilder.Sql("INSERT INTO [dbo].[UserCountries] ([Title]) VALUES ('Абхазия'),('Австралия'),('Австрия'),('Азербайджан'),('Албания'),('Алжир'),('Амбазония'),('Ангола'),('Андорра'),('Антигуа и Барбуда'),('Аргентина'),('Армения'),('Афганистан'),('Багамы'),('Бангладеш'),('Барбадос'),('Бахрейн'),('Белиз'),('Белоруссия'),('Бельгия'),('Бенин'),('Болгария'),('Боливия'),('Босния и Герцеговина'),('Ботсвана'),('Бразилия'),('Бруней'),('Буркина-Фасо'),('Бурунди'),('Бутан'),('Вануату'),('Великобритания'),('Венгрия'),('Венесуэла'),('Восточный Тимор'),('Вьетнам'),('Габон'),('Гаити'),('Гайана'),('Гамбия'),('Гана'),('Гватемала'),('Гвинея'),('Гвинея-Бисау'),('Германия'),('Гондурас'),('Гренада'),('Греция'),('Грузия'),('Дания'),('Джибути'),('ДНР'),('Доминика'),('Доминикана'),('ДР Конго'),('Египет'),('Замбия'),('Зимбабве'),('Израиль'),('Индия'),('Индонезия'),('Иордания'),('Ирак'),('Иран'),('Ирландия'),('Исландия'),('Испания'),('Италия'),('Йемен'),('Кабо-Верде'),('Казахстан'),('Камбоджа'),('Камерун'),('Канада'),('Катар'),('Кения'),('Кипр'),('Киргизия'),('Кирибати'),('Китай'),('Китайская Республика (Тайвань)'),('КНДР'),('Колумбия'),('Коморы'),('Конго'),('Корея'),('Косово'),('Коста-Рика'),('Кот-д''Ивуар'),('Куба'),('Кувейт'),('Лаос'),('Латвия'),('Лесото'),('Либерия'),('Ливан'),('Ливия'),('Литва'),('Лихтенштейн'),('ЛНР'),('Люксембург'),('Маврикий'),('Мавритания'),('Мадагаскар'),('Малави'),('Малайзия'),('Мали'),('Мальдивы'),('Мальта'),('Марокко'),('Маршалловы Острова'),('Мексика'),('Микронезия'),('Мозамбик'),('Молдавия'),('Монако'),('Монголия'),('Мьянма'),('Нагалим'),('Намибия'),('Науру'),('Непал'),('Нигер'),('Нигерия'),('Нидерланды'),('Никарагуа'),('НКР'),('Новая Зеландия'),('Норвегия'),('ОАЭ'),('Оман'),('Пакистан'),('Палау'),('Панама'),('Папуа — Новая Гвинея'),('Парагвай'),('Перу'),('ПМР'),('Польша'),('Португалия'),('Россия'),('Руанда'),('Румыния'),('САДР'),('Сальвадор'),('Самоа'),('Сан-Марино'),('Сан-Томе и Принсипи'),('Саудовская Аравия'),('Северная Македония'),('Сейшелы'),('Сенегал'),('Сент-Винсент и Гренадины'),('Сент-Китс и Невис'),('Сент-Люсия'),('Сербия'),('Сингапур'),('Сирия'),('Словакия'),('Словения'),('Соломоновы Острова'),('Сомали'),('Сомалиленд'),('Судан'),('Суринам'),('США'),('Сьерра-Леоне'),('Таджикистан'),('Таиланд'),('Танзания'),('Того'),('Тонга'),('Тринидад и Тобаго'),('ТРСК'),('Тувалу'),('Тунис'),('Туркмения'),('Турция'),('Уганда'),('Узбекистан'),('Украина'),('Уругвай'),('Фиджи'),('Филиппины'),('Финляндия'),('Франция'),('Хорватия'),('ЦАР'),('Чад'),('Черногория'),('Чехия'),('Чили'),('Швейцария'),('Швеция'),('Шри-Ланка'),('Эквадор'),('Экваториальная Гвинея'),('Эритрея'),('Эсватини'),('Эстония'),('Эфиопия'),('ЮАР'),('Южная Осетия'),('Южный Судан'),('Ямайка'),('Япония')");
            migrationBuilder.Sql("INSERT INTO [dbo].[UserRussiaSubjects]([Title]) VALUES ('Республика Адыгея (Адыгея)'),('Республика Алтай'),('Республика Башкортостан'),('Республика Бурятия'),('Республика Дагестан'),('Республика Ингушетия'),('Кабардино-Балкарская Республика'),('Республика Калмыкия'),('Карачаево-Черкесская Республика'),('Республика Карелия'),('Республика Коми'),('Республика Крым'),('Республика Марий Эл'),('Республика Мордовия'),('Республика Саха (Якутия)'),('Республика Северная Осетия – Алания'),('Республика Татарстан (Татарстан)'),('Республика Тыва'),('Удмуртская Республика'),('Республика Хакасия'),('Чеченская Республика'),('Чувашская Республика – Чувашия'),('Алтайский край'),('Забайкальский край'),('Камчатский край'),('Краснодарский край'),('Красноярский край'),('Пермский край'),('Приморский край'),('Ставропольский край'),('Хабаровский край'),('Амурская область'),('Архангельская область'),('Астраханская область'),('Белгородская область'),('Брянская область'),('Владимирская область'),('Волгоградская область'),('Вологодская область'),('Воронежская область'),('Ивановская область'),('Иркутская область'),('Калининградская область'),('Калужская область'),('Кемеровская область'),('Кировская область'),('Костромская область'),('Курганская область'),('Курская область'),('Ленинградская область'),('Липецкая область'),('Магаданская область'),('Московская область'),('Мурманская область'),('Нижегородская область'),('Новгородская область'),('Новосибирская область'),('Омская область'),('Оренбургская область'),('Орловская область'),('Пензенская область'),('Псковская область'),('Ростовская область'),('Рязанская область'),('Самарская область'),('Саратовская область'),('Сахалинская область'),('Свердловская область'),('Смоленская область'),('Тамбовская область'),('Тверская область'),('Томская область'),('Тульская область'),('Тюменская область'),('Ульяновская область'),('Челябинская область'),('Ярославская область'),('Город Москва'),('Город Санкт-Петербург'),('Город Севастополь'),('Еврейская автономная область'),('Ненецкий автономный округ'),('Ханты-Мансийский автономный округ – Югра'),('Чукотский автономный округ'),('Ямало-Ненецкий автономный округ')");
            migrationBuilder.Sql("INSERT INTO [dbo].[UserCompetences] ([Title]) VALUES ('3D Моделирование для компьютерных игр (14-16)'),('Администрирование отеля (10-12)'),('Администрирование отеля (12-14)'),('Администрирование отеля (14-16)'),('Веб-технологии (14-16)'),('Видеопроизводство (10-12)'),('Видеопроизводство (12-14)'),('Видеопроизводство (14-16)'),('Визуальный мерчендайзинг (14-16)'),('Геномная инженерия'),('Графический дизайн (14-16)'),('Документационное обеспечение управления и архивоведение'),('Дошкольное воспитание (14-16)'),('Инженерный дизайн CAD (12-14)'),('Инженерный дизайн CAD (14-16)'),('Интернет маркетинг'),('Информационные кабельные сети'),('Кибербезопасность'),('Корпоративная защита от внутренних угроз информационной безопасности'),('Кровельные работы'),('Кровельные работы по металлу (12-16)'),('Машинное обучение и большие данные'),('Облицовка плиткой'),('Организация экскурсионных услуг (14-16)'),('Охрана труда'),('Полимеханика и автоматизация'),('Предпринимательство (14-16)'),('Преподавание в младших классах (14-16)'),('Программные решения для бизнеса (14-16)'),('Проектирование и изготовление пресс-форм'),('Проектирование нейроинтерфейсов (14-16)'),('Промышленная автоматика (14-16)'),('Промышленный дизайн (14-16)'),('Разработка виртуальной и дополненной реальности (14-16)'),('Разработка компьютерных игр и мультимедийных приложений'),('Рекрутинг'),('Ресторанный сервис (14-16)'),('Сетевое и системное администрирование (11-14)'),('Сетевое и системное администрирование (14-16)'),('Социальная работа'),('Турагентская деятельность'),('Туроператорская деятельность'),('Фармацевтика (14-16)'),('Фотография (14-16)'),('Холодильная техника и системы кондиционирования (14-16)'),('Экспедирование грузов'),('Эстетическая косметология (14-16)')");
            migrationBuilder.Sql("UPDATE [dbo].[UserCountries] SET [Order] = [Id] * 10");
            migrationBuilder.Sql("UPDATE [dbo].[UserRussiaSubjects] SET [Order] = [Id] * 10");
            migrationBuilder.Sql("UPDATE [dbo].[UserCompetences] SET [Order] = [Id] * 10");
            migrationBuilder.Sql("UPDATE [dbo].[UserCountries] SET [Order] = 5 WHERE [Title] = 'Россия'");
            //migrationBuilder.Sql("UPDATE [dbo].[UserSubcategories] SET [ExcludeProperties] = 'CountryId,RussiaSubjectId,CompetenceId,CompanyName'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserCompetences_CompetenceId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserCountries_CountryId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserRussiaSubjects_RussiaSubjectId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "UserCompetences");

            migrationBuilder.DropTable(
                name: "UserCountries");

            migrationBuilder.DropTable(
                name: "UserRussiaSubjects");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CompetenceId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CountryId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_RussiaSubjectId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ExcludeProperties",
                table: "UserSubcategories");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompetenceId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RussiaSubjectId",
                table: "AspNetUsers");
        }
    }
}
