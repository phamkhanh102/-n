using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace D.A.sneaker.Migrations
{
    /// <inheritdoc />
    public partial class AddChatState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContextProductId",
                table: "ChatHistories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserChatStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CurrentProductId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChatStates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserChatStates");

            migrationBuilder.DropColumn(
                name: "ContextProductId",
                table: "ChatHistories");
        }
    }
}
