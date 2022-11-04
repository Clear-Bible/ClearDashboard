using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    public partial class verseRow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PropertiesJson",
                table: "TokenComponent",
                newName: "ParallelCorpusId");

            migrationBuilder.AddColumn<string>(
                name: "ExtendedProperties",
                table: "TokenComponent",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VerseRowId",
                table: "TokenComponent",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "VerseRow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookChapterVerse = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalText = table.Column<string>(type: "TEXT", nullable: true),
                    IsSentenceStart = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsInRange = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsRangeStart = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEmpty = table.Column<bool>(type: "INTEGER", nullable: false),
                    TokenizationId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerseRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerseRow_TokenizedCorpus_TokenizationId",
                        column: x => x.TokenizationId,
                        principalTable: "TokenizedCorpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_ParallelCorpusId",
                table: "TokenComponent",
                column: "ParallelCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_VerseRowId",
                table: "TokenComponent",
                column: "VerseRowId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseRow_TokenizationId",
                table: "VerseRow",
                column: "TokenizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenComponent_ParallelCorpus_ParallelCorpusId",
                table: "TokenComponent",
                column: "ParallelCorpusId",
                principalTable: "ParallelCorpus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent",
                column: "VerseRowId",
                principalTable: "VerseRow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenComponent_ParallelCorpus_ParallelCorpusId",
                table: "TokenComponent");

            migrationBuilder.DropForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent");

            migrationBuilder.DropTable(
                name: "VerseRow");

            migrationBuilder.DropIndex(
                name: "IX_TokenComponent_ParallelCorpusId",
                table: "TokenComponent");

            migrationBuilder.DropIndex(
                name: "IX_TokenComponent_VerseRowId",
                table: "TokenComponent");

            migrationBuilder.DropColumn(
                name: "ExtendedProperties",
                table: "TokenComponent");

            migrationBuilder.DropColumn(
                name: "VerseRowId",
                table: "TokenComponent");

            migrationBuilder.RenameColumn(
                name: "ParallelCorpusId",
                table: "TokenComponent",
                newName: "PropertiesJson");
        }
    }
}
