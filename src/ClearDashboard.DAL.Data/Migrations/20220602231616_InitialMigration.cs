using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
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
                    IsRtl = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Language = table.Column<int>(type: "INTEGER", nullable: true),
                    ParatextGuid = table.Column<string>(type: "TEXT", nullable: false),
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
                    IsRtl = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastContentWordLevel = table.Column<int>(type: "INTEGER", nullable: true),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    English = table.Column<string>(type: "TEXT", nullable: true),
                    AltText = table.Column<string>(type: "TEXT", nullable: true),
                    LastChanged = table.Column<double>(type: "REAL", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    LastAlignmentLevelId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParallelCorpus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceCorpusId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetCorpusId = table.Column<int>(type: "INTEGER", nullable: false),
                    AlignmentType = table.Column<int>(type: "INTEGER", nullable: false),
                    LastGenerated = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<long>(type: "INTEGER", nullable: false)
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
                    SilBookNumber = table.Column<string>(type: "TEXT", nullable: true),
                    ChapterNumber = table.Column<string>(type: "TEXT", nullable: true),
                    VerseText = table.Column<string>(type: "TEXT", nullable: true),
                    CorpusId = table.Column<int>(type: "INTEGER", nullable: true),
                    VerseBBCCCVVV = table.Column<string>(type: "TEXT", nullable: true),
                    VerseId = table.Column<string>(type: "TEXT", nullable: false),
                    Found = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<long>(type: "INTEGER", nullable: false)
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
                name: "AlignmentVersion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDirty = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<long>(type: "INTEGER", nullable: false)
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
                name: "Note",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AuthorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Note", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Note_User_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParallelVersesLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParallelCorpusId = table.Column<int>(type: "INTEGER", nullable: true),
                    VerseId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParallelVersesLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParallelVersesLink_ParallelCorpus_ParallelCorpusId",
                        column: x => x.ParallelCorpusId,
                        principalTable: "ParallelCorpus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ParallelVersesLink_Verse_VerseId",
                        column: x => x.VerseId,
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
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    FirstLetter = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "NoteAssociation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssociationId = table.Column<string>(type: "TEXT", nullable: true),
                    AssociationType = table.Column<string>(type: "TEXT", nullable: false),
                    NoteId = table.Column<int>(type: "INTEGER", nullable: true),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteAssociation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoteAssociation_Note_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Note",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NoteRecipient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserType = table.Column<int>(type: "INTEGER", nullable: false),
                    NoteId = table.Column<int>(type: "INTEGER", nullable: true),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteRecipient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoteRecipient_Note_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Note",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NoteRecipient_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RawContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bytes = table.Column<byte[]>(type: "BLOB", nullable: true),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    NoteId = table.Column<int>(type: "INTEGER", nullable: true),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawContent_Note_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Note",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VerseLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VerseId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParallelVersesLinkId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSource = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerseLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerseLink_ParallelVersesLink_ParallelVersesLinkId",
                        column: x => x.ParallelVersesLinkId,
                        principalTable: "ParallelVersesLink",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VerseLink_Verse_VerseId",
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
                    Lemma = table.Column<string>(type: "TEXT", nullable: true),
                    PartsOfSpeech = table.Column<string>(type: "TEXT", nullable: true),
                    Strong = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "Alignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceTokenId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetTokenId = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<decimal>(type: "TEXT", nullable: false),
                    AlignmentVersionId = table.Column<int>(type: "INTEGER", nullable: true),
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
                column: "SourceTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_Alignment_TargetTokenId",
                table: "Alignment",
                column: "TargetTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentVersion_UserId",
                table: "AlignmentVersion",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Note_AuthorId",
                table: "Note",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteAssociation_NoteId",
                table: "NoteAssociation",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteRecipient_NoteId",
                table: "NoteRecipient",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteRecipient_UserId",
                table: "NoteRecipient",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelCorpus_SourceCorpusId",
                table: "ParallelCorpus",
                column: "SourceCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelCorpus_TargetCorpusId",
                table: "ParallelCorpus",
                column: "TargetCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelVersesLink_ParallelCorpusId",
                table: "ParallelVersesLink",
                column: "ParallelCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelVersesLink_VerseId",
                table: "ParallelVersesLink",
                column: "VerseId");

            migrationBuilder.CreateIndex(
                name: "IX_RawContent_NoteId",
                table: "RawContent",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Token_VerseId",
                table: "Token",
                column: "VerseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Verse_CorpusId",
                table: "Verse",
                column: "CorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseLink_ParallelVersesLinkId",
                table: "VerseLink",
                column: "ParallelVersesLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseLink_VerseId",
                table: "VerseLink",
                column: "VerseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adornment");

            migrationBuilder.DropTable(
                name: "Alignment");

            migrationBuilder.DropTable(
                name: "NoteAssociation");

            migrationBuilder.DropTable(
                name: "NoteRecipient");

            migrationBuilder.DropTable(
                name: "ProjectInfo");

            migrationBuilder.DropTable(
                name: "QuestionGroup");

            migrationBuilder.DropTable(
                name: "RawContent");

            migrationBuilder.DropTable(
                name: "VerseLink");

            migrationBuilder.DropTable(
                name: "AlignmentVersion");

            migrationBuilder.DropTable(
                name: "Token");

            migrationBuilder.DropTable(
                name: "Note");

            migrationBuilder.DropTable(
                name: "ParallelVersesLink");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "ParallelCorpus");

            migrationBuilder.DropTable(
                name: "Verse");

            migrationBuilder.DropTable(
                name: "Corpus");
        }
    }
}
