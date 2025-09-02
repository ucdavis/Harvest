using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harvest.Core.Migrations.SqlServer
{
    public partial class SaltAndHashSized : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Salt",
                table: "Permissions",
                type: "varbinary(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Lookup",
                table: "Permissions",
                type: "varbinary(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(900)",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Hash",
                table: "Permissions",
                type: "varbinary(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Salt",
                table: "Permissions",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Lookup",
                table: "Permissions",
                type: "varbinary(900)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Hash",
                table: "Permissions",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(32)",
                oldMaxLength: 32,
                oldNullable: true);
        }
    }
}
