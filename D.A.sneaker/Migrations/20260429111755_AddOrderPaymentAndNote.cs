using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace D.A.sneaker.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPaymentAndNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2026, 4, 29, 18, 17, 54, 794, DateTimeKind.Local).AddTicks(6352), "$2a$11$7QhQWngLQ8qTAx/0orK1hu1saSp8sJQ80D447AmYNIia94aOnPtlW" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2026, 4, 9, 23, 37, 0, 960, DateTimeKind.Local).AddTicks(4525), "$2a$11$6PF17ojtfXwy7bBFd3H6burdyx2fhjs1EjAdO6ym6smNLFzm8mYK2" });
        }
    }
}
