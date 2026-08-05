using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActionSentAndPlannedTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "PlannedDepartureTime",
                table: "BrandActions",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PlannedReturnTime",
                table: "BrandActions",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Sent",
                table: "BrandActions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "BrandActions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SentBy",
                table: "BrandActions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedDepartureTime",
                table: "BrandActions");

            migrationBuilder.DropColumn(
                name: "PlannedReturnTime",
                table: "BrandActions");

            migrationBuilder.DropColumn(
                name: "Sent",
                table: "BrandActions");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "BrandActions");

            migrationBuilder.DropColumn(
                name: "SentBy",
                table: "BrandActions");
        }
    }
}
