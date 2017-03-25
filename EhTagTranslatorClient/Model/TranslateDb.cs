using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace EhTagTranslatorClient.Model
{
    class TranslateDb : DbContext
    {
        public static void Migrate()
        {
            using(var db = new TranslateDb())
            {
                db.Database.Migrate();
            }
        }

        public DbSet<Record> Table { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=EhTagTranslatorClient.db");
        }
    }
}
