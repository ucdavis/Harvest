using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class TicketUpdates20210621 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "TicketMessages",
                newName: "CreatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "TicketAttachments",
                newName: "CreatedById");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOn",
                table: "Tickets",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedById",
                table: "Tickets",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "Tickets",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CreatedById",
                table: "Tickets",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UpdatedById",
                table: "Tickets",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_CreatedById",
                table: "TicketMessages",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_CreatedById",
                table: "TicketAttachments",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAttachments_Users_CreatedById",
                table: "TicketAttachments",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMessages_Users_CreatedById",
                table: "TicketMessages",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_CreatedById",
                table: "Tickets",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_UpdatedById",
                table: "Tickets",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketAttachments_Users_CreatedById",
                table: "TicketAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketMessages_Users_CreatedById",
                table: "TicketMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_CreatedById",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_UpdatedById",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CreatedById",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_UpdatedById",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_TicketMessages_CreatedById",
                table: "TicketMessages");

            migrationBuilder.DropIndex(
                name: "IX_TicketAttachments_CreatedById",
                table: "TicketAttachments");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "TicketMessages",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "TicketAttachments",
                newName: "CreatedBy");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOn",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UpdatedById",
                table: "Tickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
