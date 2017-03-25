using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
