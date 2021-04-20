using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.SqlServer
{
    public partial class updateAndRemoveTablesForSlothService : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Accounts_FromAccountId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Accounts_ToAccountId",
                table: "Transfers");

            migrationBuilder.DropTable(
                name: "TransferHistory");

            migrationBuilder.DropTable(
                name: "TransferRequests");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_FromAccountId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "FromAccountId",
                table: "Transfers");

            migrationBuilder.RenameColumn(
                name: "ToAccountId",
                table: "Transfers",
                newName: "InvoiceId");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Transfers",
                newName: "Total");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_ToAccountId",
                table: "Transfers",
                newName: "IX_Transfers_InvoiceId");

            migrationBuilder.AddColumn<string>(
                name: "Account",
                table: "Transfers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Transfers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Invoices_InvoiceId",
                table: "Transfers",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Invoices_InvoiceId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "Account",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Transfers");

            migrationBuilder.RenameColumn(
                name: "Total",
                table: "Transfers",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "InvoiceId",
                table: "Transfers",
                newName: "ToAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_InvoiceId",
                table: "Transfers",
                newName: "IX_Transfers_ToAccountId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Transfers",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FromAccountId",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TransferRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    KfsTrackingNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    RequestedById = table.Column<int>(type: "int", nullable: false),
                    RequestedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SlothTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransferRequests_Users_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ActionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ActorName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransferId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferHistory_TransferRequests_TransferId",
                        column: x => x.TransferId,
                        principalTable: "TransferRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_FromAccountId",
                table: "Transfers",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferHistory_TransferId",
                table: "TransferHistory",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_ProjectId",
                table: "TransferRequests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_RequestedById",
                table: "TransferRequests",
                column: "RequestedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Accounts_FromAccountId",
                table: "Transfers",
                column: "FromAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Accounts_ToAccountId",
                table: "Transfers",
                column: "ToAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
