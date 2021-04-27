using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.SqlServer
{
    public partial class RemoveQuoteId1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Quotes_QuoteId1",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_QuoteId1",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "QuoteId1",
                table: "Projects");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Quotes_QuoteId",
                table: "Projects",
                column: "QuoteId",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Quotes_QuoteId",
                table: "Projects");

            migrationBuilder.AddColumn<int>(
                name: "QuoteId1",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_QuoteId1",
                table: "Projects",
                column: "QuoteId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Quotes_QuoteId1",
                table: "Projects",
                column: "QuoteId1",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
