using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class MakeIamUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Iam",
                table: "Users");

            migrationBuilder.AlterColumn<bool>(
                name: "IsProjectAccount",
                table: "Transfers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Iam",
                table: "Users",
                column: "Iam",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Iam",
                table: "Users");

            migrationBuilder.AlterColumn<bool>(
                name: "IsProjectAccount",
                table: "Transfers",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Iam",
                table: "Users",
                column: "Iam");
        }
    }
}
