using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vitacure.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantSupportToCartAndOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerCartItems_AppUserId_ProductId",
                table: "CustomerCartItems");

            migrationBuilder.AddColumn<int>(
                name: "ProductVariantId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantLabel",
                table: "OrderItems",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductVariantId",
                table: "CustomerCartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductVariantId",
                table: "OrderItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCartItems_AppUserId_ProductId_ProductVariantId",
                table: "CustomerCartItems",
                columns: new[] { "AppUserId", "ProductId", "ProductVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCartItems_ProductVariantId",
                table: "CustomerCartItems",
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerCartItems_ProductVariants_ProductVariantId",
                table: "CustomerCartItems",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductVariants_ProductVariantId",
                table: "OrderItems",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerCartItems_ProductVariants_ProductVariantId",
                table: "CustomerCartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductVariants_ProductVariantId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductVariantId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_CustomerCartItems_AppUserId_ProductId_ProductVariantId",
                table: "CustomerCartItems");

            migrationBuilder.DropIndex(
                name: "IX_CustomerCartItems_ProductVariantId",
                table: "CustomerCartItems");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VariantLabel",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "CustomerCartItems");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCartItems_AppUserId_ProductId",
                table: "CustomerCartItems",
                columns: new[] { "AppUserId", "ProductId" },
                unique: true);
        }
    }
}
