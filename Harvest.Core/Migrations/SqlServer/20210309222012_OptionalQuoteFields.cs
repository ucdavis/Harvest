using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.SqlServer
{
    public partial class OptionalQuoteFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Users_ApprovedById",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_CurrentDocumentId",
                table: "Quotes");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentDocumentId",
                table: "Quotes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedOn",
                table: "Quotes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedById",
                table: "Quotes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "QuoteId",
                table: "Projects",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Projects",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "QuoteId1",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CurrentDocumentId",
                table: "Quotes",
                column: "CurrentDocumentId",
                unique: true,
                filter: "[CurrentDocumentId] IS NOT NULL");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Users_ApprovedById",
                table: "Quotes",
                column: "ApprovedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Quotes_QuoteId1",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Users_ApprovedById",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_CurrentDocumentId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Projects_QuoteId1",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "QuoteId1",
                table: "Projects");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentDocumentId",
                table: "Quotes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedOn",
                table: "Quotes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedById",
                table: "Quotes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "QuoteId",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CurrentDocumentId",
                table: "Quotes",
                column: "CurrentDocumentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Users_ApprovedById",
                table: "Quotes",
                column: "ApprovedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
