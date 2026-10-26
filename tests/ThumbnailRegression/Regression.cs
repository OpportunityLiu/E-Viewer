using ExClient.Internal;

using HtmlAgilityPack;

using System;
using System.Threading.Tasks;

namespace ThumbnailRegression {
    internal sealed class Regression {
        private readonly Action<string> log;
        public int Checks { get; private set; }
        public Regression(Action<string> log) { this.log = log; }
        public void Check(bool condition, string name) {
            if (!condition) throw new InvalidOperationException("FAIL: " + name);
            Checks++;
            log("PASS: " + name);
        }

        private static ThumbnailSource Style(string css) => ThumbnailParser.ParseStyle(css);
        private static ThumbnailSource Html(string html) {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return ThumbnailParser.Parse(document.DocumentNode.SelectSingleNode("//a"));
        }
        private const string Dimensions = "width:64px;height:80px;";
        private const string Image = "url(https://fixture.invalid/sheet.png)";

        public async Task RunAsync(bool smokeOnly = false) {
            var first = Style(Dimensions + "background:" + Image + " 0 0 no-repeat;");
            var middle = Style(Dimensions + "background:" + Image + " -64px 0 no-repeat;");
            var last = Style("width:96px;height:48px;background:" + Image + " -128px -32px;");
            Check(first?.Region?.X == 0 && middle?.Region?.X == 64 && last?.Region?.X == 128,
                "same URL, distinct first/middle/last offsets");
            Check(Style(Dimensions + "background-position:-200px 0;background:" + Image + " 0 0 no-repeat;")?.Region?.X == 0,
                "later shorthand resets position");
            Check(Style(Dimensions + "background:" + Image + " -100px 0 no-repeat;background-position:-300px 0;")?.Region?.X == 300,
                "later longhand overrides position");
            var complex = Style(Dimensions + "background:url(\"https://fixture.invalid/sheet.png?sig=a;b(c),d\") -64px 0;");
            Check(complex?.Uri.Query == "?sig=a;b(c),d" && complex.Region?.X == 64,
                "quoted URL semicolon, parentheses and comma are not CSS delimiters");
            if (smokeOnly) return;
            Check(last.Region?.Width == 96 && last.Region?.Height == 48 && last.Region?.Y == 32, "mixed cell sizes");
            Check(Style(" HEIGHT : 80PX ; background-position: -64px 0; WIDTH: 64px; background-image: url( 'https://fixture.invalid/sheet.png' ); ")?.Equals(middle) == true,
                "reordered properties, separated position, quotes and whitespace");
            Check(Style(Dimensions + "background-image:" + Image + ";")?.Equals(first) == true, "CSS default top-left single image");
            Check(Html("<a><img src='https://fixture.invalid/plain.jpg?a=1&amp;b=2'/></a>")?.Uri.Query == "?a=1&b=2", "ordinary img and HTML entities");
            Check(Html("<a><div style='width:1px'><img src='https://fixture.invalid/plain.jpg'/></div></a>")?.Region == null, "legacy img inside non-background div");
            Check(Html("<a></a>") == null && ThumbnailParser.Parse(null) == null, "missing nodes are safe");
            foreach (var css in new[] {
                "width:64px;background:" + Image + " -64px 0;",
                "height:64px;background:" + Image + " -64px 0;",
                "width:0;height:64px;background:" + Image + ";",
                "width:64em;height:64px;background:" + Image + ";",
                Dimensions + "background:" + Image + " 5px 0;",
                Dimensions + "background:" + Image + " -9223372036854775808px 0;",
                Dimensions + "background:" + Image + " 50% 0;",
                Dimensions + "background:" + Image + " center center;",
                Dimensions + "background:" + Image + " 0 0 / cover;",
                Dimensions + "background:" + Image + ";background-size:200px 200px;",
                Dimensions + "background:" + Image + ";background-origin:content-box;",
                Dimensions + "background:" + Image + "," + Image + ";",
                Dimensions + "background:linear-gradient(red,blue)," + Image + ";",
                Dimensions + "background-image: url('https://fixture.invalid/a.png);",
                Dimensions + "background-image: url(file:///C:/secret.png);",
                Dimensions + "background-image:" + Image + " !important;"
            }) Check(Style(css) == null, "reject missing/invalid/unsupported CSS: " + css);
            Check(Html("<a><div style='background-size:cover'></div><img src='https://fixture.invalid/sheet.png'/></a>") == null,
                "known invalid sprite never falls back to img sheet");
            foreach (var region in new[] {
                new ThumbnailRegion(-1, 0, 1, 1), new ThumbnailRegion(0, -1, 1, 1),
                new ThumbnailRegion(0, 0, 0, 1), new ThumbnailRegion(0, 0, 1, -1),
                new ThumbnailRegion(224, 0, 1, 1), new ThumbnailRegion(220, 0, 8, 1),
                new ThumbnailRegion(0, 70, 1, 20), new ThumbnailRegion(long.MaxValue, 0, 1, 1),
                new ThumbnailRegion(0, 0, long.MaxValue, long.MaxValue)
            }) Check(!region.Fits(224, 80), "signed/empty/overflow/out-of-bounds: " + region.X + "," + region.Y + "," + region.Width + "," + region.Height);
            Check(new ThumbnailRegion(128, 32, 96, 48).Fits(224, 80), "exact bottom-right edge fits");
            TestState(first, middle);
            await TestCacheAsync();
        }

