using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class OriginalProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OriginalProjectId",
                table: "Projects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OriginalProjectId",
                table: "Projects",
                column: "OriginalProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Projects_OriginalProjectId",
                table: "Projects",
                column: "OriginalProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Projects_OriginalProjectId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_OriginalProjectId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OriginalProjectId",
                table: "Projects");
        }
    }
}
