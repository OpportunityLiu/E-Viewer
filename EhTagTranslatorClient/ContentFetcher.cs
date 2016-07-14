using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace EhTagTranslatorClient
{
    public class ContentFetcher:IDisposable
    {
        public static ContentFetcher Current
        {
            get;
        } = new ContentFetcher();

        private ContentFetcher() { }

        private HttpClient client = new HttpClient();

        private static readonly Uri wikiRootUri = new Uri("https://raw.github.com/wiki/Mapaler/EhTagTranslator/");
        private static readonly Uri wikiRowsUri = new Uri(wikiRootUri, "rows.md");
        private static readonly Uri wikiTagsRootUri = new Uri(wikiRootUri, "tags/");

        public async void get()
        {
            var x = await client.GetInputStreamAsync(wikiRowsUri);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    client.Dispose();
                }
                client = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
