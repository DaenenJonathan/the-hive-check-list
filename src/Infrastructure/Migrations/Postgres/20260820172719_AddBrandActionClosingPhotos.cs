using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHive.Infrastructure.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddBrandActionClosingPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsumablesPhotoPath",
                table: "BrandActions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialPhotoPath",
                table: "BrandActions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsumablesPhotoPath",
                table: "BrandActions");

            migrationBuilder.DropColumn(
                name: "MaterialPhotoPath",
                table: "BrandActions");
        }
    }
}
