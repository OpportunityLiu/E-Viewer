using ExClient.Api;
using ExClient.Galleries;
using ExClient.Launch;
using ExClient.Search;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public static async Task<List<HistoryRecord>> GetAsync(int limit = -1)
        {
            using (var db = new HistoryDb())
            {
                IQueryable<HistoryRecord> data = db.HistorySet.OrderByDescending(h => h.TimeStamp);
                if (limit > 0)
                    data = data.Take(limit);
                return await data.ToListAsync();
            }
        }

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

        public static void Remove(Uri uri)
        {
            if (uri is null)
                throw new ArgumentNullException(nameof(uri));
            var str = uri.ToString();
            using (var db = new HistoryDb())
            {
                db.HistorySet.RemoveRange(db.HistorySet.Where(r => EF.Property<string>(r, "uri") == str));
                db.SaveChanges();
            }
        }

        public static void Remove(Expression<Func<HistoryRecord, bool>> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            using (var db = new HistoryDb())
            {
                db.HistorySet.RemoveRange(db.HistorySet.Where(predicate));
                db.SaveChanges();
            }
        }

        public static async Task ClearAsync()
        {
            using (var db = new HistoryDb())
            {
                db.HistorySet.RemoveRange(db.HistorySet);
                await db.SaveChangesAsync();
            }
        }
    }

    internal enum HistoryRecordType
    {
        Default,
        Search,
        Favorites,
        Gallery,
        Image,
    }

    internal class HistoryRecord : IEquatable<HistoryRecord>
    {
        public int Id { get; set; }

        private string uri;
        public Uri Uri
        {
            get => uri.IsNullOrEmpty() ? null : new Uri(uri);
            set => uri = (value ?? throw new ArgumentNullException(nameof(value))).ToString();
        }

        public string Title { get; set; }

        public HistoryRecordType Type { get; set; }

        public long TimeStamp { get; private set; }
        public DateTimeOffset Time => DateTimeOffset.FromUnixTimeMilliseconds(TimeStamp);

        public void UpdateTime()
        {
            TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public bool Equals(HistoryRecord other)
        {
            if (other is null)
                return false;
            if (Id != other.Id)
                return false;
            if (Id > 0)
                return true;

            return uri == other.uri;
        }

        public override bool Equals(object obj)
        {
            if (obj is HistoryRecord hr)
                return Equals(hr);
            return false;
        }

        public override int GetHashCode()
        {
            if (Id > 0)
                return Id;
            return uri?.GetHashCode() ?? -1;
        }

        public override string ToString()
        {
            return Title ?? "";
        }

        public string ToDisplayString()
        {
            var resource = Strings.Resources.JumpList.Recent.HistoryRecord;
            var title = Title is null ? "" : Title.Trim();
            try
            {
                switch (Type)
                {
                case HistoryRecordType.Search:
                    return resource.Search(title);
                case HistoryRecordType.Favorites:
                {
                    var data = FavoritesSearchResult.Parse(Uri);
                    return resource.Favorites(title, data.Category);
                }
                case HistoryRecordType.Gallery:
                    return resource.Gallery(title);
                case HistoryRecordType.Image:
                {
                    var data = ImageInfo.Parse(Uri);
                    return resource.Image(title, data.PageID);
                }
                default:
                    return resource.Default(title);
                }
            }
            catch (Exception ex)
            {
                Telemetry.LogException(ex);
                return $"{title} - E-Viewer";
            }
        }
    }
}
