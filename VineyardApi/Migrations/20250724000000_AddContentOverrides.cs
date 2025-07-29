using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VineyardApi.Migrations
{
    /// <inheritdoc />
    public partial class AddContentOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    HtmlValue = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "draft"),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ChangedById = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentOverrides_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentOverrides_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentOverrides_PageId",
                table: "ContentOverrides",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentOverrides_ChangedById",
                table: "ContentOverrides",
                column: "ChangedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentOverrides");
        }
    }
}
