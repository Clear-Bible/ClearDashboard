using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class indexes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentSetId",
                table: "AlignmentTopTargetTrainingText",
                column: "AlignmentSetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentSetId",
                table: "AlignmentTopTargetTrainingText");
        }
    }
}
