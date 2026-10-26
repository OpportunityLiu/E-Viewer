using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExClient.Internal {
    internal sealed class ThumbnailDownloadCache {
        private sealed class Entry {
            public Uri Uri;
            public byte[] Bytes;
            public DateTimeOffset Expires;
        }

        private readonly object gate = new object();
        private readonly Dictionary<Uri, Task<byte[]>> pending = new Dictionary<Uri, Task<byte[]>>();
        private readonly LinkedList<Entry> entries = new LinkedList<Entry>();
        private readonly Func<Uri, Task<byte[]>> download;
        private readonly int maxBytes, maxEntries, maxPending;
        private readonly TimeSpan lifetime;
        private int bytes;

        public ThumbnailDownloadCache(Func<Uri, Task<byte[]>> download, int maxBytes, int maxEntries,
            int maxPending, TimeSpan lifetime) {
            this.download = download;
            this.maxBytes = maxBytes;
            this.maxEntries = maxEntries;
            this.maxPending = maxPending;
            this.lifetime = lifetime;
        }

        public Task<byte[]> GetAsync(Uri uri) {
            TaskCompletionSource<byte[]> completion;
            lock (gate) {
                var node = entries.First;
                while (node != null) {
                    var next = node.Next;
                    if (node.Value.Expires <= DateTimeOffset.UtcNow) Remove(node);
                    else if (node.Value.Uri == uri) {
                        entries.Remove(node);
                        entries.AddLast(node);
                        return Task.FromResult(node.Value.Bytes);
                    }
                    node = next;
                }
                if (pending.TryGetValue(uri, out var task)) return task;
                if (pending.Count >= maxPending) return Task.FromResult<byte[]>(null);
                completion = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
                pending.Add(uri, completion.Task);
            }
            _ = DownloadAsync(uri, completion);
            return completion.Task;
        }

        private async Task DownloadAsync(Uri uri, TaskCompletionSource<byte[]> completion) {
            byte[] value = null;
            try { value = await download(uri).ConfigureAwait(false); }
            catch (Exception) { }
            lock (gate) {
                pending.Remove(uri);
                if (value != null && value.Length <= maxBytes) {
                    while (entries.Count >= maxEntries || bytes > maxBytes - value.Length)
                        Remove(entries.First);
                    entries.AddLast(new Entry { Uri = uri, Bytes = value, Expires = DateTimeOffset.UtcNow + lifetime });
                    bytes += value.Length;
                }
            }
            completion.TrySetResult(value);
        }

        private void Remove(LinkedListNode<Entry> node) {
            bytes -= node.Value.Bytes.Length;
            entries.Remove(node);
        }
    }
}
