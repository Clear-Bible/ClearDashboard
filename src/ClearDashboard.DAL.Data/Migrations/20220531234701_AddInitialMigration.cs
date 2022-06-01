using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    public partial class AddInitialMigration : Migration
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
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Language = table.Column<int>(type: "INTEGER", nullable: true),
                    ParatextGUID = table.Column<string>(type: "varchar(250)", nullable: false),
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
                    Created = table.Column<long>(type: "datetime", nullable: false),
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
                    AlignmentType = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<long>(type: "datetime", nullable: false),
                    LastGenerated = table.Column<long>(type: "datetime", nullable: false)
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
                    Modified = table.Column<long>(type: "datetime", nullable: true),
                    CorpusId = table.Column<int>(type: "INTEGER", nullable: true),
                    VerseBBCCCVVV = table.Column<string>(type: "TEXT", nullable: true),
                    VerseId = table.Column<string>(type: "TEXT", nullable: false),
                    Found = table.Column<bool>(type: "INTEGER", nullable: false)
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
                name: "VerseLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VerseId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParallelVersesLinkId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSource = table.Column<bool>(type: "INTEGER", nullable: false)
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
                name: "InterlinearNote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TokenId = table.Column<int>(type: "INTEGER", nullable: true),
                    Note = table.Column<string>(type: "varchar(600)", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<long>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterlinearNote", x => x.Id);
                    table.UniqueConstraint("AK_InterlinearNote_UserId", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_InterlinearNote_Token_TokenId",
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
                        name: "FK_User_InterlinearNote_Id",
                        column: x => x.Id,
                        principalTable: "InterlinearNote",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "AlignmentVersion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
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
                name: "Note",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<long>(type: "datetime", nullable: false),
                    Modified = table.Column<long>(type: "datetime", nullable: false),
                    AuthorId = table.Column<int>(type: "INTEGER", nullable: false)
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
                name: "Alignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceTokenId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetTokenId = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(3)", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "RecipientNoteUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserType = table.Column<int>(type: "INTEGER", nullable: false),
                    NoteId = table.Column<int>(type: "INTEGER", nullable: true),
                    Discriminator = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipientNoteUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipientNoteUser_Note_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Note",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RecipientNoteUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
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
                name: "IX_InterlinearNote_TokenId",
                table: "InterlinearNote",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_InterlinearNote_UserId",
                table: "InterlinearNote",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Note_AuthorId",
                table: "Note",
                column: "AuthorId");

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
                name: "IX_ParallelVersesLink_ParallelCorpusId",
                table: "ParallelVersesLink",
                column: "ParallelCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelVersesLink_VerseId",
                table: "ParallelVersesLink",
                column: "VerseId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipientNoteUser_NoteId",
                table: "RecipientNoteUser",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipientNoteUser_UserId",
                table: "RecipientNoteUser",
                column: "UserId");

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
                name: "ProjectInfo");

            migrationBuilder.DropTable(
                name: "QuestionGroup");

            migrationBuilder.DropTable(
                name: "RecipientNoteUser");

            migrationBuilder.DropTable(
                name: "VerseLink");

            migrationBuilder.DropTable(
                name: "AlignmentVersion");

            migrationBuilder.DropTable(
                name: "Note");

            migrationBuilder.DropTable(
                name: "ParallelVersesLink");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "ParallelCorpus");

            migrationBuilder.DropTable(
                name: "InterlinearNote");

            migrationBuilder.DropTable(
                name: "Token");

            migrationBuilder.DropTable(
                name: "Verse");

            migrationBuilder.DropTable(
                name: "Corpus");
        }
    }
}
