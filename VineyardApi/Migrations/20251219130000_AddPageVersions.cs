using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VineyardApi.Data;
using VineyardApi.Domain.Content;

#nullable disable

namespace VineyardApi.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(VineyardDbContext))]
    [Migration("20251219130000_AddPageVersions")]
    public partial class AddPageVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS \"pgcrypto\";");

            migrationBuilder.CreateTable(
                name: "PageVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNo = table.Column<int>(type: "integer", nullable: false),
                    ContentJson = table.Column<PageContent>(type: "jsonb", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ChangeNote = table.Column<string>(type: "text", nullable: true),
                    ContentHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageVersions_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentVersionId",
                table: "Pages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageVersions_PageId_VersionNo",
                table: "PageVersions",
                columns: new[] { "PageId", "VersionNo" },
                unique: true);

            migrationBuilder.Sql(
                "CREATE INDEX \"IX_PageVersions_PageId_CreatedUtc\" ON \"PageVersions\" (\"PageId\", \"CreatedUtc\" DESC);");
            migrationBuilder.Sql(
                "CREATE INDEX \"IX_PageVersions_PageId_VersionNo_Desc\" ON \"PageVersions\" (\"PageId\", \"VersionNo\" DESC);");

            migrationBuilder.Sql(
                @"WITH inserted AS (
                    INSERT INTO ""PageVersions"" (""Id"", ""PageId"", ""VersionNo"", ""ContentJson"", ""CreatedUtc"")
                    SELECT gen_random_uuid(), p.""Id"", 1,
                           COALESCE(p.""DefaultContent"", '{}'::jsonb),
                           now()
                    FROM ""Pages"" p
                    RETURNING ""Id"", ""PageId""
                )
                UPDATE ""Pages"" p
                SET ""CurrentVersionId"" = i.""Id""
                FROM inserted i
                WHERE p.""Id"" = i.""PageId"";");

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentVersionId",
                table: "Pages",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pages_CurrentVersionId",
                table: "Pages",
                column: "CurrentVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_PageVersions_CurrentVersionId",
                table: "Pages",
                column: "CurrentVersionId",
                principalTable: "PageVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pages_PageVersions_CurrentVersionId",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_CurrentVersionId",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "CurrentVersionId",
                table: "Pages");

            migrationBuilder.DropTable(
                name: "PageVersions");
        }
    }
}
