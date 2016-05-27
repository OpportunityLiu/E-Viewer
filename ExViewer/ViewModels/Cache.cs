using ExClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.ViewModels
{
    static class Cache
    {
        private class CacheStorage<T>
        {
            private Queue<string> cacheQueue = new Queue<string>();
            private Dictionary<string, T> cacheDictionary = new Dictionary<string, T>();

            public Func<string, T> Loader;
            public int MaxCount = 10;

            public void Add(string key, T value)
            {
                EnsureCapacity();
                if(cacheQueue.Contains(key))
                    cacheDictionary[key] = value;
                else
                {
                    cacheQueue.Enqueue(key);
                    cacheDictionary[key] = value;
                }
            }

            public T Get(string key)
            {
                EnsureCapacity();
                if(cacheQueue.Contains(key))
                    return cacheDictionary[key];
                else
                {
                    cacheQueue.Enqueue(key);
                    return cacheDictionary[key] = Loader(key);
                }
            }

            public void EnsureCapacity()
            {
                while(cacheQueue.Count > MaxCount)
                    cacheDictionary.Remove(cacheQueue.Dequeue());
            }
        }

        private class SearchResultData
        {
            public string KeyWord
            {
                get; set;
            }

            public Category Filter
            {
                get; set;
            }

            public AdvancedSearchOptions AdvancedSearch
            {
                get; set;
            }
        }

        private static CacheStorage<SearchResult> srCache = new CacheStorage<SearchResult>()
        {
            Loader = query =>
            {
                var data = JsonConvert.DeserializeObject<SearchResultData>(query);
                return Client.Current.Search(data.KeyWord, data.Filter, data.AdvancedSearch);
            },
            MaxCount = 3
        };

        public static SearchResult GetSearchResult(string query)
        {
            return srCache.Get(query);
        }

        public static string GetSearchQuery(string keyWord)
        {
            return JsonConvert.SerializeObject(new SearchResultData()
            {
                KeyWord = keyWord
            });
        }

        public static string GetSearchQuery(string keyWord, Category filter)
        {
            return JsonConvert.SerializeObject(new SearchResultData()
            {
                KeyWord = keyWord,
                Filter = filter
            });
        }

        public static string GetSearchQuery(string keyWord, Category filter, AdvancedSearchOptions advancedSearch)
        {
            return JsonConvert.SerializeObject(new SearchResultData()
            {
                KeyWord = keyWord,
                Filter = filter,
                AdvancedSearch = advancedSearch
            });
        }

        public static string AddSearchResult(SearchResult searchResult)
        {
            var query = JsonConvert.SerializeObject(new SearchResultData()
            {
                KeyWord = searchResult.KeyWord,
                Filter = searchResult.Filter,
                AdvancedSearch = (AdvancedSearchOptions)searchResult.AdvancedSearch
            });
            srCache.Add(query, searchResult);
            return query;
        }
    }
}
