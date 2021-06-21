using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class UpdateTicketAttachmentFileSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Invoices_InvoiceId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Projects_ProjectId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketAttachment_Ticket_TicketId",
                table: "TicketAttachment");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketMessage_Ticket_TicketId",
                table: "TicketMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketMessage",
                table: "TicketMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketAttachment",
                table: "TicketAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ticket",
                table: "Ticket");

            migrationBuilder.RenameTable(
                name: "TicketMessage",
                newName: "TicketMessages");

            migrationBuilder.RenameTable(
                name: "TicketAttachment",
                newName: "TicketAttachments");

            migrationBuilder.RenameTable(
                name: "Ticket",
                newName: "Tickets");

            migrationBuilder.RenameIndex(
                name: "IX_TicketMessage_TicketId",
                table: "TicketMessages",
                newName: "IX_TicketMessages_TicketId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketAttachment_TicketId",
                table: "TicketAttachments",
                newName: "IX_TicketAttachments_TicketId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_ProjectId",
                table: "Tickets",
                newName: "IX_Tickets_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_InvoiceId",
                table: "Tickets",
                newName: "IX_Tickets_InvoiceId");

            migrationBuilder.AddColumn<int>(
                name: "FileSize",
                table: "TicketAttachments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "Completed",
                table: "Tickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketMessages",
                table: "TicketMessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketAttachments",
                table: "TicketAttachments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tickets",
                table: "Tickets",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_CreatedOn",
                table: "TicketMessages",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_CreatedOn",
                table: "TicketAttachments",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Completed",
                table: "Tickets",
                column: "Completed");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CreatedOn",
                table: "Tickets",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_DueDate",
                table: "Tickets",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Name",
                table: "Tickets",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAttachments_Tickets_TicketId",
                table: "TicketAttachments",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMessages_Tickets_TicketId",
                table: "TicketMessages",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Invoices_InvoiceId",
                table: "Tickets",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Projects_ProjectId",
                table: "Tickets",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketAttachments_Tickets_TicketId",
                table: "TicketAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketMessages_Tickets_TicketId",
                table: "TicketMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Invoices_InvoiceId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Projects_ProjectId",
                table: "Tickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tickets",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_Completed",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CreatedOn",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_DueDate",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_Name",
                table: "Tickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketMessages",
                table: "TicketMessages");

            migrationBuilder.DropIndex(
                name: "IX_TicketMessages_CreatedOn",
                table: "TicketMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketAttachments",
                table: "TicketAttachments");

            migrationBuilder.DropIndex(
                name: "IX_TicketAttachments_CreatedOn",
                table: "TicketAttachments");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "TicketAttachments");

            migrationBuilder.RenameTable(
                name: "Tickets",
                newName: "Ticket");

            migrationBuilder.RenameTable(
                name: "TicketMessages",
                newName: "TicketMessage");

            migrationBuilder.RenameTable(
                name: "TicketAttachments",
                newName: "TicketAttachment");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_ProjectId",
                table: "Ticket",
                newName: "IX_Ticket_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_InvoiceId",
                table: "Ticket",
                newName: "IX_Ticket_InvoiceId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketMessages_TicketId",
                table: "TicketMessage",
                newName: "IX_TicketMessage_TicketId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketAttachments_TicketId",
                table: "TicketAttachment",
                newName: "IX_TicketAttachment_TicketId");

            migrationBuilder.AlterColumn<bool>(
                name: "Completed",
                table: "Ticket",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ticket",
                table: "Ticket",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketMessage",
                table: "TicketMessage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketAttachment",
                table: "TicketAttachment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Invoices_InvoiceId",
                table: "Ticket",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Projects_ProjectId",
                table: "Ticket",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAttachment_Ticket_TicketId",
                table: "TicketAttachment",
                column: "TicketId",
                principalTable: "Ticket",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMessage_Ticket_TicketId",
                table: "TicketMessage",
                column: "TicketId",
                principalTable: "Ticket",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
