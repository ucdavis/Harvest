using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.SqlServer
{
    public partial class AddExpenseActivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Activity",
                table: "Expenses",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activity",
                table: "Expenses");
        }
    }
}
