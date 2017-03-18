using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient
{
    public sealed class CommentCollection : IncrementalLoadingCollection<Comment>
    {
        public CommentCollection(Gallery owner)
            : base(0)
        {
            this.Owner = owner;
        }

        public Gallery Owner { get; }

        private bool isLoaded;

        public bool IsLoaded
        {
            get => isLoaded;
            set => Set(nameof(IsEmpty), ref this.isLoaded, value);
        }

        public new bool IsEmpty => this.Count == 0 && this.IsLoaded;

        private readonly object syncroot = new object();

        public IAsyncOperation<int> FetchAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                this.Clear();
                this.IsLoaded = false;
                var get = Client.Current.HttpClient.GetStringAsync(new Uri(this.Owner.GalleryUri, "?hc=1"));
                token.Register(get.Cancel);
                var html = await get;
                if(this.IsLoaded)
                    return this.Count;
                var document = new HtmlDocument();
                document.LoadHtml(html);
                return AnalyzeDocument(document);
            });
        }

        internal int AnalyzeDocument(HtmlDocument doc)
        {
            lock(this.syncroot)
            {
                if(this.IsLoaded)
                    return this.Count;
                this.Clear();
                var c = this.AddRange(Comment.AnalyzeDocument(this, doc));
                this.IsLoaded = true;
                return c;
            }
        }

        protected override IAsyncOperation<IReadOnlyList<Comment>> LoadPageAsync(int pageIndex)
        {
            throw new NotImplementedException();
        }
    }
}
