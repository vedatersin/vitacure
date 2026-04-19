using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vitacure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductFeatureValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "ProductFeatures",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "ProductFeatures");
        }
    }
}
