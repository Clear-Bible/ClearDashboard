using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class collab : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent");

            migrationBuilder.AddColumn<long>(
                name: "LastTokenized",
                table: "TokenizedCorpus",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Created",
                table: "TokenComponent",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "TokenComponent",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "LastMergedCommitSha",
                table: "Project",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenComponent_UserId",
                table: "TokenComponent",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenComponent_User_UserId",
                table: "TokenComponent",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent",
                column: "VerseRowId",
                principalTable: "VerseRow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenComponent_User_UserId",
                table: "TokenComponent");

            migrationBuilder.DropForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent");

            migrationBuilder.DropIndex(
                name: "IX_TokenComponent_UserId",
                table: "TokenComponent");

            migrationBuilder.DropColumn(
                name: "LastTokenized",
                table: "TokenizedCorpus");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "TokenComponent");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TokenComponent");

            migrationBuilder.DropColumn(
                name: "LastMergedCommitSha",
                table: "Project");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenComponent_VerseRow_VerseRowId",
                table: "TokenComponent",
                column: "VerseRowId",
                principalTable: "VerseRow",
                principalColumn: "Id");
        }
    }
}
