using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    public partial class denormfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlignmentTopTargetTrainingText_Alignment_AlignmentId",
                table: "AlignmentTopTargetTrainingText");

            migrationBuilder.DropIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentId",
                table: "AlignmentTopTargetTrainingText");

            migrationBuilder.DropColumn(
                name: "AlignmentId",
                table: "AlignmentTopTargetTrainingText");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AlignmentId",
                table: "AlignmentTopTargetTrainingText",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AlignmentTopTargetTrainingText_AlignmentId",
                table: "AlignmentTopTargetTrainingText",
                column: "AlignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlignmentTopTargetTrainingText_Alignment_AlignmentId",
                table: "AlignmentTopTargetTrainingText",
                column: "AlignmentId",
                principalTable: "Alignment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
