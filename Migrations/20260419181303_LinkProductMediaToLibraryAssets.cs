using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vitacure.Migrations
{
    /// <inheritdoc />
    public partial class LinkProductMediaToLibraryAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MediaAssetId",
                table: "ProductMedias",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AltText",
                table: "MediaAssets",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "MediaAssets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductMedias_MediaAssetId",
                table: "ProductMedias",
                column: "MediaAssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMedias_MediaAssets_MediaAssetId",
                table: "ProductMedias",
                column: "MediaAssetId",
                principalTable: "MediaAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductMedias_MediaAssets_MediaAssetId",
                table: "ProductMedias");

            migrationBuilder.DropIndex(
                name: "IX_ProductMedias_MediaAssetId",
                table: "ProductMedias");

            migrationBuilder.DropColumn(
                name: "MediaAssetId",
                table: "ProductMedias");

            migrationBuilder.DropColumn(
                name: "AltText",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "MediaAssets");
        }
    }
}
