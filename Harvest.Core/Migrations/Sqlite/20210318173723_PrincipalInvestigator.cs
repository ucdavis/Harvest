using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class PrincipalInvestigator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_CreatedById",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "PrincipalInvestigator",
                table: "Projects",
                newName: "PrincipalInvestigatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_PrincipalInvestigatorId",
                table: "Projects",
                column: "PrincipalInvestigatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_CreatedById",
                table: "Projects",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_PrincipalInvestigatorId",
                table: "Projects",
                column: "PrincipalInvestigatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_CreatedById",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_PrincipalInvestigatorId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_PrincipalInvestigatorId",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "PrincipalInvestigatorId",
                table: "Projects",
                newName: "PrincipalInvestigator");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_CreatedById",
                table: "Projects",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
