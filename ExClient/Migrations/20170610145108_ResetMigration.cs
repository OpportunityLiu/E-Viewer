using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExClient.Migrations
{
    public partial class ResetMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GallerySet",
                columns: table => new
                {
                    GalleryModelId = table.Column<long>(nullable: false),
                    Available = table.Column<bool>(nullable: false),
                    Category = table.Column<uint>(nullable: false),
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
                    ImageId = table.Column<string>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    OriginalLoaded = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageSet", x => x.ImageId);
                });

            migrationBuilder.CreateTable(
                name: "SavedSet",
                columns: table => new
                {
                    GalleryId = table.Column<long>(nullable: false),
                    ThumbData = table.Column<byte[]>(nullable: true),
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
                    ImageId = table.Column<string>(nullable: false)
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
                        name: "FK_GalleryImageSet_ImageSet_ImageId",
                        column: x => x.ImageId,
                        principalTable: "ImageSet",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GalleryImageSet_ImageId",
                table: "GalleryImageSet",
                column: "ImageId");
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
