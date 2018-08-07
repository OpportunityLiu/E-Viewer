using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace ExViewer.Database
{
    class HistoryDb : DbContext
    {
        private const string dbFilename = "ExViewer.History.db";

        static HistoryDb()
        {
            using (var db = new HistoryDb())
            {
                db.Database.Migrate();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source ={dbFilename}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistoryRecord>().Ignore(sh => sh.Time);
            modelBuilder.Entity<HistoryRecord>().HasKey(sh => sh.Id);
            modelBuilder.Entity<HistoryRecord>().Property(sh => sh.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<HistoryRecord>().Property(sh => sh.TimeStamp)
                .IsRequired()
                .ValueGeneratedNever();
            modelBuilder.Entity<HistoryRecord>().Property(s => s.Title);
            modelBuilder.Entity<HistoryRecord>().HasIndex(s => s.Title);
            modelBuilder.Entity<HistoryRecord>().Property(s => s.Type);
            modelBuilder.Entity<HistoryRecord>().Property<string>("uri")
                .IsRequired();
            modelBuilder.Entity<HistoryRecord>().Ignore(sh => sh.Uri);
        }

        public DbSet<HistoryRecord> HistorySet { get; protected set; }

        public int AddHistory(HistoryRecord record)
        {
            if (record is null)
                throw new ArgumentNullException(nameof(record));
            if (record.Id != 0)
                throw new ArgumentException("Id of record is not 0.", nameof(record));
            record.UpdateTime();
            this.HistorySet.Add(record);
            this.SaveChanges();
            return record.Id;
        }

        public void UpdateHistory(HistoryRecord record)
        {
            if (record is null)
                throw new ArgumentNullException(nameof(record));
            if (record.Id == 0)
                throw new ArgumentException("Id of record is 0.", nameof(record));
            record.UpdateTime();
            this.HistorySet.Update(record);
            this.SaveChanges();
        }

        public void RemoveHistory(int id)
        {
            this.HistorySet.Remove(this.HistorySet.Find(id));
            this.SaveChanges();
        }

        public void ClearHistory()
        {
            this.HistorySet.RemoveRange(this.HistorySet);
            this.SaveChanges();
        }
    }

    enum HistoryRecordType
    {
        Default,
        Search,
        Favorite,
        Gallery,
        Image,
    }

    class HistoryRecord : IEquatable<HistoryRecord>
    {
        public int Id { get; set; }

        private string uri;
        public Uri Uri
        {
            get => this.uri.IsNullOrEmpty() ? null : new Uri(this.uri);
            set => this.uri = value?.ToString();
        }

        public string Title { get; set; }

        public HistoryRecordType Type { get; set; }

        public long TimeStamp { get; private set; }
        public DateTimeOffset Time => DateTimeOffset.FromUnixTimeMilliseconds(TimeStamp);

        public void UpdateTime()
        {
            this.TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public bool Equals(HistoryRecord other)
        {
            return this.uri == other.uri;
        }

        public override bool Equals(object obj)
        {
            if (obj is HistoryRecord hr)
                return Equals(hr);
            return false;
        }

        public override int GetHashCode()
        {
            return this.uri?.GetHashCode() ?? -1;
        }

        public override string ToString()
        {
            return this.Title ?? "";
        }
    }
}
