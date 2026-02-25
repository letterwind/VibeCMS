using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebCMS.Api.Migrations
{
    /// <inheritdoc />
    public partial class LanguageResourceKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LanguageResources_LanguageCode_ResourceKey",
                table: "LanguageResources");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageResources_LanguageCode_ResourceKey_IsDeleted",
                table: "LanguageResources",
                columns: new[] { "LanguageCode", "ResourceKey", "IsDeleted" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LanguageResources_LanguageCode_ResourceKey_IsDeleted",
                table: "LanguageResources");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageResources_LanguageCode_ResourceKey",
                table: "LanguageResources",
                columns: new[] { "LanguageCode", "ResourceKey" },
                unique: true);
        }
    }
}
