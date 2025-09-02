using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harvest.Core.Migrations.SqlServer
{
    public partial class PermissionToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Expires",
                table: "Permissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Token",
                table: "Permissions",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Expires",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "Permissions");
        }
    }
}
