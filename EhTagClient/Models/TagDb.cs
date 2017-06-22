using Microsoft.EntityFrameworkCore;

namespace EhTagClient.Models
{
    internal class TagDb : DbContext
    {
        public static void Migrate()
        {
            using(var db = new TagDb())
            {
                db.Database.Migrate();
            }
        }

        public DbSet<TagRecord> TagTable { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=EhTagClient.db");
        }
    }
}
