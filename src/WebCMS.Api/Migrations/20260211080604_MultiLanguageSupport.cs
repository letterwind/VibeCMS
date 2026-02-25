using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebCMS.Api.Migrations
{
    /// <inheritdoc />
    public partial class MultiLanguageSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Roles_Name",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Slug",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Articles_Slug",
                table: "Articles");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SiteSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "SiteSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "zh-TW");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "Roles",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "zh-TW");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HeaderSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "HeaderSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "zh-TW");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "Functions",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "zh-TW");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FooterSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "FooterSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "zh-TW");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "Categories",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "zh-TW");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "Articles",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "zh-TW");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "AdminUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LanguageName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Id", "CreatedAt", "IsActive", "LanguageCode", "LanguageName", "SortOrder", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 11, 0, 0, 0, 0, DateTimeKind.Utc), true, "zh-TW", "繁體中文", 1, new DateTime(2026, 2, 11, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2026, 2, 11, 0, 0, 0, 0, DateTimeKind.Utc), true, "en-US", "English", 2, new DateTime(2026, 2, 11, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2026, 2, 11, 0, 0, 0, 0, DateTimeKind.Utc), true, "ja-JP", "日本語", 3, new DateTime(2026, 2, 11, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_LanguageCode",
                table: "SiteSettings",
                column: "LanguageCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name_LanguageCode",
                table: "Roles",
                columns: new[] { "Name", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderSettings_LanguageCode",
                table: "HeaderSettings",
                column: "LanguageCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FooterSettings_LanguageCode",
                table: "FooterSettings",
                column: "LanguageCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug_LanguageCode",
                table: "Categories",
                columns: new[] { "Slug", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Slug_LanguageCode",
                table: "Articles",
                columns: new[] { "Slug", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Languages_LanguageCode",
                table: "Languages",
                column: "LanguageCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropIndex(
                name: "IX_SiteSettings_LanguageCode",
                table: "SiteSettings");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Name_LanguageCode",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_HeaderSettings_LanguageCode",
                table: "HeaderSettings");

            migrationBuilder.DropIndex(
                name: "IX_FooterSettings_LanguageCode",
                table: "FooterSettings");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Slug_LanguageCode",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Articles_Slug_LanguageCode",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HeaderSettings");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "HeaderSettings");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "Functions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FooterSettings");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "FooterSettings");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "AdminUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Slug",
                table: "Articles",
                column: "Slug",
                unique: true);
        }
    }
}
