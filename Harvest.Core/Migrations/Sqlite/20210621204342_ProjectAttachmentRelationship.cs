using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class ProjectAttachmentRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectAttachments_Projects_ProjectId",
                table: "ProjectAttachments");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectAttachments_Projects_ProjectId",
                table: "ProjectAttachments",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectAttachments_Projects_ProjectId",
                table: "ProjectAttachments");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectAttachments_Projects_ProjectId",
                table: "ProjectAttachments",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