        private void TestState(ThumbnailSource first, ThumbnailSource middle) {
            var state = new ThumbnailLoadState<object>();
            state.SetSource(first);
            Check(state.TryBegin(out var remote) && !state.TryBegin(out _), "single-flight remote loading");
            var file = new object();
            state.SetFile(file);
            Check(state.TryBegin(out var local) && ReferenceEquals(local.File, file), "local loading starts without waiting for pending remote");
            Check(!state.Complete(remote, new object(), out var retry) && !retry && !state.TryGet(out _), "late remote cannot publish over local file");
            Check(!state.TryBegin(out _), "stale remote completion does not clear current local request");
            var localResult = new object();
            Check(state.Complete(local, localResult, out _) && state.TryGet(out var result) && ReferenceEquals(result, localResult), "current local result publishes");
            state.SetSource(middle);
            Check(!state.TryGet(out _) && state.Source.Equals(middle) && state.File == file, "same URI, changed region invalidates thumbnail but preserves local file");
            state.TryBegin(out var oldRegion);
            state.SetFile(null);
            Check(!state.Complete(oldRegion, new object(), out retry) && !retry, "late local result cannot publish after file removal");
            state.TryBegin(out var failed);
            Check(!state.Complete(failed, null, out retry) && !retry, "failed request clears single-flight state");
            Check(state.TryBegin(out var retried), "failed request can be retried");
            state.SetSource(first);
            state.TryBegin(out var current);
            Check(!state.Complete(retried, new object(), out retry) && !retry, "late source/region result cannot publish or restart obsolete work");
            Check(current.Source.Equals(first) && current.File == null && state.Complete(current, new object(), out _), "new source may publish without waiting for stale request");
        }

        private async Task TestCacheAsync() {
            var calls = 0;
            var release = new TaskCompletionSource<byte[]>();
            var cache = new ThumbnailDownloadCache(uri => { calls++; return release.Task; }, 4, 1, 1, TimeSpan.FromMinutes(1));
            var uri1 = new Uri("https://fixture.invalid/one.png");
            var uri2 = new Uri("https://fixture.invalid/two.png");
            var one = cache.GetAsync(uri1);
            var duplicate = cache.GetAsync(uri1);
            Check(ReferenceEquals(one, duplicate) && calls == 1, "concurrent same-URI downloads merge");
            Check(await cache.GetAsync(uri2) == null && calls == 1, "pending downloads are bounded");
            release.SetResult(new byte[] { 1, 2, 3 });
            var data = await one;
            Check(ReferenceEquals(await cache.GetAsync(uri1), data) && calls == 1, "sheet bytes shared, not cached per region");
            await cache.GetAsync(uri2);
            await cache.GetAsync(uri1);
            Check(calls == 3, "entry/byte budget evicts old sheet");
            calls = 0;
            cache = new ThumbnailDownloadCache(uri => { calls++; return Task.FromResult(new byte[8]); }, 4, 1, 1, TimeSpan.FromMinutes(1));
            await cache.GetAsync(uri1);
            await cache.GetAsync(uri1);
            Check(calls == 2, "oversize response is not retained");
            calls = 0;
            cache = new ThumbnailDownloadCache(uri => { calls++; throw new Exception("offline failure"); }, 4, 1, 1, TimeSpan.FromMinutes(1));
            Check(await cache.GetAsync(uri1) == null && await cache.GetAsync(uri1) == null && calls == 2, "failed downloads retry without poisoned entries");
        }
    }
}
