using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Models
{
    public class GalleryDb : DbContext
    {
        static GalleryDb()
        {
            using (var db = new GalleryDb())
            {
                db.Database.Migrate();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=ExClient.Gallery.db");
#if DEBUG
            optionsBuilder
                .ConfigureWarnings(warnings => warnings
                    .Throw(
                        RelationalEventId.QueryClientEvaluationWarning, 
                        RelationalEventId.PossibleUnintendedUseOfEqualsWarning, 
                        RelationalEventId.AmbientTransactionWarning)
                    .Throw(
                        CoreEventId.IncludeIgnoredWarning,
                        CoreEventId.ModelValidationWarning,
                        CoreEventId.SensitiveDataLoggingEnabledWarning));
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ImageModel>()
                .HasKey(i => i.ImageId);
            modelBuilder.Entity<ImageModel>()
                .Property(i => i.FileName).ValueGeneratedNever();

            modelBuilder.Entity<GalleryModel>()
                .HasKey(g => g.GalleryModelId);
            modelBuilder.Entity<GalleryModel>()
                .Property(g => g.GalleryModelId).ValueGeneratedNever();
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

            modelBuilder.Entity<GalleryImageModel>()
                .HasKey(gi => new { gi.GalleryId, gi.PageId });
            modelBuilder.Entity<GalleryImageModel>()
                .Property(gi => gi.ImageId).IsRequired().ValueGeneratedNever();

            modelBuilder.Entity<SavedGalleryModel>()
                .HasOne(c => c.Gallery)
                .WithOne()
                .HasForeignKey<SavedGalleryModel>(c => c.GalleryId);

            modelBuilder.Entity<GalleryImageModel>()
                .HasOne(gi => gi.Gallery)
                .WithMany(g => g.Images)
                .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade)
                .HasForeignKey(gi => gi.GalleryId);
            modelBuilder.Entity<GalleryImageModel>()
                .HasOne(gi => gi.Image)
                .WithMany(i => i.UsingBy)
                .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade)
                .HasForeignKey(gi => gi.ImageId);
        }

        internal DbSet<GalleryModel> GallerySet { get; set; }

        internal DbSet<GalleryImageModel> GalleryImageSet { get; set; }

        internal DbSet<ImageModel> ImageSet { get; set; }

        internal DbSet<SavedGalleryModel> SavedSet { get; set; }
    }
}
