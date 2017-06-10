using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ExClient.Models;
using ExClient;

namespace ExClient.Migrations
{
    [DbContext(typeof(GalleryDb))]
    [Migration("20170610145108_ResetMigration")]
    partial class ResetMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("ExClient.Models.GalleryImageModel", b =>
                {
                    b.Property<long>("GalleryId");

                    b.Property<int>("PageId");

                    b.Property<string>("ImageId")
                        .IsRequired();

                    b.HasKey("GalleryId", "PageId");

                    b.HasIndex("ImageId");

                    b.ToTable("GalleryImageSet");
                });

            modelBuilder.Entity("ExClient.Models.GalleryModel", b =>
                {
                    b.Property<long>("GalleryModelId");

                    b.Property<bool>("Available");

                    b.Property<uint>("Category");

                    b.Property<bool>("Expunged");

                    b.Property<long>("FileSize");

                    b.Property<double>("Rating");

                    b.Property<int>("RecordCount");

                    b.Property<string>("Tags");

                    b.Property<string>("ThumbUri");

                    b.Property<string>("Title");

                    b.Property<string>("TitleJpn");

                    b.Property<ulong>("Token");

                    b.Property<string>("Uploader");

                    b.Property<long>("posted");

                    b.HasKey("GalleryModelId");

                    b.ToTable("GallerySet");
                });

            modelBuilder.Entity("ExClient.Models.ImageModel", b =>
                {
                    b.Property<string>("ImageId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FileName");

                    b.Property<bool>("OriginalLoaded");

                    b.HasKey("ImageId");

                    b.ToTable("ImageSet");
                });

            modelBuilder.Entity("ExClient.Models.SavedGalleryModel", b =>
                {
                    b.Property<long>("GalleryId");

                    b.Property<byte[]>("ThumbData");

                    b.Property<long>("saved");

                    b.HasKey("GalleryId");

                    b.ToTable("SavedSet");
                });

            modelBuilder.Entity("ExClient.Models.GalleryImageModel", b =>
                {
                    b.HasOne("ExClient.Models.GalleryModel", "Gallery")
                        .WithMany("Images")
                        .HasForeignKey("GalleryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ExClient.Models.ImageModel", "Image")
                        .WithMany("UsingBy")
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ExClient.Models.SavedGalleryModel", b =>
                {
                    b.HasOne("ExClient.Models.GalleryModel", "Gallery")
                        .WithOne()
                        .HasForeignKey("ExClient.Models.SavedGalleryModel", "GalleryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
