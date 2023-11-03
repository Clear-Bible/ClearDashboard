using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class TokenSplittingOriginTokenLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginTokenLocation",
                table: "TokenComponent",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_OriginTokenLocation",
                table: "TokenComponent",
                column: "OriginTokenLocation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TokenComponent_OriginTokenLocation",
                table: "TokenComponent");

            migrationBuilder.DropColumn(
                name: "OriginTokenLocation",
                table: "TokenComponent");
        }
    }
}
