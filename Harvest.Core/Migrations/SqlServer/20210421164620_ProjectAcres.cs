using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.SqlServer
{
    public partial class ProjectAcres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcreageRateId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Acres",
                table: "Projects",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_AcreageRateId",
                table: "Projects",
                column: "AcreageRateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Rates_AcreageRateId",
                table: "Projects",
                column: "AcreageRateId",
                principalTable: "Rates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Rates_AcreageRateId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_AcreageRateId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "AcreageRateId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Acres",
                table: "Projects");
        }
    }
}
