using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class labelgroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DefaultLabelGroupId",
                table: "User",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateText",
                table: "Label",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LabelGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LabelGroupAssociation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LabelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LabelGroupId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelGroupAssociation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabelGroupAssociation_LabelGroup_LabelGroupId",
                        column: x => x.LabelGroupId,
                        principalTable: "LabelGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabelGroupAssociation_Label_LabelId",
                        column: x => x.LabelId,
                        principalTable: "Label",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabelGroup_Name",
                table: "LabelGroup",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabelGroupAssociation_LabelGroupId",
                table: "LabelGroupAssociation",
                column: "LabelGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_LabelGroupAssociation_LabelId",
                table: "LabelGroupAssociation",
                column: "LabelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabelGroupAssociation");

            migrationBuilder.DropTable(
                name: "LabelGroup");

            migrationBuilder.DropColumn(
                name: "DefaultLabelGroupId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "TemplateText",
                table: "Label");
        }
    }
}
