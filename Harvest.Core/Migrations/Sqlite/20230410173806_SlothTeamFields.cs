using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class SlothTeamFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SlothApiKey",
                table: "Teams",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlothSource",
                table: "Teams",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlothApiKey",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "SlothSource",
                table: "Teams");
        }
    }
}
