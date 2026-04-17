using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vitacure.Migrations
{
    /// <inheritdoc />
    public partial class AddShowcaseModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Showcases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TagsContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackgroundImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SeoTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ShowOnHome = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Showcases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShowcaseCategories",
                columns: table => new
                {
                    ShowcaseId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowcaseCategories", x => new { x.ShowcaseId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_ShowcaseCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShowcaseCategories_Showcases_ShowcaseId",
                        column: x => x.ShowcaseId,
                        principalTable: "Showcases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShowcaseFeaturedProducts",
                columns: table => new
                {
                    ShowcaseId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowcaseFeaturedProducts", x => new { x.ShowcaseId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_ShowcaseFeaturedProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShowcaseFeaturedProducts_Showcases_ShowcaseId",
                        column: x => x.ShowcaseId,
                        principalTable: "Showcases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShowcaseCategories_CategoryId",
                table: "ShowcaseCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowcaseFeaturedProducts_ProductId",
                table: "ShowcaseFeaturedProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Showcases_Slug",
                table: "Showcases",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShowcaseCategories");

            migrationBuilder.DropTable(
                name: "ShowcaseFeaturedProducts");

            migrationBuilder.DropTable(
                name: "Showcases");
        }
    }
}
