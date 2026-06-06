using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderType",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "RentalDailyRate",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "RentalEndDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RentalStartDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RentalDailyRate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RentalEndDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RentalStartDate",
                table: "Orders");
        }
    }
}
