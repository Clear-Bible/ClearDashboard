using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class parallelcorpusverse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParallelCorpusId",
                table: "Verse",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Verse_ParallelCorpusId",
                table: "Verse",
                column: "ParallelCorpusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Verse_ParallelCorpus_ParallelCorpusId",
                table: "Verse",
                column: "ParallelCorpusId",
                principalTable: "ParallelCorpus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Verse_ParallelCorpus_ParallelCorpusId",
                table: "Verse");

            migrationBuilder.DropIndex(
                name: "IX_Verse_ParallelCorpusId",
                table: "Verse");

            migrationBuilder.DropColumn(
                name: "ParallelCorpusId",
                table: "Verse");
        }
    }
}
