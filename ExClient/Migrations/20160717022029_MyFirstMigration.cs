using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExClient.Migrations
{
    public partial class MyFirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GallerySet",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    ArchiverKey = table.Column<string>(nullable: true),
                    Available = table.Column<bool>(nullable: false),
                    Category = table.Column<uint>(nullable: false),
                    Expunged = table.Column<bool>(nullable: false),
                    FileSize = table.Column<long>(nullable: false),
                    Posted = table.Column<DateTimeOffset>(nullable: false),
                    Rating = table.Column<double>(nullable: false),
                    RecordCount = table.Column<int>(nullable: false),
                    Tags = table.Column<string>(nullable: true),
                    ThumbUri = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    TitleJpn = table.Column<string>(nullable: true),
                    Token = table.Column<string>(nullable: true),
                    Uploader = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GallerySet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedSet",
                columns: table => new
                {
                    GalleryId = table.Column<long>(nullable: false),
                    Saved = table.Column<DateTimeOffset>(nullable: false),
                    ThumbData = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedSet", x => x.GalleryId);
                    table.ForeignKey(
                        name: "FK_SavedSet_GallerySet_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "GallerySet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImageSet",
                columns: table => new
                {
                    PageId = table.Column<int>(nullable: false),
                    OwnerId = table.Column<long>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    ImageKey = table.Column<string>(nullable: true),
                    OriginalLoaded = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageSet", x => new { x.PageId, x.OwnerId });
                    table.ForeignKey(
                        name: "FK_ImageSet_GallerySet_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "GallerySet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedSet_GalleryId",
                table: "SavedSet",
                column: "GalleryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageSet_OwnerId",
                table: "ImageSet",
                column: "OwnerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedSet");

            migrationBuilder.DropTable(
                name: "ImageSet");

            migrationBuilder.DropTable(
                name: "GallerySet");
        }
    }
}
