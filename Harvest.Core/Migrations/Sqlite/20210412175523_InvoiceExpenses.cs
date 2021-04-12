using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class InvoiceExpenses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Total = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    Total = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    RateId = table.Column<int>(type: "INTEGER", nullable: false),
                    InvoiceId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Expenses_Rates_RateId",
                        column: x => x.RateId,
                        principalTable: "Rates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Expenses_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CreatedById",
                table: "Expenses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_InvoiceId",
                table: "Expenses",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ProjectId",
                table: "Expenses",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_RateId",
                table: "Expenses",
                column: "RateId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ProjectId",
                table: "Invoices",
                column: "ProjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Invoices");
        }
    }
}
