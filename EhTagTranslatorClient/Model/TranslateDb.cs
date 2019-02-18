using Microsoft.EntityFrameworkCore;

namespace EhTagTranslatorClient.Model
{
    class TranslateDb : DbContext
    {
        static TranslateDb()
        {
            using (var db = new TranslateDb())
            {
                db.Database.Migrate();
            }
        }

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
