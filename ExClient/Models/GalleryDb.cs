using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ExClient.Models
{
    public class GalleryDb : DbContext
    {
        private const string dbFilename = "Gallery.db";

        public static void Migrate()
        {
            using(var db = new GalleryDb())
            {
                db.Database.Migrate();
            }
        }

        internal GalleryDb()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={dbFilename}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ImageModel>()
                .HasKey(i => new
                {
                    i.PageId,
                    i.OwnerId
                });
            modelBuilder.Entity<ImageModel>()
                .Property(i => i.PageId).ValueGeneratedNever();
            modelBuilder.Entity<ImageModel>()
                .Property(i => i.OwnerId).ValueGeneratedNever();

            modelBuilder.Entity<GalleryModel>()
                .HasKey(g => g.Id);
            modelBuilder.Entity<GalleryModel>()
                .Property(g => g.Id).ValueGeneratedNever();

            modelBuilder.Entity<CachedGalleryModel>()
                .HasKey(c => c.GalleryId);
            modelBuilder.Entity<CachedGalleryModel>()
                .Property(c => c.GalleryId).ValueGeneratedNever();

            modelBuilder.Entity<CachedGalleryModel>()
                .HasOne(c => c.Gallery)
                .WithOne()
                .HasForeignKey<CachedGalleryModel>(c => c.GalleryId);
            modelBuilder.Entity<ImageModel>()
                .HasOne(i => i.Owner)
                .WithMany(g => g.Images)
                .HasForeignKey(i => i.OwnerId);
        }

        internal DbSet<GalleryModel> GallerySet
        {
            get;
            set;
        }

        internal DbSet<ImageModel> ImageSet
        {
            get;
            set;
        }

        internal DbSet<CachedGalleryModel> CacheSet
        {
            get;
            set;
        }
    }
}
