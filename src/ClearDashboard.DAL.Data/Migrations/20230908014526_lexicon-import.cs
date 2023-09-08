using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class lexiconimport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginatedFrom",
                table: "Lexicon_Translation",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Created",
                table: "Lexicon_Form",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Lexicon_Form",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("UPDATE Lexicon_Form SET (UserId, Created) = (SELECT UserId, Created FROM Project LIMIT 1);");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Form_UserId",
                table: "Lexicon_Form",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lexicon_Form_User_UserId",
                table: "Lexicon_Form",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lexicon_Form_User_UserId",
                table: "Lexicon_Form");

            migrationBuilder.DropIndex(
                name: "IX_Lexicon_Form_UserId",
                table: "Lexicon_Form");

            migrationBuilder.DropColumn(
                name: "OriginatedFrom",
                table: "Lexicon_Translation");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Lexicon_Form");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Lexicon_Form");
        }
    }
}
