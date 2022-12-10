using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    public partial class compositeVerseRowNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent");

            migrationBuilder.AlterColumn<Guid>(
                name: "VerseRowId",
                table: "TokenComponent",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent",
                column: "VerseRowId",
                principalTable: "VerseRow",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent");

            migrationBuilder.AlterColumn<Guid>(
                name: "VerseRowId",
                table: "TokenComponent",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent",
                column: "VerseRowId",
                principalTable: "VerseRow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
