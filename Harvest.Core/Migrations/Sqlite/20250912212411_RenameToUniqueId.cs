using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class RenameToUniqueId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkerMobileId",
                table: "Expenses",
                newName: "UniqueId");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_WorkerMobileId",
                table: "Expenses",
                newName: "IX_Expenses_UniqueId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UniqueId",
                table: "Expenses",
                newName: "WorkerMobileId");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_UniqueId",
                table: "Expenses",
                newName: "IX_Expenses_WorkerMobileId");
        }
    }
}
