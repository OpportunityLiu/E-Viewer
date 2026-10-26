using System;

namespace ExClient.Internal {
    internal readonly struct ThumbnailRegion : IEquatable<ThumbnailRegion> {
        public ThumbnailRegion(long x, long y, long width, long height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public long X { get; }
        public long Y { get; }
        public long Width { get; }
        public long Height { get; }

        public bool Fits(uint width, uint height) => X >= 0 && Y >= 0 && Width > 0 && Height > 0
            && X < width && Y < height && Width <= width - X && Height <= height - Y;

        public bool Equals(ThumbnailRegion other) => X == other.X && Y == other.Y
            && Width == other.Width && Height == other.Height;
        public override bool Equals(object obj) => obj is ThumbnailRegion other && Equals(other);
        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
    }

    internal sealed class ThumbnailSource : IEquatable<ThumbnailSource> {
        public ThumbnailSource(Uri uri, ThumbnailRegion? region = null) {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            Region = region;
        }

        public Uri Uri { get; }
        public ThumbnailRegion? Region { get; }
        public ThumbnailSource WithUri(Uri uri) => new ThumbnailSource(uri, Region);
        public bool Equals(ThumbnailSource other) => other != null && Uri == other.Uri && Region.Equals(other.Region);
        public override bool Equals(object obj) => Equals(obj as ThumbnailSource);
        public override int GetHashCode() => Uri.GetHashCode() ^ Region.GetHashCode();
    }
}
