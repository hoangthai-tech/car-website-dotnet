using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddTermsOfService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TermsOfServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermsOfServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermsOfServices_Users_PublishedById",
                        column: x => x.PublishedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserTermAgreements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermsOfServiceId = table.Column<int>(type: "int", nullable: false),
                    AgreedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTermAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTermAgreements_TermsOfServices_TermsOfServiceId",
                        column: x => x.TermsOfServiceId,
                        principalTable: "TermsOfServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTermAgreements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TermsOfServices_PublishedById",
                table: "TermsOfServices",
                column: "PublishedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserTermAgreements_TermsOfServiceId",
                table: "UserTermAgreements",
                column: "TermsOfServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTermAgreements_UserId_TermsOfServiceId",
                table: "UserTermAgreements",
                columns: new[] { "UserId", "TermsOfServiceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTermAgreements");

            migrationBuilder.DropTable(
                name: "TermsOfServices");
        }
    }
}
