using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace D.A.sneaker.Migrations
{
    /// <inheritdoc />
    public partial class FixRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_VariantId",
                table: "Orders",
                column: "VariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ProductVariants_VariantId",
                table: "Orders",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ProductVariants_VariantId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_VariantId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "Orders");
        }
    }
}
