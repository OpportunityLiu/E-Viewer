using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace ExViewer.Database
{
    internal class HistoryDb : DbContext
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

        public static int Add(HistoryRecord record)
        {
            if (record is null)
                throw new ArgumentNullException(nameof(record));
            if (record.Id != 0)
                throw new ArgumentException("Id of record is not 0.", nameof(record));

            record.UpdateTime();
            using (var db = new HistoryDb())
            {
                db.HistorySet.Add(record);
                db.SaveChanges();
                return record.Id;
            }
        }

        public static void Update(HistoryRecord record)
        {
            if (record is null)
                throw new ArgumentNullException(nameof(record));
            if (record.Id == 0)
                throw new ArgumentException("Id of record is 0.", nameof(record));
            record.UpdateTime();
            using (var db = new HistoryDb())
            {
                db.HistorySet.Update(record);
                db.SaveChanges();
            }
        }

        public static void Remove(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Id of record is 0.");
            using (var db = new HistoryDb())
            {
                db.HistorySet.Remove(db.HistorySet.Find(id));
                db.SaveChanges();
            }
        }

        public static void Clear()
        {
            using (var db = new HistoryDb())
            {
                db.HistorySet.RemoveRange(db.HistorySet);
                db.SaveChanges();
            }
        }
    }

    internal enum HistoryRecordType
    {
        Default,
        Search,
        Favorite,
        Gallery,
        Image,
    }

    internal class HistoryRecord : IEquatable<HistoryRecord>
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
