using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    public partial class alignmentSetDenormalization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlignmentSetDenormalizationTask",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlignmentSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceText = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlignmentSetDenormalizationTask", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlignmentSetDenormalizationTask_AlignmentSet_AlignmentSetId",
                        column: x => x.AlignmentSetId,
                        principalTable: "AlignmentSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlignmentTopTargetTrainingText",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlignmentSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlignmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceTokenComponentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceTrainingText = table.Column<string>(type: "TEXT", nullable: false),
                    TopTargetTrainingText = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlignmentTopTargetTrainingText", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlignmentTopTargetTrainingText_Alignment_AlignmentId",
                        column: x => x.AlignmentId,
                        principalTable: "Alignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlignmentTopTargetTrainingText_AlignmentSet_AlignmentSetId",
                        column: x => x.AlignmentSetId,
                        principalTable: "AlignmentSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlignmentTopTargetTrainingText_TokenComponent_SourceTokenComponentId",
                        column: x => x.SourceTokenComponentId,
                        principalTable: "TokenComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Verse_BookNumber",
                table: "Verse",
                column: "BookNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Verse_ChapterNumber",
                table: "Verse",
                column: "ChapterNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Verse_VerseNumber",
                table: "Verse",
                column: "VerseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentSetDenormalizationTask_AlignmentSetId",
                table: "AlignmentSetDenormalizationTask",
                column: "AlignmentSetId");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentId",
                table: "AlignmentTopTargetTrainingText",
                column: "AlignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentSetId",
                table: "AlignmentTopTargetTrainingText",
                column: "AlignmentSetId");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentTopTargetTrainingText_SourceTokenComponentId",
                table: "AlignmentTopTargetTrainingText",
                column: "SourceTokenComponentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlignmentSetDenormalizationTask");

            migrationBuilder.DropTable(
                name: "AlignmentTopTargetTrainingText");

            migrationBuilder.DropIndex(
                name: "IX_Verse_BookNumber",
                table: "Verse");

            migrationBuilder.DropIndex(
                name: "IX_Verse_ChapterNumber",
                table: "Verse");

            migrationBuilder.DropIndex(
                name: "IX_Verse_VerseNumber",
                table: "Verse");
        }
    }
}
