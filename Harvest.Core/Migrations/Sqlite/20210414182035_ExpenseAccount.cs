using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class ExpenseAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Account",
                table: "Expenses",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Account",
                table: "Expenses");
        }
    }
}
