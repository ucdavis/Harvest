using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.SqlServer
{
    public partial class RemoveTeams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Teams_TeamId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Users_TeamId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Teams_TeamId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TeamId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_TeamId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Permissions");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Users_UserId",
                table: "Permissions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Users_UserId",
                table: "Permissions");

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Permissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TeamId",
                table: "Projects",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_TeamId",
                table: "Permissions",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Name",
                table: "Teams",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Slug",
                table: "Teams",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Teams_TeamId",
                table: "Permissions",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Users_TeamId",
                table: "Permissions",
                column: "TeamId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Teams_TeamId",
                table: "Projects",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
