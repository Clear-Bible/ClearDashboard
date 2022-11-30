using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    public partial class deletes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TranslationModelEntry_TranslationSet_TranslationSetId",
                table: "TranslationModelEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_TranslationModelTargetTextScore_TranslationModelEntry_TranslationModelEntryId",
                table: "TranslationModelTargetTextScore");

            migrationBuilder.DropForeignKey(
                name: "FK_Verse_VerseMapping_VerseMappingId",
                table: "Verse");

            migrationBuilder.DropForeignKey(
                name: "FK_VerseMapping_ParallelCorpus_ParallelCorpusId",
                table: "VerseMapping");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParallelCorpusId",
                table: "VerseMapping",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "VerseMappingId",
                table: "Verse",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TranslationModelEntryId",
                table: "TranslationModelTargetTextScore",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TranslationSetId",
                table: "TranslationModelEntry",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TranslationModelEntry_TranslationSet_TranslationSetId",
                table: "TranslationModelEntry",
                column: "TranslationSetId",
                principalTable: "TranslationSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TranslationModelTargetTextScore_TranslationModelEntry_TranslationModelEntryId",
                table: "TranslationModelTargetTextScore",
                column: "TranslationModelEntryId",
                principalTable: "TranslationModelEntry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Verse_VerseMapping_VerseMappingId",
                table: "Verse",
                column: "VerseMappingId",
                principalTable: "VerseMapping",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VerseMapping_ParallelCorpus_ParallelCorpusId",
                table: "VerseMapping",
                column: "ParallelCorpusId",
                principalTable: "ParallelCorpus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TranslationModelEntry_TranslationSet_TranslationSetId",
                table: "TranslationModelEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_TranslationModelTargetTextScore_TranslationModelEntry_TranslationModelEntryId",
                table: "TranslationModelTargetTextScore");

            migrationBuilder.DropForeignKey(
                name: "FK_Verse_VerseMapping_VerseMappingId",
                table: "Verse");

            migrationBuilder.DropForeignKey(
                name: "FK_VerseMapping_ParallelCorpus_ParallelCorpusId",
                table: "VerseMapping");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParallelCorpusId",
                table: "VerseMapping",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "VerseMappingId",
                table: "Verse",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "TranslationModelEntryId",
                table: "TranslationModelTargetTextScore",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "TranslationSetId",
                table: "TranslationModelEntry",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_TranslationModelEntry_TranslationSet_TranslationSetId",
                table: "TranslationModelEntry",
                column: "TranslationSetId",
                principalTable: "TranslationSet",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TranslationModelTargetTextScore_TranslationModelEntry_TranslationModelEntryId",
                table: "TranslationModelTargetTextScore",
                column: "TranslationModelEntryId",
                principalTable: "TranslationModelEntry",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Verse_VerseMapping_VerseMappingId",
                table: "Verse",
                column: "VerseMappingId",
                principalTable: "VerseMapping",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VerseMapping_ParallelCorpus_ParallelCorpusId",
                table: "VerseMapping",
                column: "ParallelCorpusId",
                principalTable: "ParallelCorpus",
                principalColumn: "Id");
        }
    }
}
