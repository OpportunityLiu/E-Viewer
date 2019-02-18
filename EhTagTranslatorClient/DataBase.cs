using EhTagTranslatorClient.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EhTagTranslatorClient
{
    public class DataBase : IDisposable
    {
        internal DataBase() => _Db = new TranslateDb();

        public IQueryable<Record> Tags => _Db.Table.AsNoTracking();

        private TranslateDb _Db;

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if(!this.disposedValue)
            {
                if(disposing)
                {
                    this._Db.Dispose();
                }
                this._Db = null;
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
