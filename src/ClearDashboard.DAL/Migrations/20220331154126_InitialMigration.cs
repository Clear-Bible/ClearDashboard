using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Corpus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsRTL = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: true),
                    Language = table.Column<int>(type: "INTEGER", nullable: true),
                    ParatextGUID = table.Column<string>(type: "varchar(250)", nullable: true),
                    CorpusTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    CorpusType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Corpus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectName = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsRTL = table.Column<bool>(type: "bit", nullable: false),
                    LastContentWordLevel = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionGroup",
                columns: table => new
                {
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    English = table.Column<string>(type: "TEXT", nullable: true),
                    AltText = table.Column<string>(type: "TEXT", nullable: true),
                    LastChanged = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "ParallelCorpus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceCorpusId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetCorpusId = table.Column<int>(type: "INTEGER", nullable: false),
                    AlignmentTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastGenerated = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParallelCorpus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParallelCorpus_Corpus_SourceCorpusId",
                        column: x => x.SourceCorpusId,
                        principalTable: "Corpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParallelCorpus_Corpus_TargetCorpusId",
                        column: x => x.TargetCorpusId,
                        principalTable: "Corpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Verse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VerseNumber = table.Column<string>(type: "TEXT", nullable: true),
                    SilBookNumber = table.Column<string>(type: "varchar(2)", nullable: true),
                    ChapterNumber = table.Column<string>(type: "TEXT", nullable: true),
                    VerseText = table.Column<string>(type: "text", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "datetime", nullable: false),
                    CorpusId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Verse_Corpus_CorpusId",
                        column: x => x.CorpusId,
                        principalTable: "Corpus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ParallelVerse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceVerseId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetVerseId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParallelCorpusId = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetVersenId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParallelVerse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParallelVerse_ParallelCorpus_ParallelCorpusId",
                        column: x => x.ParallelCorpusId,
                        principalTable: "ParallelCorpus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ParallelVerse_Verse_SourceVerseId",
                        column: x => x.SourceVerseId,
                        principalTable: "Verse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParallelVerse_Verse_TargetVersenId",
                        column: x => x.TargetVersenId,
                        principalTable: "Verse",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Token",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WordNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SubwordNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    VerseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "varchar(250)", nullable: true),
                    FirstLetter = table.Column<string>(type: "varchar(2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Token_Verse_VerseId",
                        column: x => x.VerseId,
                        principalTable: "Verse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Adornment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TokenId = table.Column<int>(type: "INTEGER", nullable: true),
                    Lemma = table.Column<string>(type: "varchar(50)", nullable: true),
                    PartsOfSpeech = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false),
                    Strong = table.Column<string>(type: "varchar(15)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adornment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Adornment_Token_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Token",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InterlinearNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TokenId = table.Column<int>(type: "INTEGER", nullable: true),
                    Note = table.Column<string>(type: "varchar(600)", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterlinearNotes", x => x.Id);
                    table.UniqueConstraint("AK_InterlinearNotes_UserId", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_InterlinearNotes_Token_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Token",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    ParatextUsername = table.Column<string>(type: "varchar(100)", nullable: true),
                    LastAlignmentLevelId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_InterlinearNotes_Id",
                        column: x => x.Id,
                        principalTable: "InterlinearNotes",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "AlignmentVersion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDirty = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlignmentVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlignmentVersion_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Alignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceTokenId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetTokenId = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(3)", nullable: false),
                    AlignmentVersionId = table.Column<int>(type: "INTEGER", nullable: true),
                    AlignmentTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    AlignmentType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alignment_AlignmentVersion_AlignmentVersionId",
                        column: x => x.AlignmentVersionId,
                        principalTable: "AlignmentVersion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Alignment_Token_SourceTokenId",
                        column: x => x.SourceTokenId,
                        principalTable: "Token",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alignment_Token_TargetTokenId",
                        column: x => x.TargetTokenId,
                        principalTable: "Token",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adornment_TokenId",
                table: "Adornment",
                column: "TokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alignment_AlignmentVersionId",
                table: "Alignment",
                column: "AlignmentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Alignment_SourceTokenId",
                table: "Alignment",
                column: "SourceTokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alignment_TargetTokenId",
                table: "Alignment",
                column: "TargetTokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentVersion_UserId",
                table: "AlignmentVersion",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InterlinearNotes_TokenId",
                table: "InterlinearNotes",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_InterlinearNotes_UserId",
                table: "InterlinearNotes",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParallelCorpus_SourceCorpusId",
                table: "ParallelCorpus",
                column: "SourceCorpusId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParallelCorpus_TargetCorpusId",
                table: "ParallelCorpus",
                column: "TargetCorpusId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParallelVerse_ParallelCorpusId",
                table: "ParallelVerse",
                column: "ParallelCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelVerse_SourceVerseId",
                table: "ParallelVerse",
                column: "SourceVerseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParallelVerse_TargetVerseId",
                table: "ParallelVerse",
                column: "TargetVerseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParallelVerse_TargetVersenId",
                table: "ParallelVerse",
                column: "TargetVersenId");

            migrationBuilder.CreateIndex(
                name: "IX_Token_VerseId",
                table: "Token",
                column: "VerseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Verse_CorpusId",
                table: "Verse",
                column: "CorpusId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adornment");

            migrationBuilder.DropTable(
                name: "Alignment");

            migrationBuilder.DropTable(
                name: "ParallelVerse");

            migrationBuilder.DropTable(
                name: "ProjectInfo");

            migrationBuilder.DropTable(
                name: "QuestionGroup");

            migrationBuilder.DropTable(
                name: "AlignmentVersion");

            migrationBuilder.DropTable(
                name: "ParallelCorpus");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "InterlinearNotes");

            migrationBuilder.DropTable(
                name: "Token");

            migrationBuilder.DropTable(
                name: "Verse");

            migrationBuilder.DropTable(
                name: "Corpus");
        }
    }
}
