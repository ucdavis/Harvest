using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class ExpenseObjectCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpenseObjectCode",
                table: "Rates",
                type: "TEXT",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpenseObjectCode",
                table: "Expenses",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpenseObjectCode",
                table: "Rates");

            migrationBuilder.DropColumn(
                name: "ExpenseObjectCode",
                table: "Expenses");
        }
    }
}
