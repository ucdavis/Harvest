using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class TeamDesc2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Teams");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TeamDetails",
                type: "TEXT",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "TeamDetails");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Teams",
                type: "TEXT",
                maxLength: 256,
                nullable: true);
        }
    }
}
