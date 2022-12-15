using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    public partial class importByBook : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Deleted",
                table: "Translation",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Deleted",
                table: "TokenVerseAssociation",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Deleted",
                table: "TokenComponent",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Deleted",
                table: "Alignment",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Translation");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "TokenVerseAssociation");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "TokenComponent");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Alignment");
        }
    }
}
