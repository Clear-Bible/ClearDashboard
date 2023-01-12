using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class composites2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenComponent_TokenComponent_TokenCompositeId",
                table: "TokenComponent");

            migrationBuilder.DropIndex(
                name: "IX_TokenComponent_TokenCompositeId",
                table: "TokenComponent");

            migrationBuilder.DropColumn(
                name: "TokenCompositeId",
                table: "TokenComponent");

            migrationBuilder.CreateTable(
                name: "TokenCompositeTokenAssociation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenCompositeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Deleted = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenCompositeTokenAssociation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenCompositeTokenAssociation_TokenComponent_TokenCompositeId",
                        column: x => x.TokenCompositeId,
                        principalTable: "TokenComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TokenCompositeTokenAssociation_TokenComponent_TokenId",
                        column: x => x.TokenId,
                        principalTable: "TokenComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenCompositeTokenAssociation_TokenCompositeId",
                table: "TokenCompositeTokenAssociation",
                column: "TokenCompositeId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenCompositeTokenAssociation_TokenId",
                table: "TokenCompositeTokenAssociation",
                column: "TokenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenCompositeTokenAssociation");

            migrationBuilder.AddColumn<Guid>(
                name: "TokenCompositeId",
                table: "TokenComponent",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_TokenCompositeId",
                table: "TokenComponent",
                column: "TokenCompositeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenComponent_TokenComponent_TokenCompositeId",
                table: "TokenComponent",
                column: "TokenCompositeId",
                principalTable: "TokenComponent",
                principalColumn: "Id");
        }
    }
}
