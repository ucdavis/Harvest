using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class ProjectRequirementsRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Requirements",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: "N/A",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Requirements",
                table: "Projects",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "N/A");
        }
    }
}
