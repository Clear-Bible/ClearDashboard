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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Corpus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParallelCorpus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParallelCorpus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectName = table.Column<string>(type: "TEXT", nullable: true),
                    IsRtl = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastContentWordLevel = table.Column<int>(type: "INTEGER", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    English = table.Column<string>(type: "TEXT", nullable: true),
                    AltText = table.Column<string>(type: "TEXT", nullable: true),
                    LastChanged = table.Column<double>(type: "REAL", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    LastAlignmentLevelId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VerseMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerseMapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CorpusVersion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsRtl = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<int>(type: "INTEGER", nullable: true),
                    ParatextGuid = table.Column<string>(type: "TEXT", nullable: true),
                    CorpusType = table.Column<int>(type: "INTEGER", nullable: false),
                    CorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorpusVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorpusVersion_Corpus_CorpusId",
                        column: x => x.CorpusId,
                        principalTable: "Corpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tokenization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenizationFunction = table.Column<string>(type: "TEXT", nullable: true),
                    CorpusId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokenization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tokenization_Corpus_CorpusId",
                        column: x => x.CorpusId,
                        principalTable: "Corpus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Verse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VerseNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    BookNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    SilBookNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    ChapterNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    VerseText = table.Column<string>(type: "TEXT", nullable: true),
                    CorpusId = table.Column<Guid>(type: "TEXT", nullable: true),
                    VerseBBBCCCVVV = table.Column<string>(type: "TEXT", nullable: true),
                    Found = table.Column<bool>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
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
                name: "ParallelCorpusVersion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlignmentType = table.Column<int>(type: "INTEGER", nullable: false),
                    LastGenerated = table.Column<long>(type: "INTEGER", nullable: false),
                    ParallelCorpusId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParallelCorpusVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParallelCorpusVersion_Corpus_SourceCorpusId",
                        column: x => x.SourceCorpusId,
                        principalTable: "Corpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParallelCorpusVersion_Corpus_TargetCorpusId",
                        column: x => x.TargetCorpusId,
                        principalTable: "Corpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParallelCorpusVersion_ParallelCorpus_ParallelCorpusId",
                        column: x => x.ParallelCorpusId,
                        principalTable: "ParallelCorpus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AlignmentSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlignmentSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlignmentSet_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AlignmentVersion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsDirty = table.Column<bool>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlignmentVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlignmentVersion_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Note",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuthorId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                name: "VerseMappingTokenizationsAssociation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceTokenizationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TargetTokenizationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    VerseMappingId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TokenizationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerseMappingTokenizationsAssociation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerseMappingTokenizationsAssociation_Tokenization_SourceTokenizationId",
                        column: x => x.SourceTokenizationId,
                        principalTable: "Tokenization",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VerseMappingTokenizationsAssociation_Tokenization_TargetTokenizationId",
                        column: x => x.TargetTokenizationId,
                        principalTable: "Tokenization",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VerseMappingTokenizationsAssociation_Tokenization_TokenizationId",
                        column: x => x.TokenizationId,
                        principalTable: "Tokenization",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VerseMappingTokenizationsAssociation_VerseMapping_VerseMappingId",
                        column: x => x.VerseMappingId,
                        principalTable: "VerseMapping",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Token",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WordNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SubwordNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    VerseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    FirstLetter = table.Column<string>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Token_Tokenization_TokenizationId",
                        column: x => x.TokenizationId,
                        principalTable: "Tokenization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Token_Verse_VerseId",
                        column: x => x.VerseId,
                        principalTable: "Verse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerseMappingVerseAssociation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VerseMappingId = table.Column<Guid>(type: "TEXT", nullable: true),
                    VerseId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerseMappingVerseAssociation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerseMappingVerseAssociation_Verse_VerseId",
                        column: x => x.VerseId,
                        principalTable: "Verse",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VerseMappingVerseAssociation_VerseMapping_VerseMappingId",
                        column: x => x.VerseMappingId,
                        principalTable: "VerseMapping",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NoteAssociation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssociationId = table.Column<string>(type: "TEXT", nullable: true),
                    AssociationType = table.Column<string>(type: "TEXT", nullable: false),
                    NoteId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserType = table.Column<int>(type: "INTEGER", nullable: false),
                    NoteId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bytes = table.Column<byte[]>(type: "BLOB", nullable: true),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    NoteId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                name: "Adornment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Lemma = table.Column<string>(type: "TEXT", nullable: true),
                    PartsOfSpeech = table.Column<string>(type: "TEXT", nullable: true),
                    Strong = table.Column<string>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceTokenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetTokenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Score = table.Column<decimal>(type: "TEXT", nullable: false),
                    AlignmentVersionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AlignmentType = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
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
                name: "AlignmentTokenPair",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceTokenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetTokenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlignmentType = table.Column<int>(type: "INTEGER", nullable: false),
                    AlignmentState = table.Column<int>(type: "INTEGER", nullable: false),
                    AlignmentSetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlignmentTokenPair", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlignmentTokenPair_AlignmentSet_AlignmentSetId",
                        column: x => x.AlignmentSetId,
                        principalTable: "AlignmentSet",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlignmentTokenPair_Token_SourceTokenId",
                        column: x => x.SourceTokenId,
                        principalTable: "Token",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlignmentTokenPair_Token_TargetTokenId",
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
                name: "IX_AlignmentSet_UserId",
                table: "AlignmentSet",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentTokenPair_AlignmentSetId",
                table: "AlignmentTokenPair",
                column: "AlignmentSetId");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentTokenPair_SourceTokenId",
                table: "AlignmentTokenPair",
                column: "SourceTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentTokenPair_TargetTokenId",
                table: "AlignmentTokenPair",
                column: "TargetTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentVersion_UserId",
                table: "AlignmentVersion",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CorpusVersion_CorpusId",
                table: "CorpusVersion",
                column: "CorpusId");

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
                name: "IX_ParallelCorpusVersion_ParallelCorpusId",
                table: "ParallelCorpusVersion",
                column: "ParallelCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelCorpusVersion_SourceCorpusId",
                table: "ParallelCorpusVersion",
                column: "SourceCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelCorpusVersion_TargetCorpusId",
                table: "ParallelCorpusVersion",
                column: "TargetCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_RawContent_NoteId",
                table: "RawContent",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Token_TokenizationId",
                table: "Token",
                column: "TokenizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Token_VerseId",
                table: "Token",
                column: "VerseId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokenization_CorpusId",
                table: "Tokenization",
                column: "CorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_Verse_CorpusId",
                table: "Verse",
                column: "CorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseMappingTokenizationsAssociation_SourceTokenizationId",
                table: "VerseMappingTokenizationsAssociation",
                column: "SourceTokenizationId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseMappingTokenizationsAssociation_TargetTokenizationId",
                table: "VerseMappingTokenizationsAssociation",
                column: "TargetTokenizationId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseMappingTokenizationsAssociation_TokenizationId",
                table: "VerseMappingTokenizationsAssociation",
                column: "TokenizationId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseMappingTokenizationsAssociation_VerseMappingId",
                table: "VerseMappingTokenizationsAssociation",
                column: "VerseMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseMappingVerseAssociation_VerseId",
                table: "VerseMappingVerseAssociation",
                column: "VerseId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseMappingVerseAssociation_VerseMappingId",
                table: "VerseMappingVerseAssociation",
                column: "VerseMappingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adornment");

            migrationBuilder.DropTable(
                name: "Alignment");

            migrationBuilder.DropTable(
                name: "AlignmentTokenPair");

            migrationBuilder.DropTable(
                name: "CorpusVersion");

            migrationBuilder.DropTable(
                name: "NoteAssociation");

            migrationBuilder.DropTable(
                name: "NoteRecipient");

            migrationBuilder.DropTable(
                name: "ParallelCorpusVersion");

            migrationBuilder.DropTable(
                name: "ProjectInfo");

            migrationBuilder.DropTable(
                name: "QuestionGroup");

            migrationBuilder.DropTable(
                name: "RawContent");

            migrationBuilder.DropTable(
                name: "VerseMappingTokenizationsAssociation");

            migrationBuilder.DropTable(
                name: "VerseMappingVerseAssociation");

            migrationBuilder.DropTable(
                name: "AlignmentVersion");

            migrationBuilder.DropTable(
                name: "AlignmentSet");

            migrationBuilder.DropTable(
                name: "Token");

            migrationBuilder.DropTable(
                name: "ParallelCorpus");

            migrationBuilder.DropTable(
                name: "Note");

            migrationBuilder.DropTable(
                name: "VerseMapping");

            migrationBuilder.DropTable(
                name: "Tokenization");

            migrationBuilder.DropTable(
                name: "Verse");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Corpus");
        }
    }
}
