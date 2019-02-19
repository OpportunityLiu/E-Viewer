using EhTagClient.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EhTagClient
{
    public class DataBase : IDisposable
    {
        internal DataBase()
        {
            db = new TagDb();
        }

        public IQueryable<TagRecord> Tags => db.TagTable.AsNoTracking();

        private TagDb db;

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
