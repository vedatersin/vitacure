using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vitacure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeShowcaseCategoryAndPrompts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrimaryCategoryId",
                table: "Showcases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShowcasePrompts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShowcaseId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowcasePrompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShowcasePrompts_Showcases_ShowcaseId",
                        column: x => x.ShowcaseId,
                        principalTable: "Showcases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Showcases_PrimaryCategoryId",
                table: "Showcases",
                column: "PrimaryCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowcasePrompts_ShowcaseId_SortOrder",
                table: "ShowcasePrompts",
                columns: new[] { "ShowcaseId", "SortOrder" });

            migrationBuilder.Sql("""
                UPDATE s
                SET s.PrimaryCategoryId = sc.CategoryId
                FROM Showcases s
                OUTER APPLY (
                    SELECT TOP (1) CategoryId
                    FROM ShowcaseCategories
                    WHERE ShowcaseId = s.Id
                    ORDER BY CategoryId
                ) sc
                WHERE s.PrimaryCategoryId IS NULL;
                """);

            migrationBuilder.Sql("""
                INSERT INTO ShowcasePrompts (ShowcaseId, Text, SortOrder)
                SELECT
                    s.Id,
                    LTRIM(RTRIM(parts.value)) AS Text,
                    ROW_NUMBER() OVER (PARTITION BY s.Id ORDER BY (SELECT 1)) - 1 AS SortOrder
                FROM Showcases s
                CROSS APPLY STRING_SPLIT(
                    REPLACE(
                        REPLACE(
                            REPLACE(ISNULL(s.ExamplePromptsContent, N''), CHAR(13) + CHAR(10), N'|'),
                            CHAR(13), N'|'),
                        CHAR(10), N'|'),
                    N'|') AS parts
                WHERE LTRIM(RTRIM(parts.value)) <> N'';
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Showcases_Categories_PrimaryCategoryId",
                table: "Showcases",
                column: "PrimaryCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Showcases_Categories_PrimaryCategoryId",
                table: "Showcases");

            migrationBuilder.DropTable(
                name: "ShowcasePrompts");

            migrationBuilder.DropIndex(
                name: "IX_Showcases_PrimaryCategoryId",
                table: "Showcases");

            migrationBuilder.DropColumn(
                name: "PrimaryCategoryId",
                table: "Showcases");
        }
    }
}
