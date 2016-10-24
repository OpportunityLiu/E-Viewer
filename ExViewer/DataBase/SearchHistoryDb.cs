using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.Collections;

namespace ExViewer.Database
{
    struct SearchHistoryDb : ICollection<SearchHistory>
    {
        private static ApplicationDataManager.ApplicationDataDictionary<string> SearchHistorySet
        {
            get;
        } = new ApplicationDataManager.ApplicationDataDictionary<string>("SearchHistory", Windows.Storage.ApplicationDataLocality.Local);

        public int Count => SearchHistorySet.Count;

        public bool IsReadOnly => false;

        public void Add(SearchHistory item)
        {
            SearchHistorySet[item.Time.ToUnixTimeSeconds().ToString()] = item.Content;
        }

        public void Clear()
        {
            SearchHistorySet.Clear();
        }

        public bool Contains(SearchHistory item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(SearchHistory[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<SearchHistory> GetEnumerator()
        {
            return SearchHistorySet.Select(kv => new SearchHistory
            {
                Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(kv.Key)),
                Content = kv.Value
            }).GetEnumerator();
        }

        public bool Remove(SearchHistory item)
        {
            return SearchHistorySet.Remove(item.Time.ToUnixTimeSeconds().ToString());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class SearchHistory : IEquatable<SearchHistory>
    {
        public string Content
        {
            get; set;
        }

        public string Highlight
        {
            get; private set;
        }

        public SearchHistory SetHighlight(string highlight)
        {
            Highlight = highlight;
            return this;
        }

        public DateTimeOffset Time
        {
            get; set;
        }

        public static SearchHistory Create(string content)
        {
            return new SearchHistory
            {
                Content = (content ?? string.Empty).Trim(),
                Time = DateTimeOffset.UtcNow
            };
        }

        public bool Equals(SearchHistory other)
        {
            return this.Content == other.Content;
        }

        public override bool Equals(object obj)
        {
            if(obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals((SearchHistory)obj);
        }

        public override int GetHashCode()
        {
            return Content.GetHashCode();
        }

        public override string ToString()
        {
            return Content;
        }
    }
}
