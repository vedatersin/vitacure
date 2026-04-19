using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vitacure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemStorageSettingsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "MediaAssets",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StorageProvider",
                table: "MediaAssets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "StorageSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCdnEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ServiceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PublicBaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BucketName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccessKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SecretKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    KeyPrefix = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UsePathStyle = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StorageSettings");

            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "StorageProvider",
                table: "MediaAssets");
        }
    }
}
