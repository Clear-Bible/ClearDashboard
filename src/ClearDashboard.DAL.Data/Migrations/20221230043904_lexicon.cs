using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class lexicon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lexicon_Lexeme",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    Lemma = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lexicon_Lexeme", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lexicon_Lexeme_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lexicon_SemanticDomain",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lexicon_SemanticDomain", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lexicon_SemanticDomain_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lexicon_Form",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    LexemeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lexicon_Form", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lexicon_Form_Lexicon_Lexeme_LexemeId",
                        column: x => x.LexemeId,
                        principalTable: "Lexicon_Lexeme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lexicon_Meaning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    LexemeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lexicon_Meaning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lexicon_Meaning_Lexicon_Lexeme_LexemeId",
                        column: x => x.LexemeId,
                        principalTable: "Lexicon_Lexeme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lexicon_Meaning_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lexicon_SemanticDomainMeaningAssociation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SemanticDomainId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MeaningId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lexicon_SemanticDomainMeaningAssociation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lexicon_SemanticDomainMeaningAssociation_Lexicon_Meaning_MeaningId",
                        column: x => x.MeaningId,
                        principalTable: "Lexicon_Meaning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lexicon_SemanticDomainMeaningAssociation_Lexicon_SemanticDomain_SemanticDomainId",
                        column: x => x.SemanticDomainId,
                        principalTable: "Lexicon_SemanticDomain",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lexicon_Translation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    MeaningId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lexicon_Translation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lexicon_Translation_Lexicon_Meaning_MeaningId",
                        column: x => x.MeaningId,
                        principalTable: "Lexicon_Meaning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lexicon_Translation_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Form_LexemeId",
                table: "Lexicon_Form",
                column: "LexemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Lexeme_Lemma_Language",
                table: "Lexicon_Lexeme",
                columns: new[] { "Lemma", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Lexeme_UserId",
                table: "Lexicon_Lexeme",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Meaning_LexemeId",
                table: "Lexicon_Meaning",
                column: "LexemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Meaning_UserId",
                table: "Lexicon_Meaning",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_SemanticDomain_UserId",
                table: "Lexicon_SemanticDomain",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_SemanticDomainMeaningAssociation_MeaningId",
                table: "Lexicon_SemanticDomainMeaningAssociation",
                column: "MeaningId");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_SemanticDomainMeaningAssociation_SemanticDomainId",
                table: "Lexicon_SemanticDomainMeaningAssociation",
                column: "SemanticDomainId");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Translation_MeaningId",
                table: "Lexicon_Translation",
                column: "MeaningId");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Translation_UserId",
                table: "Lexicon_Translation",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lexicon_Form");

            migrationBuilder.DropTable(
                name: "Lexicon_SemanticDomainMeaningAssociation");

            migrationBuilder.DropTable(
                name: "Lexicon_Translation");

            migrationBuilder.DropTable(
                name: "Lexicon_SemanticDomain");

            migrationBuilder.DropTable(
                name: "Lexicon_Meaning");

            migrationBuilder.DropTable(
                name: "Lexicon_Lexeme");
        }
    }
}
