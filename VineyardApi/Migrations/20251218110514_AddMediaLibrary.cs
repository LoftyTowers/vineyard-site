using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VineyardApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Images",
                newName: "PublicUrl");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Images",
                newName: "CreatedUtc");

            migrationBuilder.AddColumn<long>(
                name: "ByteSize",
                table: "Images",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Images",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Images",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Images",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFilename",
                table: "Images",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "Images",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "Images",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("UPDATE \"Images\" SET \"StorageKey\" = \"PublicUrl\" WHERE \"StorageKey\" IS NULL;");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedUtc",
                table: "Images",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "StorageKey",
                table: "Images",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ImageUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UsageType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    JsonPath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageUsages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_StorageKey",
                table: "Images",
                column: "StorageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageUsages_EntityType_EntityKey",
                table: "ImageUsages",
                columns: new[] { "EntityType", "EntityKey" });

            migrationBuilder.CreateIndex(
                name: "IX_ImageUsages_ImageId",
                table: "ImageUsages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageUsages_ImageId_EntityType_EntityKey_UsageType_Source_J~",
                table: "ImageUsages",
                columns: new[] { "ImageId", "EntityType", "EntityKey", "UsageType", "Source", "JsonPath" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageUsages");

            migrationBuilder.DropIndex(
                name: "IX_Images_StorageKey",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ByteSize",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "OriginalFilename",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Images");

            migrationBuilder.RenameColumn(
                name: "CreatedUtc",
                table: "Images",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "PublicUrl",
                table: "Images",
                newName: "Url");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Images",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now() at time zone 'utc'");
        }
    }
}
