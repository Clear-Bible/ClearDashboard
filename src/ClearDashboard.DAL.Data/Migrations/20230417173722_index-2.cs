using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class index2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VerseRow_BookChapterVerse",
                table: "VerseRow",
                column: "BookChapterVerse");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentSetId_SourceTrainingText",
                table: "AlignmentTopTargetTrainingText",
                columns: new[] { "AlignmentSetId", "SourceTrainingText" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VerseRow_BookChapterVerse",
                table: "VerseRow");

            migrationBuilder.DropIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentSetId_SourceTrainingText",
                table: "AlignmentTopTargetTrainingText");
        }
    }
}
