using Microsoft.EntityFrameworkCore;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Models
{
    public class GalleryDb : DbContext
    {
        static GalleryDb()
        {
            using(var db = new GalleryDb())
            {
                db.Database.Migrate();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=ExClient.db");
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
            modelBuilder.Entity<GalleryModel>()
                .Ignore(g => g.Posted);
            modelBuilder.Entity<GalleryModel>()
                .Property<long>("posted");

            modelBuilder.Entity<SavedGalleryModel>()
                .HasKey(c => c.GalleryId);
            modelBuilder.Entity<SavedGalleryModel>()
                .Property(c => c.GalleryId).ValueGeneratedNever();
            modelBuilder.Entity<SavedGalleryModel>()
                .Ignore(c => c.Saved);
            modelBuilder.Entity<SavedGalleryModel>()
                .Property<long>("saved");

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
