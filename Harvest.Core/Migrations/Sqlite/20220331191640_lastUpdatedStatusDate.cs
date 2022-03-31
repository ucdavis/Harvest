﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harvest.Core.Migrations.Sqlite
{
    public partial class lastUpdatedStatusDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusUpdatedOn",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastStatusUpdatedOn",
                table: "Projects");
        }
    }
}
