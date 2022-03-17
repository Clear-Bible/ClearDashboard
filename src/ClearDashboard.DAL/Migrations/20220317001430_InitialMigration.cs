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
                name: "AlignmentType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlignmentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CorpusType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "integer", nullable: false),
                    Description = table.Column<long>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorpusType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParallelCorpus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    SourceCorpusId = table.Column<long>(type: "integer", nullable: false),
                    TargetCorpusId = table.Column<long>(type: "integer", nullable: false),
                    AlignmentType = table.Column<long>(type: "integer", nullable: true),
                    CreationDate = table.Column<byte[]>(type: "datetime", nullable: true),
                    LastGenerated = table.Column<byte[]>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParallelCorpus", x => x.Id);
                    table.UniqueConstraint("AK_ParallelCorpus_SourceCorpusId", x => x.SourceCorpusId);
                    table.UniqueConstraint("AK_ParallelCorpus_TargetCorpusId", x => x.TargetCorpusId);
                });

            migrationBuilder.CreateTable(
                name: "ProjectInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "integer", nullable: false),
                    ProjectName = table.Column<string>(type: "text", nullable: true),
                    CreationDate = table.Column<byte[]>(type: "datetime", nullable: true),
                    IsRTL = table.Column<byte[]>(type: "bit", nullable: true),
                    LastContentWordLevel = table.Column<long>(type: "integer", nullable: true)
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
                name: "Corpus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "integer", nullable: false),
                    IsRTL = table.Column<byte[]>(type: "bit", nullable: true),
                    Name = table.Column<string>(type: "varchar(100)", nullable: true),
                    Language = table.Column<long>(type: "integer", nullable: true),
                    ParatextGUID = table.Column<string>(type: "varchar(250)", nullable: true),
                    CorpusTypeId = table.Column<long>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Corpus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Corpus_CorpusType_CorpusTypeId",
                        column: x => x.CorpusTypeId,
                        principalTable: "CorpusType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Corpus_ParallelCorpus_Id",
                        column: x => x.Id,
                        principalTable: "ParallelCorpus",
                        principalColumn: "SourceCorpusId");
                    table.ForeignKey(
                        name: "FK_Corpus_ParallelCorpus_Id1",
                        column: x => x.Id,
                        principalTable: "ParallelCorpus",
                        principalColumn: "TargetCorpusId");
                });

            migrationBuilder.CreateTable(
                name: "ParallelVerse",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    SourceVerseId = table.Column<long>(type: "bigint", nullable: false),
                    TargetVerseId = table.Column<long>(type: "bigint", nullable: false),
                    ParallelCorpusId = table.Column<long>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParallelVerse", x => x.Id);
                    table.UniqueConstraint("AK_ParallelVerse_SourceVerseId", x => x.SourceVerseId);
                    table.UniqueConstraint("AK_ParallelVerse_TargetVerseId", x => x.TargetVerseId);
                    table.ForeignKey(
                        name: "FK_ParallelVerse_ParallelCorpus_ParallelCorpusId",
                        column: x => x.ParallelCorpusId,
                        principalTable: "ParallelCorpus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Adornment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    TokenId = table.Column<long>(type: "bigint", nullable: true),
                    Lemma = table.Column<string>(type: "varchar(50)", nullable: true),
                    POS = table.Column<string>(type: "varchar(15)", nullable: false),
                    Strong = table.Column<string>(type: "varchar(15)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adornment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alignment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    SourceTokenId = table.Column<long>(type: "integer", nullable: false),
                    TargetTokenId = table.Column<long>(type: "integer", nullable: false),
                    Score = table.Column<byte[]>(type: "decimal(3)", nullable: true),
                    AlignmentVersionId = table.Column<long>(type: "bigint", nullable: true),
                    AlignmentTypeId = table.Column<long>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alignment", x => x.Id);
                    table.UniqueConstraint("AK_Alignment_SourceTokenId", x => x.SourceTokenId);
                    table.UniqueConstraint("AK_Alignment_TargetTokenId", x => x.TargetTokenId);
                    table.ForeignKey(
                        name: "FK_Alignment_AlignmentType_AlignmentTypeId",
                        column: x => x.AlignmentTypeId,
                        principalTable: "AlignmentType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Token",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    WordId = table.Column<long>(type: "integer", nullable: false),
                    PartId = table.Column<long>(type: "integer", nullable: false),
                    VerseId = table.Column<long>(type: "bigint", nullable: false),
                    Text = table.Column<string>(type: "varchar(250)", nullable: true),
                    FirstLetter = table.Column<string>(type: "varchar(2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Token", x => new { x.Id, x.WordId, x.PartId, x.VerseId });
                    table.UniqueConstraint("AK_Token_Id", x => x.Id);
                    table.UniqueConstraint("AK_Token_VerseId", x => x.VerseId);
                    table.ForeignKey(
                        name: "FK_Token_Alignment_Id",
                        column: x => x.Id,
                        principalTable: "Alignment",
                        principalColumn: "SourceTokenId");
                    table.ForeignKey(
                        name: "FK_Token_Alignment_Id1",
                        column: x => x.Id,
                        principalTable: "Alignment",
                        principalColumn: "TargetTokenId");
                });

            migrationBuilder.CreateTable(
                name: "InterlinearNotes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    TokenId = table.Column<long>(type: "bigint", nullable: true),
                    Note = table.Column<string>(type: "varchar(600)", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreationDate = table.Column<byte[]>(type: "datetime", nullable: true)
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
                name: "Verse",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    BookId = table.Column<string>(type: "varchar(2)", nullable: true),
                    VerseText = table.Column<string>(type: "text", nullable: true),
                    LastChanged = table.Column<byte[]>(type: "datetime", nullable: true),
                    CorpusId = table.Column<long>(type: "integer", nullable: true)
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
                        name: "FK_Verse_ParallelVerse_Id",
                        column: x => x.Id,
                        principalTable: "ParallelVerse",
                        principalColumn: "SourceVerseId");
                    table.ForeignKey(
                        name: "FK_Verse_ParallelVerse_Id1",
                        column: x => x.Id,
                        principalTable: "ParallelVerse",
                        principalColumn: "TargetVerseId");
                    table.ForeignKey(
                        name: "FK_Verse_Token_Id",
                        column: x => x.Id,
                        principalTable: "Token",
                        principalColumn: "VerseId");
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ParatextUsername = table.Column<string>(type: "varchar(100)", nullable: true),
                    LastAlignmentLevelId = table.Column<long>(type: "bigint", nullable: true)
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
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<byte[]>(type: "datetime", nullable: true),
                    UserId = table.Column<long>(type: "integer", nullable: true),
                    IsDirty = table.Column<byte[]>(type: "bit", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "pk_Adornments",
                table: "Adornment",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "unq_Adornments_TokenId",
                table: "Adornment",
                column: "TokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alignment_AlignmentTypeId",
                table: "Alignment",
                column: "AlignmentTypeId");

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
                name: "Unq_Alignment_SourceTokenId",
                table: "Alignment",
                column: "SourceTokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Unq_Alignment_TargetTokenId",
                table: "Alignment",
                column: "TargetTokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentVersion_UserId",
                table: "AlignmentVersion",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Corpus_CorpusTypeId",
                table: "Corpus",
                column: "CorpusTypeId");

            migrationBuilder.CreateIndex(
                name: "Pk_CorpusType_CorpusTypeId",
                table: "CorpusType",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterlinearNotes_TokenId",
                table: "InterlinearNotes",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "unq_InterlinearNotes_UserId",
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
                name: "Pk_ParallelCorpus_ParallelCorpusId",
                table: "ParallelCorpus",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Unq_ParallelCorpus_SourceCorpusId",
                table: "ParallelCorpus",
                column: "SourceCorpusId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Unq_ParallelCorpus_TargetCorpusId",
                table: "ParallelCorpus",
                column: "TargetCorpusId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParallelVerse_ParallelCorpusId",
                table: "ParallelVerse",
                column: "ParallelCorpusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParallelVerses_SourceVerseId",
                table: "ParallelVerse",
                column: "SourceVerseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParallelVerses_TargetVerseId",
                table: "ParallelVerse",
                column: "TargetVerseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Unq_ParallelVerses_SourceVerseId",
                table: "ParallelVerse",
                column: "SourceVerseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Unq_ParallelVerses_TargetVerseId",
                table: "ParallelVerse",
                column: "TargetVerseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Token_VerseId",
                table: "Token",
                column: "VerseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Unq_Token_TokenId",
                table: "Token",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Unq_Token_VerseId",
                table: "Token",
                column: "VerseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Verse_CorpusId",
                table: "Verse",
                column: "CorpusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Adornment_Token_TokenId",
                table: "Adornment",
                column: "TokenId",
                principalTable: "Token",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Alignment_AlignmentVersion_AlignmentVersionId",
                table: "Alignment",
                column: "AlignmentVersionId",
                principalTable: "AlignmentVersion",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterlinearNotes_Token_TokenId",
                table: "InterlinearNotes");

            migrationBuilder.DropTable(
                name: "Adornment");

            migrationBuilder.DropTable(
                name: "ProjectInfo");

            migrationBuilder.DropTable(
                name: "QuestionGroup");

            migrationBuilder.DropTable(
                name: "Verse");

            migrationBuilder.DropTable(
                name: "Corpus");

            migrationBuilder.DropTable(
                name: "ParallelVerse");

            migrationBuilder.DropTable(
                name: "CorpusType");

            migrationBuilder.DropTable(
                name: "ParallelCorpus");

            migrationBuilder.DropTable(
                name: "Token");

            migrationBuilder.DropTable(
                name: "Alignment");

            migrationBuilder.DropTable(
                name: "AlignmentType");

            migrationBuilder.DropTable(
                name: "AlignmentVersion");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "InterlinearNotes");
        }
    }
}
