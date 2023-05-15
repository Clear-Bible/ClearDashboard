using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class bulkalignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BBBCCCVVV",
                table: "Verse",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Verse_BBBCCCVVV",
                table: "Verse",
                column: "BBBCCCVVV");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Verse_BBBCCCVVV",
                table: "Verse");

            migrationBuilder.DropColumn(
                name: "BBBCCCVVV",
                table: "Verse");
        }
    }
}
