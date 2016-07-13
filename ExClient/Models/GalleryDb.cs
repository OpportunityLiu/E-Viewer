using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Models
{
    class GalleryDb : DbContext
    {
        private const string dbFilename = "Gallery.db";
        private static object syncroot = new object();
        private static bool created = false;

        public static GalleryDb Create()
        {
            var db = new GalleryDb();
            if(!created)
                lock(syncroot)
                    if(!created)
                    {
                        db.Database.EnsureCreated();
                        created = true;
                    }
            return db;
        }

        public static void Delete()
        {
            if(created)
                lock(syncroot)
                    if(created)
                    {
                        new GalleryDb().Database.EnsureDeleted();
                        created = false;
                    }
        }

        public static IAsyncAction DeleteAsync()
        {
            return Task.Run((Action)Delete).AsAsyncAction();
        }

        protected GalleryDb()
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

        public DbSet<GalleryModel> GallerySet
        {
            get;
            set;
        }

        public DbSet<ImageModel> ImageSet
        {
            get;
            set;
        }

        public DbSet<CachedGalleryModel> CacheSet
        {
            get;
            set;
        }
    }
}
