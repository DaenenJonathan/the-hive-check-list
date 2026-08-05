using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandActionAddressAndCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "BrandActions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "BrandActions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "BrandActions");

            migrationBuilder.DropColumn(
                name: "City",
                table: "BrandActions");
        }
    }
}
