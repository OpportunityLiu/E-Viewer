using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
            set => Set(ref this.isLoaded, value, nameof(IsEmpty));
        }

        public new bool IsEmpty => this.Count == 0 && this.IsLoaded;

        public IAsyncOperation<int> FetchAsync()
        {
            return Task.Run(async () =>
            {
                if(this.Count != 0)
                {
                    this.Clear();
                }
                this.IsLoaded = false;
                var html = await Client.Current.HttpClient.GetStringAsync(new Uri(this.Owner.GalleryUri, "?hc=1"));
                var document = new HtmlDocument();
                document.LoadHtml(html);
                return AnalyzeDocument(document);
            }).AsAsyncOperation();
        }

        internal int AnalyzeDocument(HtmlDocument doc)
        {
            if(this.Count != 0)
            {
                this.Clear();
            }
            var c = this.AddRange(Comment.AnalyzeDocument(doc));
            this.IsLoaded = true;
            return c;
        }

        protected override IAsyncOperation<IReadOnlyList<Comment>> LoadPageAsync(int pageIndex)
        {
            throw new NotImplementedException();
        }
    }
}
