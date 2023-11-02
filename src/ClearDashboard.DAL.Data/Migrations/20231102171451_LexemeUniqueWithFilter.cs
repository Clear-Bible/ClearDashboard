using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class LexemeUniqueWithFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lexicon_Lexeme_Lemma_Type_Language",
                table: "Lexicon_Lexeme");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Lexeme_Lemma_Language",
                table: "Lexicon_Lexeme",
                columns: new[] { "Lemma", "Language" },
                unique: true,
                filter: "Type IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Lexeme_Lemma_Type_Language",
                table: "Lexicon_Lexeme",
                columns: new[] { "Lemma", "Type", "Language" },
                unique: true,
                filter: "Type IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lexicon_Lexeme_Lemma_Language",
                table: "Lexicon_Lexeme");

            migrationBuilder.DropIndex(
                name: "IX_Lexicon_Lexeme_Lemma_Type_Language",
                table: "Lexicon_Lexeme");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Lexeme_Lemma_Type_Language",
                table: "Lexicon_Lexeme",
                columns: new[] { "Lemma", "Type", "Language" },
                unique: true);
        }
    }
}
