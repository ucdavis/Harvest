using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class SaltAndHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Hash",
                table: "Permissions",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Lookup",
                table: "Permissions",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Salt",
                table: "Permissions",
                type: "BLOB",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Lookup",
                table: "Permissions",
                column: "Lookup");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_Lookup",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Lookup",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Salt",
                table: "Permissions");
        }
    }
}
