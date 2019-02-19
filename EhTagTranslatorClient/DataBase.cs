using EhTagTranslatorClient.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EhTagTranslatorClient
{
    public sealed class DataBase : IDisposable
    {
        static DataBase()
        {
            using (var db = new TranslateDb())
            {
                db.Database.Migrate();
            }
        }

        internal DataBase() => Db = new TranslateDb();

        public IQueryable<Record> Tags => Db.Table.AsNoTracking();

        internal TranslateDb Db { get; private set; }

        void IDisposable.Dispose()
        {
            if (Db is TranslateDb db)
            {
                db.Dispose();
                Db = null;
            }
        }
    }
}
