using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExClient.Migrations
{
    public partial class NewMig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GallerySet",
                columns: table => new
                {
                    GalleryModelId = table.Column<long>(nullable: false),
                    Available = table.Column<bool>(nullable: false),
                    Category = table.Column<int>(nullable: false),
                    Expunged = table.Column<bool>(nullable: false),
                    FileSize = table.Column<long>(nullable: false),
                    Rating = table.Column<double>(nullable: false),
                    RecordCount = table.Column<int>(nullable: false),
                    Tags = table.Column<string>(nullable: true),
                    ThumbUri = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    TitleJpn = table.Column<string>(nullable: true),
                    Token = table.Column<ulong>(nullable: false),
                    Uploader = table.Column<string>(nullable: true),
                    posted = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GallerySet", x => x.GalleryModelId);
                });

            migrationBuilder.CreateTable(
                name: "ImageSet",
                columns: table => new
                {
                    Data0 = table.Column<ulong>(nullable: false),
                    Data1 = table.Column<ulong>(nullable: false),
                    Data2 = table.Column<uint>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    OriginalLoaded = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageSet", x => new { x.Data0, x.Data1, x.Data2 });
                });

            migrationBuilder.CreateTable(
                name: "SavedSet",
                columns: table => new
                {
                    GalleryId = table.Column<long>(nullable: false),
                    saved = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedSet", x => x.GalleryId);
                    table.ForeignKey(
                        name: "FK_SavedSet_GallerySet_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "GallerySet",
                        principalColumn: "GalleryModelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GalleryImageSet",
                columns: table => new
                {
                    GalleryId = table.Column<long>(nullable: false),
                    PageId = table.Column<int>(nullable: false),
                    Data0 = table.Column<ulong>(nullable: false),
                    Data1 = table.Column<ulong>(nullable: false),
                    Data2 = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryImageSet", x => new { x.GalleryId, x.PageId });
                    table.ForeignKey(
                        name: "FK_GalleryImageSet_GallerySet_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "GallerySet",
                        principalColumn: "GalleryModelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GalleryImageSet_ImageSet_Data0_Data1_Data2",
                        columns: x => new { x.Data0, x.Data1, x.Data2 },
                        principalTable: "ImageSet",
                        principalColumns: new[] { "Data0", "Data1", "Data2" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GalleryImageSet_Data0_Data1_Data2",
                table: "GalleryImageSet",
                columns: new[] { "Data0", "Data1", "Data2" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GalleryImageSet");

            migrationBuilder.DropTable(
                name: "SavedSet");

            migrationBuilder.DropTable(
                name: "ImageSet");

            migrationBuilder.DropTable(
                name: "GallerySet");
        }
    }
}
