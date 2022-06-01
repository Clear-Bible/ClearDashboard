using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    public partial class AddRawContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "RawContent",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "RawContent",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "RawContent");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "RawContent",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
