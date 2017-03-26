using EhTagTranslatorClient.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EhTagTranslatorClient
{
    public class DataBase : IDisposable
    {
        internal DataBase()
        {
            this.db = new TranslateDb();
        }

        public IQueryable<Record> Tags => this.db.Table.AsNoTracking();

        private TranslateDb db;

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if(!this.disposedValue)
            {
                if(disposing)
                {
                    this.db.Dispose();
                }
                this.db = null;
                this.disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
