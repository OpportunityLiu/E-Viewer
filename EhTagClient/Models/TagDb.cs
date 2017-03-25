using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

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
