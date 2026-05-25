using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vitacure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductKindAndUnitPricingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductKind",
                table: "Products",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "Physical");

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitComparisonAmount",
                table: "Products",
                type: "decimal(12,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitComparisonType",
                table: "Products",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitContentAmount",
                table: "Products",
                type: "decimal(12,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitContentType",
                table: "Products",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductKind",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitComparisonAmount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitComparisonType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitContentAmount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitContentType",
                table: "Products");
        }
    }
}
