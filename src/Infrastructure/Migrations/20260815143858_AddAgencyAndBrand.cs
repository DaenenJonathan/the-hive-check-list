using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyAndBrand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BrandId",
                table: "BrandActions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AgencyId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Agencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brands_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Data backfill - existing BrandActions carry a free-text "Client" column that
            // Agency/Brand now replace. One Agency + a same-named Brand per distinct Client value
            // preserves that data instead of discarding it; rows with no Client fall back to a
            // shared "Non défini" bucket so BrandId can become NOT NULL below without data loss.
            migrationBuilder.Sql(
                """
                INSERT INTO [Agencies] ([Id], [Name], [Color], [CreatedAt])
                SELECT NEWID(), c.[Client], N'#6B7280', SYSUTCDATETIME()
                FROM (SELECT DISTINCT [Client] FROM [BrandActions] WHERE [Client] IS NOT NULL AND LTRIM(RTRIM([Client])) <> '') c;

                INSERT INTO [Agencies] ([Id], [Name], [Color], [CreatedAt])
                SELECT NEWID(), N'Non défini', N'#6B7280', SYSUTCDATETIME()
                WHERE EXISTS (SELECT 1 FROM [BrandActions] WHERE [Client] IS NULL OR LTRIM(RTRIM([Client])) = '');

                INSERT INTO [Brands] ([Id], [Name], [AgencyId], [CreatedAt])
                SELECT NEWID(), a.[Name], a.[Id], SYSUTCDATETIME()
                FROM [Agencies] a;

                UPDATE ba
                SET ba.[BrandId] = b.[Id]
                FROM [BrandActions] ba
                JOIN [Brands] b ON b.[Name] = COALESCE(NULLIF(LTRIM(RTRIM(ba.[Client])), ''), N'Non défini');
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "BrandId",
                table: "BrandActions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Client",
                table: "BrandActions");

            migrationBuilder.CreateIndex(
                name: "IX_BrandActions_BrandId",
                table: "BrandActions",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AgencyId",
                table: "AspNetUsers",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_Name",
                table: "Agencies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brands_AgencyId",
                table: "Brands",
                column: "AgencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Agencies_AgencyId",
                table: "AspNetUsers",
                column: "AgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BrandActions_Brands_BrandId",
                table: "BrandActions",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Agencies_AgencyId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_BrandActions_Brands_BrandId",
                table: "BrandActions");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Agencies");

            migrationBuilder.DropIndex(
                name: "IX_BrandActions_BrandId",
                table: "BrandActions");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AgencyId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "BrandActions");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Client",
                table: "BrandActions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
