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
                name: "Label",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Label", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NoteAssociation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssociationId = table.Column<string>(type: "TEXT", nullable: true),
                    AssociationType = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteAssociation", x => x.Id);
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
                name: "Corpus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsRtl = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_Corpus_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey(
                        name: "FK_CorpusHistory_User_UserId",
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
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    AbbreviatedText = table.Column<string>(type: "TEXT", nullable: true),
                    ThreadId = table.Column<Guid>(type: "TEXT", nullable: true),
                    NoteStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Note", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Note_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectName = table.Column<string>(type: "TEXT", nullable: true),
                    IsRtl = table.Column<bool>(type: "INTEGER", nullable: false),
                    DesignSurfaceLayout = table.Column<string>(type: "TEXT", nullable: true),
                    WindowTabLayout = table.Column<string>(type: "TEXT", nullable: true),
                    AppVersion = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TokenizedCorpus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CorpusHistoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    TokenizationFunction = table.Column<string>(type: "TEXT", nullable: true),
                    ScrVersType = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomVersData = table.Column<string>(type: "TEXT", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_TokenizedCorpus_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabelNoteAssociation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LabelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NoteId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelNoteAssociation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabelNoteAssociation_Label_LabelId",
                        column: x => x.LabelId,
                        principalTable: "Label",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabelNoteAssociation_Note_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Note",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NoteDomainEntityAssociation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NoteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DomainEntityIdGuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    DomainEntityIdName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteDomainEntityAssociation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoteDomainEntityAssociation_Note_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Note",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "ParallelCorpus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    SourceTokenizedCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetTokenizedCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_ParallelCorpus_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParallelCorpusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    SourceTokenizedCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetTokenizedCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_ParallelCorpusHistory_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    TokenizedCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerseRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerseRow_TokenizedCorpus_TokenizedCorpusId",
                        column: x => x.TokenizedCorpusId,
                        principalTable: "TokenizedCorpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VerseRow_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlignmentSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParallelCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    SmtModel = table.Column<string>(type: "TEXT", nullable: true),
                    IsSyntaxTreeAlignerRefined = table.Column<bool>(type: "INTEGER", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    ParallelCorpusHistoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlignmentSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlignmentSet_ParallelCorpus_ParallelCorpusId",
                        column: x => x.ParallelCorpusId,
                        principalTable: "ParallelCorpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlignmentSet_ParallelCorpusHistory_ParallelCorpusHistoryId",
                        column: x => x.ParallelCorpusHistoryId,
                        principalTable: "ParallelCorpusHistory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlignmentSet_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey(
                        name: "FK_VerseMapping_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TokenComponent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EngineTokenId = table.Column<string>(type: "TEXT", nullable: true),
                    TrainingText = table.Column<string>(type: "TEXT", nullable: true),
                    ExtendedProperties = table.Column<string>(type: "TEXT", nullable: true),
                    VerseRowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenizedCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", nullable: false),
                    BookNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    ChapterNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    VerseNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    WordNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    SubwordNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    SurfaceText = table.Column<string>(type: "TEXT", nullable: true),
                    TokenCompositeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParallelCorpusId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenComponent_ParallelCorpus_ParallelCorpusId",
                        column: x => x.ParallelCorpusId,
                        principalTable: "ParallelCorpus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TokenComponent_TokenComponent_TokenCompositeId",
                        column: x => x.TokenCompositeId,
                        principalTable: "TokenComponent",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TokenComponent_TokenizedCorpus_TokenizedCorpusId",
                        column: x => x.TokenizedCorpusId,
                        principalTable: "TokenizedCorpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TokenComponent_VerseRow_VerseRowId",
                        column: x => x.VerseRowId,
                        principalTable: "VerseRow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "TranslationSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DerivedFromId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParallelCorpusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlignmentSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    ParallelCorpusHistoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TranslationSet_AlignmentSet_AlignmentSetId",
                        column: x => x.AlignmentSetId,
                        principalTable: "AlignmentSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TranslationSet_ParallelCorpus_ParallelCorpusId",
                        column: x => x.ParallelCorpusId,
                        principalTable: "ParallelCorpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TranslationSet_ParallelCorpusHistory_ParallelCorpusHistoryId",
                        column: x => x.ParallelCorpusHistoryId,
                        principalTable: "ParallelCorpusHistory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TranslationSet_TranslationSet_DerivedFromId",
                        column: x => x.DerivedFromId,
                        principalTable: "TranslationSet",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TranslationSet_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
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
                        name: "FK_Verse_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Verse_VerseMapping_VerseMappingId",
                        column: x => x.VerseMappingId,
                        principalTable: "VerseMapping",
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
                        name: "FK_Adornment_TokenComponent_TokenId",
                        column: x => x.TokenId,
                        principalTable: "TokenComponent",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Adornment_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alignment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceTokenComponentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetTokenComponentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlignmentVerification = table.Column<int>(type: "INTEGER", nullable: false),
                    AlignmentOriginatedFrom = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<double>(type: "REAL", nullable: false),
                    AlignmentSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alignment_AlignmentSet_AlignmentSetId",
                        column: x => x.AlignmentSetId,
                        principalTable: "AlignmentSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alignment_TokenComponent_SourceTokenComponentId",
                        column: x => x.SourceTokenComponentId,
                        principalTable: "TokenComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alignment_TokenComponent_TargetTokenComponentId",
                        column: x => x.TargetTokenComponentId,
                        principalTable: "TokenComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alignment_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Translation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceTokenComponentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetText = table.Column<string>(type: "TEXT", nullable: true),
                    TranslationState = table.Column<int>(type: "INTEGER", nullable: false),
                    TranslationSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Translation_TokenComponent_SourceTokenComponentId",
                        column: x => x.SourceTokenComponentId,
                        principalTable: "TokenComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Translation_TranslationSet_TranslationSetId",
                        column: x => x.TranslationSetId,
                        principalTable: "TranslationSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Translation_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "TokenVerseAssociation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenComponentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    VerseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenVerseAssociation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenVerseAssociation_TokenComponent_TokenComponentId",
                        column: x => x.TokenComponentId,
                        principalTable: "TokenComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TokenVerseAssociation_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TokenVerseAssociation_Verse_VerseId",
                        column: x => x.VerseId,
                        principalTable: "Verse",
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

            migrationBuilder.CreateIndex(
                name: "IX_Adornment_TokenId",
                table: "Adornment",
                column: "TokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Adornment_UserId",
                table: "Adornment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Alignment_AlignmentSetId",
                table: "Alignment",
                column: "AlignmentSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Alignment_SourceTokenComponentId",
                table: "Alignment",
                column: "SourceTokenComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_Alignment_TargetTokenComponentId",
                table: "Alignment",
                column: "TargetTokenComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_Alignment_UserId",
                table: "Alignment",
                column: "UserId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Corpus_UserId",
                table: "Corpus",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CorpusHistory_UserId",
                table: "CorpusHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LabelNoteAssociation_LabelId",
                table: "LabelNoteAssociation",
                column: "LabelId");

            migrationBuilder.CreateIndex(
                name: "IX_LabelNoteAssociation_NoteId",
                table: "LabelNoteAssociation",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Note_UserId",
                table: "Note",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteDomainEntityAssociation_NoteId",
                table: "NoteDomainEntityAssociation",
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
                name: "IX_ParallelCorpus_UserId",
                table: "ParallelCorpus",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelCorpusHistory_SourceTokenizedCorpusId",
                table: "ParallelCorpusHistory",
                column: "SourceTokenizedCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelCorpusHistory_TargetTokenizedCorpusId",
                table: "ParallelCorpusHistory",
                column: "TargetTokenizedCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelCorpusHistory_UserId",
                table: "ParallelCorpusHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_UserId",
                table: "Project",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RawContent_NoteId",
                table: "RawContent",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_BookNumber",
                table: "TokenComponent",
                column: "BookNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_ChapterNumber",
                table: "TokenComponent",
                column: "ChapterNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_EngineTokenId",
                table: "TokenComponent",
                column: "EngineTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_ParallelCorpusId",
                table: "TokenComponent",
                column: "ParallelCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_TokenCompositeId",
                table: "TokenComponent",
                column: "TokenCompositeId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_TokenizedCorpusId",
                table: "TokenComponent",
                column: "TokenizedCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_TrainingText",
                table: "TokenComponent",
                column: "TrainingText");

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_VerseNumber",
                table: "TokenComponent",
                column: "VerseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_VerseRowId",
                table: "TokenComponent",
                column: "VerseRowId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenizedCorpus_CorpusHistoryId",
                table: "TokenizedCorpus",
                column: "CorpusHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenizedCorpus_CorpusId",
                table: "TokenizedCorpus",
                column: "CorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenizedCorpus_UserId",
                table: "TokenizedCorpus",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenVerseAssociation_TokenComponentId",
                table: "TokenVerseAssociation",
                column: "TokenComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenVerseAssociation_UserId",
                table: "TokenVerseAssociation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenVerseAssociation_VerseId",
                table: "TokenVerseAssociation",
                column: "VerseId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_SourceTokenComponentId",
                table: "Translation",
                column: "SourceTokenComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_TranslationSetId",
                table: "Translation",
                column: "TranslationSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_UserId",
                table: "Translation",
                column: "UserId");

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
                name: "IX_TranslationSet_AlignmentSetId",
                table: "TranslationSet",
                column: "AlignmentSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationSet_DerivedFromId",
                table: "TranslationSet",
                column: "DerivedFromId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationSet_ParallelCorpusHistoryId",
                table: "TranslationSet",
                column: "ParallelCorpusHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationSet_ParallelCorpusId",
                table: "TranslationSet",
                column: "ParallelCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationSet_UserId",
                table: "TranslationSet",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Verse_BookNumber",
                table: "Verse",
                column: "BookNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Verse_ChapterNumber",
                table: "Verse",
                column: "ChapterNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Verse_CorpusHistoryId",
                table: "Verse",
                column: "CorpusHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Verse_CorpusId",
                table: "Verse",
                column: "CorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_Verse_UserId",
                table: "Verse",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Verse_VerseMappingId",
                table: "Verse",
                column: "VerseMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_Verse_VerseNumber",
                table: "Verse",
                column: "VerseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_VerseMapping_ParallelCorpusHistoryId",
                table: "VerseMapping",
                column: "ParallelCorpusHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseMapping_ParallelCorpusId",
                table: "VerseMapping",
                column: "ParallelCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseMapping_UserId",
                table: "VerseMapping",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseRow_TokenizedCorpusId",
                table: "VerseRow",
                column: "TokenizedCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_VerseRow_UserId",
                table: "VerseRow",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adornment");

            migrationBuilder.DropTable(
                name: "AlignmentSetDenormalizationTask");

            migrationBuilder.DropTable(
                name: "AlignmentTopTargetTrainingText");

            migrationBuilder.DropTable(
                name: "LabelNoteAssociation");

            migrationBuilder.DropTable(
                name: "NoteAssociation");

            migrationBuilder.DropTable(
                name: "NoteDomainEntityAssociation");

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
                name: "Alignment");

            migrationBuilder.DropTable(
                name: "Label");

            migrationBuilder.DropTable(
                name: "Note");

            migrationBuilder.DropTable(
                name: "Verse");

            migrationBuilder.DropTable(
                name: "TranslationModelEntry");

            migrationBuilder.DropTable(
                name: "TokenComponent");

            migrationBuilder.DropTable(
                name: "VerseMapping");

            migrationBuilder.DropTable(
                name: "TranslationSet");

            migrationBuilder.DropTable(
                name: "VerseRow");

            migrationBuilder.DropTable(
                name: "AlignmentSet");

            migrationBuilder.DropTable(
                name: "ParallelCorpus");

            migrationBuilder.DropTable(
                name: "ParallelCorpusHistory");

            migrationBuilder.DropTable(
                name: "TokenizedCorpus");

            migrationBuilder.DropTable(
                name: "Corpus");

            migrationBuilder.DropTable(
                name: "CorpusHistory");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
