using Microsoft.EntityFrameworkCore.Migrations;

namespace ExViewer.Migrations
{
    public partial class MigrationReset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchHistorySet",
                columns: table => new
                {
                    time = table.Column<long>(nullable: false),
                    Content = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchHistorySet", x => x.time);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchHistorySet");
        }
    }
}
