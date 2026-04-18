using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace D.A.sneaker.Migrations
{
    /// <inheritdoc />
    public partial class AddCostPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "Products",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2026, 4, 9, 11, 45, 3, 363, DateTimeKind.Local).AddTicks(1127), "$2a$11$HLkVci2ZVdq.09iHE8zWIuW3MbOjctW.ukTytzaftf0yZdMG3zDby" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "Products");

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "Description", "IsActive", "MainImage", "Name", "Price", "Rating", "SoldCount" },
                values: new object[,]
                {
                    { 1, "Nike", 2, "Classic Nike sneaker", true, "Nike/nike-af1.jpg", "Nike Air Force 1", 3200000m, 0.0, 0 },
                    { 2, "Adidas", 1, "Lightweight running shoes", true, "Adidas/AdidasMensRunFalcon5.jpg", "Adidas Run Falcon 5", 2100000m, 0.0, 0 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2026, 3, 21, 14, 22, 32, 17, DateTimeKind.Local).AddTicks(5567), "$2a$11$FvYfpa3OcDPgDIczONvBhujFx4c4SEZbGfiwYwPUMH4qVpmZeB1hK" });

            migrationBuilder.InsertData(
                table: "ProductImages",
                columns: new[] { "Id", "ImageUrl", "ProductId" },
                values: new object[,]
                {
                    { 1, "Nike/af1-1.jpg", 1 },
                    { 2, "Adidas/AdidasMensRunFalcon5.jpg", 2 }
                });
        }
    }
}
