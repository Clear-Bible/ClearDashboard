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
                    IsRtl = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    ParatextGuid = table.Column<string>(type: "TEXT", nullable: true),
                    CorpusType = table.Column<int>(type: "INTEGER", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Corpus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CorpusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsRtl = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    ParatextGuid = table.Column<string>(type: "TEXT", nullable: true),
                    CorpusType = table.Column<int>(type: "INTEGER", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorpusHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EngineWordAlignment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SmtWordAlignerType = table.Column<string>(type: "TEXT", nullable: true),
                    IsClearAligner = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineWordAlignment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectName = table.Column<string>(type: "TEXT", nullable: true),
                    IsRtl = table.Column<bool>(type: "INTEGER", nullable: false),
                    DesignSurfaceLayout = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
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
                name: "TokenizedCorpus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CorpusHistoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TokenizationFunction = table.Column<string>(type: "TEXT", nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenizedCorpus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenizedCorpus_Corpus_CorpusId",
                        column: x => x.CorpusId,
                        principalTable: "Corpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TokenizedCorpus_CorpusHistory_CorpusHistoryId",
                        column: x => x.CorpusHistoryId,
                        principalTable: "CorpusHistory",
                        principalColumn: "Id");
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
                name: "ParallelCorpus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlignmentType = table.Column<int>(type: "INTEGER", nullable: false),
                    LastGenerated = table.Column<long>(type: "INTEGER", nullable: false),
                    SourceTokenizedCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetTokenizedCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParallelCorpus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParallelCorpus_TokenizedCorpus_SourceTokenizedCorpusId",
                        column: x => x.SourceTokenizedCorpusId,
                        principalTable: "TokenizedCorpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParallelCorpus_TokenizedCorpus_TargetTokenizedCorpusId",
                        column: x => x.TargetTokenizedCorpusId,
                        principalTable: "TokenizedCorpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParallelCorpusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlignmentType = table.Column<int>(type: "INTEGER", nullable: false),
                    LastGenerated = table.Column<long>(type: "INTEGER", nullable: false),
                    SourceTokenizedCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetTokenizedCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParallelCorpusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParallelCorpusHistory_TokenizedCorpus_SourceTokenizedCorpusId",
                        column: x => x.SourceTokenizedCorpusId,
                        principalTable: "TokenizedCorpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParallelCorpusHistory_TokenizedCorpus_TargetTokenizedCorpusId",
                        column: x => x.TargetTokenizedCorpusId,
                        principalTable: "TokenizedCorpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Token",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    ChapterNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    VerseNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    WordNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SubwordNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    TokenizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SurfaceText = table.Column<string>(type: "TEXT", nullable: true),
                    TrainingText = table.Column<string>(type: "TEXT", nullable: true),
                    TokenCompositeId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Token_TokenizedCorpus_TokenizationId",
                        column: x => x.TokenizationId,
                        principalTable: "TokenizedCorpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "TranslationSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EngineWordAlignmentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DerivedFromId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParallelCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TranslationSet_EngineWordAlignment_EngineWordAlignmentId",
                        column: x => x.EngineWordAlignmentId,
                        principalTable: "EngineWordAlignment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TranslationSet_ParallelCorpus_ParallelCorpusId",
                        column: x => x.ParallelCorpusId,
                        principalTable: "ParallelCorpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TranslationSet_TranslationSet_DerivedFromId",
                        column: x => x.DerivedFromId,
                        principalTable: "TranslationSet",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TranslationSet_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AlignmentSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EngineWordAlignmentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    ParallelCorpusHistoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParallelCorpusId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlignmentSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlignmentSet_EngineWordAlignment_EngineWordAlignmentId",
                        column: x => x.EngineWordAlignmentId,
                        principalTable: "EngineWordAlignment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlignmentSet_ParallelCorpus_ParallelCorpusId",
                        column: x => x.ParallelCorpusId,
                        principalTable: "ParallelCorpus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlignmentSet_ParallelCorpusHistory_ParallelCorpusHistoryId",
                        column: x => x.ParallelCorpusHistoryId,
                        principalTable: "ParallelCorpusHistory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlignmentSet_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VerseMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParallelCorpusId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParallelCorpusHistoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerseMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerseMapping_ParallelCorpus_ParallelCorpusId",
                        column: x => x.ParallelCorpusId,
                        principalTable: "ParallelCorpus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VerseMapping_ParallelCorpusHistory_ParallelCorpusHistoryId",
                        column: x => x.ParallelCorpusHistoryId,
                        principalTable: "ParallelCorpusHistory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Adornment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Lemma = table.Column<string>(type: "TEXT", nullable: true),
                    PartsOfSpeech = table.Column<string>(type: "TEXT", nullable: true),
                    Strong = table.Column<string>(type: "TEXT", nullable: true),
                    TokenMorphology = table.Column<string>(type: "TEXT", nullable: true),
                    TokenId = table.Column<Guid>(type: "TEXT", nullable: true),
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
                name: "Translation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetText = table.Column<string>(type: "TEXT", nullable: true),
                    TranslationState = table.Column<int>(type: "INTEGER", nullable: false),
                    TranslationSetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Translation_Token_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Token",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Translation_TranslationSet_TranslationSetId",
                        column: x => x.TranslationSetId,
                        principalTable: "TranslationSet",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TranslationModelEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TranslationSetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceText = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationModelEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TranslationModelEntry_TranslationSet_TranslationSetId",
                        column: x => x.TranslationSetId,
                        principalTable: "TranslationSet",
                        principalColumn: "Id");
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
                    Probability = table.Column<double>(type: "REAL", nullable: false),
                    AlignmentSetId = table.Column<Guid>(type: "TEXT", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "Verse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VerseNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    BookNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    ChapterNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    VerseText = table.Column<string>(type: "TEXT", nullable: true),
                    CorpusId = table.Column<Guid>(type: "TEXT", nullable: true),
                    VerseMappingId = table.Column<Guid>(type: "TEXT", nullable: true),
                    VerseBBBCCCVVV = table.Column<string>(type: "TEXT", nullable: true),
                    CorpusHistoryId = table.Column<Guid>(type: "TEXT", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_Verse_CorpusHistory_CorpusHistoryId",
                        column: x => x.CorpusHistoryId,
                        principalTable: "CorpusHistory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Verse_VerseMapping_VerseMappingId",
                        column: x => x.VerseMappingId,
                        principalTable: "VerseMapping",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TranslationModelTargetTextScore",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TranslationModelEntryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    Score = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationModelTargetTextScore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TranslationModelTargetTextScore_TranslationModelEntry_TranslationModelEntryId",
                        column: x => x.TranslationModelEntryId,
                        principalTable: "TranslationModelEntry",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TokenVerseAssociation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    VerseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenVerseAssociation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenVerseAssociation_Token_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Token",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TokenVerseAssociation_Verse_VerseId",
                        column: x => x.VerseId,
                        principalTable: "Verse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adornment_TokenId",
                table: "Adornment",
                column: "TokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentSet_EngineWordAlignmentId",
                table: "AlignmentSet",
                column: "EngineWordAlignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentSet_ParallelCorpusHistoryId",
                table: "AlignmentSet",
                column: "ParallelCorpusHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentSet_ParallelCorpusId",
                table: "AlignmentSet",
                column: "ParallelCorpusId");

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
                name: "IX_ParallelCorpus_SourceTokenizedCorpusId",
                table: "ParallelCorpus",
                column: "SourceTokenizedCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelCorpus_TargetTokenizedCorpusId",
                table: "ParallelCorpus",
                column: "TargetTokenizedCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelCorpusHistory_SourceTokenizedCorpusId",
                table: "ParallelCorpusHistory",
                column: "SourceTokenizedCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelCorpusHistory_TargetTokenizedCorpusId",
                table: "ParallelCorpusHistory",
                column: "TargetTokenizedCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_RawContent_NoteId",
                table: "RawContent",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Token_BookNumber",
                table: "Token",
                column: "BookNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Token_ChapterNumber",
                table: "Token",
                column: "ChapterNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Token_TokenCompositeId",
                table: "Token",
                column: "TokenCompositeId");

            migrationBuilder.CreateIndex(
                name: "IX_Token_TokenizationId",
                table: "Token",
                column: "TokenizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Token_VerseNumber",
                table: "Token",
                column: "VerseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TokenizedCorpus_CorpusHistoryId",
                table: "TokenizedCorpus",
                column: "CorpusHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenizedCorpus_CorpusId",
                table: "TokenizedCorpus",
                column: "CorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenVerseAssociation_TokenId",
                table: "TokenVerseAssociation",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenVerseAssociation_VerseId",
                table: "TokenVerseAssociation",
                column: "VerseId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_TokenId",
                table: "Translation",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_TranslationSetId",
                table: "Translation",
                column: "TranslationSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationModelEntry_TranslationSetId_SourceText",
                table: "TranslationModelEntry",
                columns: new[] { "TranslationSetId", "SourceText" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TranslationModelTargetTextScore_TranslationModelEntryId_Text",
                table: "TranslationModelTargetTextScore",
                columns: new[] { "TranslationModelEntryId", "Text" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TranslationSet_DerivedFromId",
                table: "TranslationSet",
                column: "DerivedFromId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationSet_EngineWordAlignmentId",
                table: "TranslationSet",
                column: "EngineWordAlignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationSet_ParallelCorpusId",
                table: "TranslationSet",
                column: "ParallelCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationSet_UserId",
                table: "TranslationSet",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Verse_CorpusHistoryId",
                table: "Verse",
                column: "CorpusHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Verse_CorpusId",
                table: "Verse",
                column: "CorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_Verse_VerseMappingId",
                table: "Verse",
                column: "VerseMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseMapping_ParallelCorpusHistoryId",
                table: "VerseMapping",
                column: "ParallelCorpusHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseMapping_ParallelCorpusId",
                table: "VerseMapping",
                column: "ParallelCorpusId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adornment");

            migrationBuilder.DropTable(
                name: "AlignmentTokenPair");

            migrationBuilder.DropTable(
                name: "NoteAssociation");

            migrationBuilder.DropTable(
                name: "NoteRecipient");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "RawContent");

            migrationBuilder.DropTable(
                name: "TokenVerseAssociation");

            migrationBuilder.DropTable(
                name: "Translation");

            migrationBuilder.DropTable(
                name: "TranslationModelTargetTextScore");

            migrationBuilder.DropTable(
                name: "AlignmentSet");

            migrationBuilder.DropTable(
                name: "Note");

            migrationBuilder.DropTable(
                name: "Verse");

            migrationBuilder.DropTable(
                name: "Token");

            migrationBuilder.DropTable(
                name: "TranslationModelEntry");

            migrationBuilder.DropTable(
                name: "VerseMapping");

            migrationBuilder.DropTable(
                name: "TranslationSet");

            migrationBuilder.DropTable(
                name: "ParallelCorpusHistory");

            migrationBuilder.DropTable(
                name: "EngineWordAlignment");

            migrationBuilder.DropTable(
                name: "ParallelCorpus");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "TokenizedCorpus");

            migrationBuilder.DropTable(
                name: "Corpus");

            migrationBuilder.DropTable(
                name: "CorpusHistory");
        }
    }
}
