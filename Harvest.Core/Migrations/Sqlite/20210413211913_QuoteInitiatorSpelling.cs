using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class QuoteInitiatorSpelling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Users_InitiatedById",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_InitatedById",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "InitatedById",
                table: "Quotes");

            migrationBuilder.AlterColumn<int>(
                name: "InitiatedById",
                table: "Quotes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Users_InitiatedById",
                table: "Quotes",
                column: "InitiatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Users_InitiatedById",
                table: "Quotes");

            migrationBuilder.AlterColumn<int>(
                name: "InitiatedById",
                table: "Quotes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "InitatedById",
                table: "Quotes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_InitatedById",
                table: "Quotes",
                column: "InitatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Users_InitiatedById",
                table: "Quotes",
                column: "InitiatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
