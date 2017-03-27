using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EhWikiClient.Migrations
{
    public partial class AddLastUpdateAndIsValid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                table: "Table",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "lastUpdate",
                table: "Table",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsValid",
                table: "Table");

            migrationBuilder.DropColumn(
                name: "lastUpdate",
                table: "Table");
        }
    }
}
