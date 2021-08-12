using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.SqlServer
{
    public partial class ExpenseMarkup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Projects_ProjectId",
                table: "Expenses");

            migrationBuilder.AddColumn<bool>(
                name: "Markup",
                table: "Expenses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Projects_ProjectId",
                table: "Expenses",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Projects_ProjectId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "Markup",
                table: "Expenses");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Projects_ProjectId",
                table: "Expenses",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
