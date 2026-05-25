using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vitacure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeShowcaseTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShowcaseTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShowcaseId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowcaseTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShowcaseTags_Showcases_ShowcaseId",
                        column: x => x.ShowcaseId,
                        principalTable: "Showcases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShowcaseTags_ShowcaseId_SortOrder",
                table: "ShowcaseTags",
                columns: new[] { "ShowcaseId", "SortOrder" });

            migrationBuilder.Sql("""
                INSERT INTO ShowcaseTags (ShowcaseId, Name, Slug, SortOrder)
                SELECT
                    s.Id,
                    LTRIM(RTRIM(parts.value)) AS Name,
                    LOWER(
                        REPLACE(
                            REPLACE(
                                REPLACE(
                                    REPLACE(
                                        REPLACE(
                                            REPLACE(
                                                REPLACE(LTRIM(RTRIM(parts.value)), N'ı', N'i'),
                                            N'ğ', N'g'),
                                        N'ü', N'u'),
                                    N'ş', N's'),
                                N'ö', N'o'),
                            N'ç', N'c'),
                        N' ', N'-')) AS Slug,
                    ROW_NUMBER() OVER (PARTITION BY s.Id ORDER BY (SELECT 1)) - 1 AS SortOrder
                FROM Showcases s
                CROSS APPLY STRING_SPLIT(
                    REPLACE(
                        REPLACE(
                            REPLACE(ISNULL(s.TagsContent, N''), CHAR(13) + CHAR(10), N'|'),
                            CHAR(13), N'|'),
                        CHAR(10), N'|'),
                    N'|') AS parts
                WHERE LTRIM(RTRIM(parts.value)) <> N'';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShowcaseTags");
        }
    }
}
