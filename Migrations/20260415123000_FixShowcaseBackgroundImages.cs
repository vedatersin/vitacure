using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using vitacure.Infrastructure.Persistence;

#nullable disable

namespace vitacure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260415123000_FixShowcaseBackgroundImages")]
    public partial class FixShowcaseBackgroundImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE Showcases
                SET BackgroundImageUrl = CASE
                    WHEN Slug = 'uyku-rutini' OR Slug = 'uyku-sagligi' THEN '/img/uykuBg.png'
                    WHEN Slug = 'multivitamin-enerji-plani' OR Slug = 'multivitamin-enerji' THEN '/img/multivitaminBg.png'
                    WHEN Slug = 'zihin-hafiza-rotasi' OR Slug = 'zihin-hafiza-guclendirme' THEN '/img/zekaHafızaBg.png'
                    WHEN Slug = 'bagisiklik-koruma-plani' OR Slug = 'hastaliklara-karsi-koruma' THEN '/img/hastalıkKorumaBg.png'
                    WHEN Slug = 'kas-iskelet-destegi' OR Slug = 'kas-ve-iskelet-sagligi' THEN '/img/kasİskeletBg.png'
                    WHEN Slug = 'zayiflama-rotasi' OR Slug = 'zayiflama-destegi' THEN '/img/zayıflamaBg.png'
                    ELSE BackgroundImageUrl
                END
                WHERE Slug IN (
                    'uyku-rutini', 'uyku-sagligi',
                    'multivitamin-enerji-plani', 'multivitamin-enerji',
                    'zihin-hafiza-rotasi', 'zihin-hafiza-guclendirme',
                    'bagisiklik-koruma-plani', 'hastaliklara-karsi-koruma',
                    'kas-iskelet-destegi', 'kas-ve-iskelet-sagligi',
                    'zayiflama-rotasi', 'zayiflama-destegi'
                );
                """
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
