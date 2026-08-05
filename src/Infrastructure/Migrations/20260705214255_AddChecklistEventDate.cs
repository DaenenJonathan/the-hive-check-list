using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklistEventDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EventDate",
                table: "Checklists",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventDate",
                table: "Checklists");
        }
    }
}
