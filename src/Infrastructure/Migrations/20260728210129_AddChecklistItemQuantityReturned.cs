using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklistItemQuantityReturned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuantityReturned",
                table: "ChecklistItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantityReturned",
                table: "ChecklistItems");
        }
    }
}
