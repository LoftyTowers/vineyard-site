using Microsoft.EntityFrameworkCore.Migrations;
using VineyardApi.Domain.Content;

#nullable disable

namespace VineyardApi.Migrations
{
    /// <inheritdoc />
    public partial class MakePageDefaultContentRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure existing rows have a non-null value before making the column required
            migrationBuilder.Sql("UPDATE \"Pages\" SET \"DefaultContent\" = '{}'::jsonb WHERE \"DefaultContent\" IS NULL;");

            migrationBuilder.AlterColumn<PageContent>(
                name: "DefaultContent",
                table: "Pages",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(PageContent),
                oldType: "jsonb",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<PageContent>(
                name: "DefaultContent",
                table: "Pages",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(PageContent),
                oldType: "jsonb");
        }
    }
}
