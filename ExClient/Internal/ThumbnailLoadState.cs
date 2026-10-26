using System;

namespace ExClient.Internal {
    internal sealed class ThumbnailLoadState<T> where T : class {
        internal sealed class Request {
            internal Request(long version, ThumbnailSource source, object file) {
                Version = version;
                Source = source;
                File = file;
            }
            internal long Version { get; }
            public ThumbnailSource Source { get; }
            public object File { get; }
        }

        private readonly object gate = new object();
        private readonly WeakReference<T> result = new WeakReference<T>(null);
        private long version;
        private ThumbnailSource source;
        private object file;
        private Request active;

        public ThumbnailSource Source { get { lock (gate) return source; } }
        public object File { get { lock (gate) return file; } }
        public bool SetSource(ThumbnailSource value) {
            lock (gate) {
                if (Equals(source, value)) return false;
                source = value;
                Invalidate();
                return true;
            }
        }
        public bool SetFile(object value) {
            lock (gate) {
                if (ReferenceEquals(file, value)) return false;
                file = value;
                Invalidate();
                return true;
            }
        }
        private void Invalidate() {
            version++;
            active = null;
            result.SetTarget(null);
        }
        public bool TryGet(out T value) {
            lock (gate) return result.TryGetTarget(out value);
        }
        public bool TryBegin(out Request request) {
            lock (gate) {
                request = null;
                if (active != null || (source == null && file == null)) return false;
                active = request = new Request(version, source, file);
                return true;
            }
        }
        public bool Complete(Request request, T value, out bool retry) {
            lock (gate) {
                retry = false;
                if (!ReferenceEquals(active, request)) return false;
                active = null;
                retry = version != request.Version || !Equals(source, request.Source) || !ReferenceEquals(file, request.File);
                if (retry || value == null) return false;
                result.SetTarget(value);
                return true;
            }
        }
    }
}
