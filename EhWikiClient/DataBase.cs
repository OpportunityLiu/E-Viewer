using EhWikiClient.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EhWikiClient
{
    public class DataBase : IDisposable
    {
        internal DataBase()
        {
            db = new WikiDb();
        }

        public IQueryable<Record> Tags => db.Table.AsNoTracking();

        private WikiDb db;

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    db.Dispose();
                }
                db = null;
                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
