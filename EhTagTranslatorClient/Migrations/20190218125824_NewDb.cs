using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EhTagTranslatorClient.Migrations
{
    public partial class NewDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Table",
                columns: table => new
                {
                    Namespace = table.Column<int>(nullable: false),
                    Original = table.Column<string>(nullable: false),
                    Introduction = table.Column<string>(nullable: true),
                    Translated = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Table", x => new { x.Namespace, x.Original });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Table");
        }
    }
}
