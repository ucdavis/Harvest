using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.SqlServer
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
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Iam",
                table: "Users",
                column: "Iam",
                unique: true,
                filter: "[Iam] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Iam",
                table: "Users");

            migrationBuilder.AlterColumn<bool>(
                name: "IsProjectAccount",
                table: "Transfers",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Iam",
                table: "Users",
                column: "Iam");
        }
    }
}
