using Microsoft.EntityFrameworkCore.Migrations;

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class CreateCrops : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CropLookups");

            migrationBuilder.CreateTable(
                name: "Crops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crops", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Crops_Name",
                table: "Crops",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Crops_Type",
                table: "Crops",
                column: "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Crops");

            migrationBuilder.CreateTable(
                name: "CropLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Crop = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropLookups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CropLookups_Crop",
                table: "CropLookups",
                column: "Crop");

            migrationBuilder.CreateIndex(
                name: "IX_CropLookups_Type",
                table: "CropLookups",
                column: "Type");
        }
    }
}
