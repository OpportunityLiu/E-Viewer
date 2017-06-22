using Microsoft.EntityFrameworkCore;

namespace EhWikiClient.Models
{
    internal class WikiDb : DbContext
    {
        public static void Migrate()
        {
            using(var db = new WikiDb())
            {
                db.Database.Migrate();
            }
        }

        public DbSet<Record> Table { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=EhWikiClient.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Record>()
                .Property<long>("lastUpdate");
        }
    }
}
