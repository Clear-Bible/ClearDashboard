using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class index1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentSetId",
                table: "AlignmentTopTargetTrainingText");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentSetId_SourceTokenComponentId",
                table: "AlignmentTopTargetTrainingText",
                columns: new[] { "AlignmentSetId", "SourceTokenComponentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentSetId_SourceTokenComponentId",
                table: "AlignmentTopTargetTrainingText");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentSetId",
                table: "AlignmentTopTargetTrainingText",
                column: "AlignmentSetId");
        }
    }
}
