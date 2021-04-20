using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class invoiceFieldsForSloth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KfsTrackingNumber",
                table: "Invoices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SlothTransactionId",
                table: "Invoices",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KfsTrackingNumber",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SlothTransactionId",
                table: "Invoices");
        }
    }
}
