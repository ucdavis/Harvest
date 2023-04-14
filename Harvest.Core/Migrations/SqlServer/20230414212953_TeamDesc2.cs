using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harvest.Core.Migrations.SqlServer
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
                type: "nvarchar(256)",
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
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
