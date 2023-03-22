using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class collab : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent");

            migrationBuilder.AddColumn<long>(
                name: "LastTokenized",
                table: "TokenizedCorpus",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastMergedCommitSha",
                table: "Project",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent",
                column: "VerseRowId",
                principalTable: "VerseRow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent");

            migrationBuilder.DropColumn(
                name: "LastTokenized",
                table: "TokenizedCorpus");

            migrationBuilder.DropColumn(
                name: "LastMergedCommitSha",
                table: "Project");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent",
                column: "VerseRowId",
                principalTable: "VerseRow",
                principalColumn: "Id");
        }
    }
}
