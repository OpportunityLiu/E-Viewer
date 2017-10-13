using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

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
                        RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning,
                        RelationalEventId.AmbientTransactionWarning)
                    .Throw(
                        CoreEventId.IncludeIgnoredWarning,
                        CoreEventId.SensitiveDataLoggingEnabledWarning));
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ImageModel>()
                .HasKey("Data0", "Data1", "Data2");
            modelBuilder.Entity<ImageModel>()
                .Ignore(i => i.ImageId);
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
                .HasKey(s => s.GalleryId);
            modelBuilder.Entity<SavedGalleryModel>()
                .Property(s => s.GalleryId).ValueGeneratedNever();
            modelBuilder.Entity<SavedGalleryModel>()
                .Ignore(s => s.Saved);
            modelBuilder.Entity<SavedGalleryModel>()
                .Property<long>("saved");

            modelBuilder.Entity<GalleryImageModel>()
                .HasKey(gi => new { gi.GalleryId, gi.PageId });
            modelBuilder.Entity<GalleryImageModel>()
                .Ignore(gi => gi.ImageId);
            modelBuilder.Entity<GalleryImageModel>()
                .Property("Data0").ValueGeneratedNever();
            modelBuilder.Entity<GalleryImageModel>()
                .Property("Data1").ValueGeneratedNever();
            modelBuilder.Entity<GalleryImageModel>()
                .Property("Data2").ValueGeneratedNever();

            modelBuilder.Entity<SavedGalleryModel>()
                .HasOne(c => c.Gallery)
                .WithOne()
                .HasForeignKey<SavedGalleryModel>(c => c.GalleryId);

            modelBuilder.Entity<GalleryImageModel>()
                .HasOne(gi => gi.Gallery)
                .WithMany(g => g.Images)
                .OnDelete(DeleteBehavior.Cascade)
                .HasForeignKey(gi => gi.GalleryId);
            modelBuilder.Entity<GalleryImageModel>()
                .HasOne(gi => gi.Image)
                .WithMany(i => i.UsingBy)
                .OnDelete(DeleteBehavior.Cascade)
                .HasForeignKey("Data0", "Data1", "Data2");
        }

        internal DbSet<GalleryModel> GallerySet { get; set; }

        internal DbSet<GalleryImageModel> GalleryImageSet { get; set; }

        internal DbSet<ImageModel> ImageSet { get; set; }

        internal DbSet<SavedGalleryModel> SavedSet { get; set; }
    }
}
