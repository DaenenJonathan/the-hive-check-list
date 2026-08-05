using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActionReturnValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReturnValidated",
                table: "BrandActions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnValidatedAt",
                table: "BrandActions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnValidatedBy",
                table: "BrandActions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnValidated",
                table: "BrandActions");

            migrationBuilder.DropColumn(
                name: "ReturnValidatedAt",
                table: "BrandActions");

            migrationBuilder.DropColumn(
                name: "ReturnValidatedBy",
                table: "BrandActions");
        }
    }
}
