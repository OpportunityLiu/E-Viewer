using Microsoft.EntityFrameworkCore;

namespace EhTagTranslatorClient.Model
{
    internal class TranslateDb : DbContext
    {
        public DbSet<Record> Table { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=EhTagTranslatorClient.v2.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Record>()
                .HasKey(r => new { r.Namespace, r.Original });
            modelBuilder.Entity<Record>()
                .Property(r => r.Namespace)
                .ValueGeneratedNever();
            modelBuilder.Entity<Record>()
                .Property(r => r.Original)
                .ValueGeneratedNever();
        }
    }
}
