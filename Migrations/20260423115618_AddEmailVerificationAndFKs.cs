using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationAndFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationExpiry",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CarId",
                table: "Orders",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StaffId",
                table: "Orders",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRequests_OrderId",
                table: "DiscountRequests",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRequests_ReviewedById",
                table: "DiscountRequests",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRequests_StaffId",
                table: "DiscountRequests",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountRequests_Orders_OrderId",
                table: "DiscountRequests",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountRequests_Users_ReviewedById",
                table: "DiscountRequests",
                column: "ReviewedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountRequests_Users_StaffId",
                table: "DiscountRequests",
                column: "StaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Cars_CarId",
                table: "Orders",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_StaffId",
                table: "Orders",
                column: "StaffId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscountRequests_Orders_OrderId",
                table: "DiscountRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscountRequests_Users_ReviewedById",
                table: "DiscountRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscountRequests_Users_StaffId",
                table: "DiscountRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Cars_CarId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_StaffId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CarId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_StaffId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_DiscountRequests_OrderId",
                table: "DiscountRequests");

            migrationBuilder.DropIndex(
                name: "IX_DiscountRequests_ReviewedById",
                table: "DiscountRequests");

            migrationBuilder.DropIndex(
                name: "IX_DiscountRequests_StaffId",
                table: "DiscountRequests");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VerificationExpiry",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VerificationToken",
                table: "Users");
        }
    }
}
