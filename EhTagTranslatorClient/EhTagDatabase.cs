using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace EhTagTranslatorClient
{
    public class EhTagDatabase
    {
        public static uint ClientVersion { get; } = 2;

        public static IAsyncOperation<IList<Record>> FetchDatabaseAsync()
        {
            return ContentFetcher.Current.FetchDatabaseAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public static IAsyncOperation<IList<Record>> LoadDatabaseAsync()
        {
            return ContentFetcher.CurrentLocal.FetchDatabaseAsync();
        }
    }
}
