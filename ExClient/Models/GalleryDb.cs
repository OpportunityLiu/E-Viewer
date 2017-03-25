using Microsoft.EntityFrameworkCore;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Models
{
    public class GalleryDb : DbContext
    {
        public static IAsyncAction MigrateAsync()
        {
            return Run(async token =>
            {
                using(var db = new GalleryDb())
                {
                    await db.Database.MigrateAsync(token);
                }
            });
        }

        internal GalleryDb()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=Gallery.db");
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

            modelBuilder.Entity<SavedGalleryModel>()
                .HasKey(c => c.GalleryId);
            modelBuilder.Entity<SavedGalleryModel>()
                .Property(c => c.GalleryId).ValueGeneratedNever();

            modelBuilder.Entity<SavedGalleryModel>()
                .HasOne(c => c.Gallery)
                .WithOne()
                .HasForeignKey<SavedGalleryModel>(c => c.GalleryId);
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

        internal DbSet<SavedGalleryModel> SavedSet
        {
            get;
            set;
        }
    }
}
