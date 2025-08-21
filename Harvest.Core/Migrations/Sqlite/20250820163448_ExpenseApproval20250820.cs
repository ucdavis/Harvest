using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class ExpenseApproval20250820 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "Expenses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOn",
                table: "Expenses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_Approved",
                table: "Expenses",
                column: "Approved");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ApprovedById",
                table: "Expenses",
                column: "ApprovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Users_ApprovedById",
                table: "Expenses",
                column: "ApprovedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Users_ApprovedById",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_Approved",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ApprovedById",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ApprovedOn",
                table: "Expenses");
        }
    }
}
