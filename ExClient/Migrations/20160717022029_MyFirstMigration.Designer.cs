using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ExClient.Models;

namespace ExClient.Migrations
{
    [DbContext(typeof(GalleryDb))]
    [Migration("20160717022029_MyFirstMigration")]
    partial class MyFirstMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("ExClient.Models.CachedGalleryModel", b =>
                {
                    b.Property<long>("GalleryId");

                    b.Property<DateTimeOffset>("Saved");

                    b.Property<byte[]>("ThumbData");

                    b.HasKey("GalleryId");

                    b.HasIndex("GalleryId")
                        .IsUnique();

                    b.ToTable("CacheSet");
                });

            modelBuilder.Entity("ExClient.Models.GalleryModel", b =>
                {
                    b.Property<long>("Id");

                    b.Property<string>("ArchiverKey");

                    b.Property<bool>("Available");

                    b.Property<uint>("Category");

                    b.Property<bool>("Expunged");

                    b.Property<long>("FileSize");

                    b.Property<DateTimeOffset>("Posted");

                    b.Property<double>("Rating");

                    b.Property<int>("RecordCount");

                    b.Property<string>("Tags");

                    b.Property<string>("ThumbUri");

                    b.Property<string>("Title");

                    b.Property<string>("TitleJpn");

                    b.Property<string>("Token");

                    b.Property<string>("Uploader");

                    b.HasKey("Id");

                    b.ToTable("GallerySet");
                });

            modelBuilder.Entity("ExClient.Models.ImageModel", b =>
                {
                    b.Property<int>("PageId");

                    b.Property<long>("OwnerId");

                    b.Property<string>("FileName");

                    b.Property<string>("ImageKey");

                    b.Property<bool>("OriginalLoaded");

                    b.HasKey("PageId", "OwnerId");

                    b.HasIndex("OwnerId");

                    b.ToTable("ImageSet");
                });

            modelBuilder.Entity("ExClient.Models.CachedGalleryModel", b =>
                {
                    b.HasOne("ExClient.Models.GalleryModel", "Gallery")
                        .WithOne()
                        .HasForeignKey("ExClient.Models.CachedGalleryModel", "GalleryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ExClient.Models.ImageModel", b =>
                {
                    b.HasOne("ExClient.Models.GalleryModel", "Owner")
                        .WithMany("Images")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
