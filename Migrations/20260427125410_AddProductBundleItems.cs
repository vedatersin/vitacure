using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vitacure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductBundleItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariantSelections_ProductVariantOptions_ProductVariantOptionId",
                table: "ProductVariantSelections");

            migrationBuilder.AddColumn<decimal>(
                name: "BundleAdjustmentAmount",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BundleAdjustmentType",
                table: "Products",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BundleMode",
                table: "Products",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BundlePricingMode",
                table: "Products",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BundleTotalQuantity",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductBundleItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    ChildProductId = table.Column<int>(type: "int", nullable: false),
                    ChildProductVariantId = table.Column<int>(type: "int", nullable: true),
                    EntryMode = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    MinQuantity = table.Column<int>(type: "int", nullable: true),
                    MaxQuantity = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBundleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductBundleItems_ProductVariants_ChildProductVariantId",
                        column: x => x.ChildProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductBundleItems_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductBundleItems_Products_ChildProductId",
                        column: x => x.ChildProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductBundleItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductBundleItems_ChildProductId",
                table: "ProductBundleItems",
                column: "ChildProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBundleItems_ChildProductVariantId",
                table: "ProductBundleItems",
                column: "ChildProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBundleItems_ProductId_ProductVariantId_SortOrder",
                table: "ProductBundleItems",
                columns: new[] { "ProductId", "ProductVariantId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductBundleItems_ProductVariantId",
                table: "ProductBundleItems",
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariantSelections_ProductVariantOptions_ProductVariantOptionId",
                table: "ProductVariantSelections",
                column: "ProductVariantOptionId",
                principalTable: "ProductVariantOptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariantSelections_ProductVariantOptions_ProductVariantOptionId",
                table: "ProductVariantSelections");

            migrationBuilder.DropTable(
                name: "ProductBundleItems");

            migrationBuilder.DropColumn(
                name: "BundleAdjustmentAmount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BundleAdjustmentType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BundleMode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BundlePricingMode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BundleTotalQuantity",
                table: "Products");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariantSelections_ProductVariantOptions_ProductVariantOptionId",
                table: "ProductVariantSelections",
                column: "ProductVariantOptionId",
                principalTable: "ProductVariantOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
