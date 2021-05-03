using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class ProjectHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actor",
                table: "ProjectHistory");

            migrationBuilder.DropColumn(
                name: "ActorName",
                table: "ProjectHistory");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ProjectHistory");

            migrationBuilder.AddColumn<int>(
                name: "ActorId",
                table: "ProjectHistory",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectHistory_ActorId",
                table: "ProjectHistory",
                column: "ActorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectHistory_Users_ActorId",
                table: "ProjectHistory",
                column: "ActorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectHistory_Users_ActorId",
                table: "ProjectHistory");

            migrationBuilder.DropIndex(
                name: "IX_ProjectHistory_ActorId",
                table: "ProjectHistory");

            migrationBuilder.DropColumn(
                name: "ActorId",
                table: "ProjectHistory");

            migrationBuilder.AddColumn<int>(
                name: "Actor",
                table: "ProjectHistory",
                type: "INTEGER",
                maxLength: 20,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ActorName",
                table: "ProjectHistory",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ProjectHistory",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }
    }
}
