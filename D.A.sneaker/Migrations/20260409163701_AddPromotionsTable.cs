using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace D.A.sneaker.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    BannerImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2026, 4, 9, 23, 37, 0, 960, DateTimeKind.Local).AddTicks(4525), "$2a$11$6PF17ojtfXwy7bBFd3H6burdyx2fhjs1EjAdO6ym6smNLFzm8mYK2" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2026, 4, 9, 11, 45, 3, 363, DateTimeKind.Local).AddTicks(1127), "$2a$11$HLkVci2ZVdq.09iHE8zWIuW3MbOjctW.ukTytzaftf0yZdMG3zDby" });
        }
    }
}
