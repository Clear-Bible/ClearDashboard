using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class lexiconTranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LexiconTranslationId",
                table: "Translation",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translation_LexiconTranslationId",
                table: "Translation",
                column: "LexiconTranslationId");

            migrationBuilder.CreateIndex(
                name: "IX_Label_Text",
                table: "Label",
                column: "Text",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Translation_Lexicon_Translation_LexiconTranslationId",
                table: "Translation",
                column: "LexiconTranslationId",
                principalTable: "Lexicon_Translation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Translation_Lexicon_Translation_LexiconTranslationId",
                table: "Translation");

            migrationBuilder.DropIndex(
                name: "IX_Translation_LexiconTranslationId",
                table: "Translation");

            migrationBuilder.DropIndex(
                name: "IX_Label_Text",
                table: "Label");

            migrationBuilder.DropColumn(
                name: "LexiconTranslationId",
                table: "Translation");
        }
    }
}
