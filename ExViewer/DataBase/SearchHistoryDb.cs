using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Microsoft.Data.Sqlite;

namespace ExViewer.Database
{
    class SearchHistoryDb : DbContext
    {
        private const string dbFilename = "ExViewer.SearchHistory.db";

        static SearchHistoryDb()
        {
            using(var db = new SearchHistoryDb())
            {
                db.Database.Migrate();
            }
        }

        public SearchHistoryDb()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source ={dbFilename}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SearchHistory>().HasKey("time");
            modelBuilder.Entity<SearchHistory>().Property<long>("time")
                .ValueGeneratedNever();
            modelBuilder.Entity<SearchHistory>().Property(s => s.Content);
            modelBuilder.Entity<SearchHistory>().Ignore(sh => sh.Highlight);
            modelBuilder.Entity<SearchHistory>().Ignore(sh => sh.Time);
        }

        public DbSet<SearchHistory> SearchHistorySet
        {
            get;
            set;
        }
    }

    class SearchHistory : IEquatable<SearchHistory>
    {
        public string Content
        {
            get; set;
        }

        public string Highlight
        {
            get; private set;
        }

        public SearchHistory SetHighlight(string highlight)
        {
            this.Highlight = highlight;
            return this;
        }

        private long time;

        public DateTimeOffset Time
        {
            get => DateTimeOffset.FromUnixTimeMilliseconds(time);
            set => this.time = value.ToUnixTimeMilliseconds();
        }

        public static SearchHistory Create(string content)
        {
            return new SearchHistory
            {
                Content = (content ?? string.Empty).Trim(),
                Time = DateTimeOffset.UtcNow
            };
        }

        public bool Equals(SearchHistory other)
        {
            return this.Content == other.Content;
        }

        public override bool Equals(object obj)
        {
            if(obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals((SearchHistory)obj);
        }

        public override int GetHashCode()
        {
            return this.Content.GetHashCode();
        }

        public override string ToString()
        {
            return this.Content;
        }
    }
}
