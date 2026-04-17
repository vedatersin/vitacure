using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vitacure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeContentSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HomeContentSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HeroTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HeroSubtitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MainPlaceholder = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SearchPlaceholder = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SearchPlaceholderLocked = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FeaturedTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FeaturedActionLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FeaturedActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PopularTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CampaignsTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DealsTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DealsActionLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DealsActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FeaturedBannerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FeaturedBannerAltText = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FeaturedBannerImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FeaturedBannerTargetUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PopularSupplementsContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CampaignBannersContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeContentSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomeContentSettings");
        }
    }
}
