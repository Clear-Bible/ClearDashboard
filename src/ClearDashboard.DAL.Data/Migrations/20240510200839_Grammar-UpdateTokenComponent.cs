using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class GrammarUpdateTokenComponent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CircumfixGroup",
                table: "TokenComponent",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GrammarId",
                table: "TokenComponent",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Grammar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
					Precedence = table.Column<int>(type: "INTEGER", nullable: false, defaultValue:1000)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grammar", x => x.Id);
                    table.UniqueConstraint("AK_Grammar_ShortName", x => x.ShortName);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_CircumfixGroup",
                table: "TokenComponent",
                column: "CircumfixGroup");

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_GrammarId",
                table: "TokenComponent",
                column: "GrammarId");

            migrationBuilder.CreateIndex(
                name: "IX_Grammar_ShortName",
                table: "Grammar",
                column: "ShortName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Grammar");

            migrationBuilder.DropIndex(
                name: "IX_TokenComponent_CircumfixGroup",
                table: "TokenComponent");

            migrationBuilder.DropIndex(
                name: "IX_TokenComponent_GrammarId",
                table: "TokenComponent");

            migrationBuilder.DropColumn(
                name: "CircumfixGroup",
                table: "TokenComponent");

            migrationBuilder.DropColumn(
                name: "GrammarId",
                table: "TokenComponent");
        }
    }
}
