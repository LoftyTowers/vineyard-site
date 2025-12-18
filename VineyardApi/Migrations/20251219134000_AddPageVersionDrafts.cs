using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VineyardApi.Data;

#nullable disable

namespace VineyardApi.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(VineyardDbContext))]
    [Migration("20251219134000_AddPageVersionDrafts")]
    public partial class AddPageVersionDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PageVersions",
                type: "text",
                nullable: false,
                defaultValue: "Published");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                table: "PageVersions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedUtc",
                table: "PageVersions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DraftVersionId",
                table: "Pages",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("UPDATE \"PageVersions\" SET \"Status\" = 'Published';");
            migrationBuilder.Sql("UPDATE \"PageVersions\" SET \"PublishedUtc\" = \"CreatedUtc\" WHERE \"PublishedUtc\" IS NULL;");
            migrationBuilder.Sql("UPDATE \"Pages\" SET \"DraftVersionId\" = NULL;");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_PageVersions_PageId_Draft\" ON \"PageVersions\" (\"PageId\") WHERE \"Status\" = 'Draft';");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_DraftVersionId",
                table: "Pages",
                column: "DraftVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_PageVersions_DraftVersionId",
                table: "Pages",
                column: "DraftVersionId",
                principalTable: "PageVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pages_PageVersions_DraftVersionId",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_DraftVersionId",
                table: "Pages");

            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_PageVersions_PageId_Draft\";");

            migrationBuilder.DropColumn(
                name: "DraftVersionId",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "PublishedUtc",
                table: "PageVersions");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "PageVersions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PageVersions");
        }
    }
}
