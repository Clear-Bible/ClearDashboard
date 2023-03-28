using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class indexes1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Translation_TranslationSetId",
                table: "Translation");

            migrationBuilder.DropIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentSetId",
                table: "AlignmentTopTargetTrainingText");

            migrationBuilder.DropIndex(
                name: "IX_Alignment_AlignmentSetId",
                table: "Alignment");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_TranslationSetId_SourceTokenComponentId",
                table: "Translation",
                columns: new[] { "TranslationSetId", "SourceTokenComponentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Meaning_Language",
                table: "Lexicon_Meaning",
                column: "Language",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentSetId_SourceTokenComponentId",
                table: "AlignmentTopTargetTrainingText",
                columns: new[] { "AlignmentSetId", "SourceTokenComponentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Alignment_AlignmentSetId_SourceTokenComponentId",
                table: "Alignment",
                columns: new[] { "AlignmentSetId", "SourceTokenComponentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Translation_TranslationSetId_SourceTokenComponentId",
                table: "Translation");

            migrationBuilder.DropIndex(
                name: "IX_Lexicon_Meaning_Language",
                table: "Lexicon_Meaning");

            migrationBuilder.DropIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentSetId_SourceTokenComponentId",
                table: "AlignmentTopTargetTrainingText");

            migrationBuilder.DropIndex(
                name: "IX_Alignment_AlignmentSetId_SourceTokenComponentId",
                table: "Alignment");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_TranslationSetId",
                table: "Translation",
                column: "TranslationSetId");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentSetId",
                table: "AlignmentTopTargetTrainingText",
                column: "AlignmentSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Alignment_AlignmentSetId",
                table: "Alignment",
                column: "AlignmentSetId");
        }
    }
}
