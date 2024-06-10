using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenComponentTypeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_Type",
                table: "TokenComponent",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TokenComponent_Type",
                table: "TokenComponent");
        }
    }
}
