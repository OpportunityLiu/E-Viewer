using ExClient.Internal;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.ApplicationModel.Activation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ThumbnailRegression {
    public sealed partial class App : Application {
        private readonly TextBlock status = new TextBlock { Text = "Ready", TextWrapping = TextWrapping.Wrap };
        private readonly TextBlock log = new TextBlock { FontSize = 13, TextWrapping = TextWrapping.Wrap };
        private readonly StackPanel images = new StackPanel { Orientation = Orientation.Horizontal };
        private readonly List<string> lines = new List<string>();
        private readonly Button run = new Button { Content = "Run / reload all checks" };
        private readonly Button local = new Button { Content = "Switch first tile to local file", IsEnabled = false };
        private readonly ThumbnailRegion[] regions = {
            new ThumbnailRegion(0, 0, 64, 80), new ThumbnailRegion(64, 0, 64, 64), new ThumbnailRegion(128, 32, 96, 48)
        };
        private byte[] png;

        public App() { InitializeComponent(); }
        protected override void OnLaunched(LaunchActivatedEventArgs args) {
            var panel = new StackPanel { Margin = new Thickness(24), Spacing = 12 };
            panel.Children.Add(new TextBlock { Text = "#179 — OFFLINE TESTHOST (not full E-Viewer)", FontSize = 24 });
            panel.Children.Add(new TextBlock { Text = "Synthetic red / green / blue pages; local yellow requires a Windows image thumbnail (icon fallback is SKIP).", TextWrapping = TextWrapping.Wrap });
            panel.Children.Add(run);
            panel.Children.Add(local);
            panel.Children.Add(status);
            panel.Children.Add(images);
            panel.Children.Add(log);
            Window.Current.Content = new ScrollViewer { Content = panel };
            run.Click += async (s, e) => await RunAsync();
            local.Click += async (s, e) => await LocalAsync();
            Window.Current.Activate();
        }

        private void Write(string line) { lines.Add(line); log.Text = string.Join("\n", lines); }
        private async Task SaveAsync() => await FileIO.WriteLinesAsync(
            await ApplicationData.Current.LocalFolder.CreateFileAsync("results.txt", CreationCollisionOption.ReplaceExisting), lines);

        private async Task RunAsync() {
            run.IsEnabled = local.IsEnabled = false;
            lines.Clear();
            images.Children.Clear();
            status.Text = "Running production-linked checks...";
            try {
                var tests = new Regression(Write);
                await tests.RunAsync();
                png = await EncodeAsync(BitmapEncoder.PngEncoderId);
                var jpeg = await EncodeAsync(BitmapEncoder.JpegEncoderId);
                await NativeAsync(tests, png, "PNG");
                await NativeAsync(tests, jpeg, "JPEG");
                await WebpAsync(tests);
                status.Text = "PASS: " + tests.Checks + " checks (see explicit WebP status below)";
                Write(status.Text);
                local.IsEnabled = true;
            } catch (Exception error) {
                status.Text = "FAIL: " + error.Message;
                Write(error.ToString());
            } finally {
                await SaveAsync();
                run.IsEnabled = true;
            }
        }

        private async Task NativeAsync(Regression tests, byte[] bytes, string format) {
            using (var input = new MemoryStream(bytes, false).AsRandomAccessStream()) {
                var original = await BitmapDecoder.CreateAsync(input);
                var reference = (await original.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                    new BitmapTransform(), ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage)).DetachPixelData();
                foreach (var region in regions) {
                    using (var output = await ThumbnailBitmap.CreateStreamAsync(bytes, region)) {
                        tests.Check(output != null, format + " production crop stream exists");
                        var decoder = await BitmapDecoder.CreateAsync(output);
                        tests.Check(decoder.PixelWidth == region.Width && decoder.PixelHeight == region.Height, format + " crop dimensions");
                        var actual = (await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                            new BitmapTransform(), ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage)).DetachPixelData();
                        var expected = Enumerable.Range(0, (int)region.Height).SelectMany(y => reference.Skip(
                            (int)(((region.Y + y) * original.PixelWidth + region.X) * 4)).Take((int)region.Width * 4)).ToArray();
                        tests.Check(actual.SequenceEqual(expected), format + " exact source-pixel comparison at " + region.X + "," + region.Y);
                    }
                    var target = new BitmapImage();
                    tests.Check(await ThumbnailBitmap.LoadAsync(bytes, region, target)
                        && target.PixelWidth == region.Width && target.PixelHeight == region.Height, format + " PNG bridge to BitmapImage");
                    if (format == "PNG") images.Children.Add(new Image { Source = target, Width = region.Width, Height = region.Height, Margin = new Thickness(8) });
                }
                var plain = new BitmapImage();
                tests.Check(await ThumbnailBitmap.LoadAsync(bytes, null, plain) && plain.PixelWidth == 224 && plain.PixelHeight == 80,
                    format + " uncropped cover-compatible BitmapImage");
                foreach (var region in new[] { new ThumbnailRegion(-1, 0, 1, 1), new ThumbnailRegion(220, 0, 8, 1), new ThumbnailRegion(0, 79, 1, 2) }) {
                    tests.Check(!await ThumbnailBitmap.LoadAsync(bytes, region, new BitmapImage()), format + " rejects invalid/partial bounds without sheet fallback");
                }
            }
            var concurrent = await Task.WhenAll(regions.Select(async region => {
                var bitmap = new BitmapImage();
                return await ThumbnailBitmap.LoadAsync(bytes, region, bitmap) && bitmap.PixelWidth == region.Width;
            }));
            tests.Check(concurrent.All(pass => pass), format + " concurrent regions have independent seekable streams");
        }

        private async Task WebpAsync(Regression tests) {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/sprite.webp"));
            var bytes = (await FileIO.ReadBufferAsync(file)).ToArray();
            using (var input = new MemoryStream(bytes, false).AsRandomAccessStream()) {
                try { await BitmapDecoder.CreateAsync(input); }
                catch (Exception error) when (error.HResult == unchecked((int)0x88982F8B) || error.HResult == unchecked((int)0x88982F50)) {
                    Write("SKIP WebP: native codec unavailable/initialization failed, HRESULT 0x" + error.HResult.ToString("X8") + "; not a passed test.");
                    return;
                }
            }
            tests.Check(await ThumbnailBitmap.LoadAsync(bytes, new ThumbnailRegion(0, 0, 64, 64), new BitmapImage()), "WebP codec and production BitmapImage path");
        }

        private async Task LocalAsync() {
            run.IsEnabled = local.IsEnabled = false;
            try {
                var tests = new Regression(Write);
                var state = new ThumbnailLoadState<BitmapImage>();
                state.SetSource(new ThumbnailSource(new Uri("https://fixture.invalid/sheet.png"), regions[0]));
                state.TryBegin(out var remote);
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("local-yellow.png", CreationCollisionOption.ReplaceExisting);
                var bytes = await EncodeAsync(BitmapEncoder.PngEncoderId, true);
                await FileIO.WriteBytesAsync(file, bytes);
                using (var input = new MemoryStream(bytes, false).AsRandomAccessStream())
                    tests.Check(await HasYellowPixelsAsync(input), "local PNG fixture contains only yellow pixels");
                state.SetFile(file);
                tests.Check(state.TryBegin(out var request) && ReferenceEquals(request.File, file), "local StorageFile starts before pending remote completes");
                var late = new BitmapImage();
                await ThumbnailBitmap.LoadAsync(png, regions[0], late);
                tests.Check(!state.Complete(remote, late, out var retry) && !retry, "late remote BitmapImage cannot overwrite local source");
                tests.Check(!state.TryBegin(out _), "stale remote completion cannot release current local request");
                var target = new BitmapImage();
                var imageThumbnail = false;
                using (var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 180, ThumbnailOptions.ResizeThumbnail)) {
                    tests.Check(thumbnail != null, "local GetThumbnailAsync returns a stream");
                    Write("INFO local GetThumbnailAsync: Type=" + thumbnail.Type + ", ContentType=" + thumbnail.ContentType + ", Size=" + thumbnail.Size);
                    imageThumbnail = thumbnail.Type == ThumbnailType.Image;
                    if (imageThumbnail) {
                        tests.Check(await HasYellowPixelsAsync(thumbnail), "local Windows image thumbnail contains yellow pixels");
                        thumbnail.Seek(0);
                    } else {
                        Write("SKIP local yellow-image pixels: Windows returned an Icon thumbnail, not image content; local/state publication checks are separate.");
                    }
                    await target.SetSourceAsync(thumbnail);
                }
                tests.Check(state.Complete(request, target, out _) && state.TryGet(out var current) && ReferenceEquals(current, target), "local GetThumbnailAsync result published (not an image-content assertion)");
                ((Image)images.Children[0]).Source = target;
                status.Text = "PASS: " + tests.Checks + " local/state checks; " + (imageThumbnail ? "yellow image verified." : "SKIP local image pixels: Windows returned Icon.") + " Reload restores RGB.";
                Write(status.Text);
            } catch (Exception error) {
                status.Text = "FAIL: " + error.Message;
                Write(error.ToString());
            } finally {
                await SaveAsync();
                run.IsEnabled = local.IsEnabled = true;
            }
        }

        private static async Task<bool> HasYellowPixelsAsync(IRandomAccessStream stream) {
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var pixels = (await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                new BitmapTransform(), ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage)).DetachPixelData();
            return pixels.Length != 0 && Enumerable.Range(0, pixels.Length / 4).All(i =>
                Math.Abs(pixels[i * 4] - 20) <= 12 && Math.Abs(pixels[i * 4 + 1] - 220) <= 12
                && Math.Abs(pixels[i * 4 + 2] - 230) <= 12 && pixels[i * 4 + 3] == 255);
        }

        private static async Task<byte[]> EncodeAsync(Guid encoderId, bool local = false) {
            uint width = local ? 64u : 224u, height = 80;
            var pixels = new byte[width * height * 4];
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++) {
                    var i = (y * width + x) * 4;
                    pixels[i] = !local && x >= 128 ? (byte)230 : (byte)20;
                    pixels[i + 1] = local || (x >= 64 && x < 128) ? (byte)220 : (byte)20;
                    pixels[i + 2] = local || x < 64 ? (byte)230 : (byte)20;
                    pixels[i + 3] = 255;
                }
            using (var output = new InMemoryRandomAccessStream()) {
                var encoder = await BitmapEncoder.CreateAsync(encoderId, output);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, width, height, 96, 96, pixels);
                await encoder.FlushAsync();
                output.Seek(0);
                var result = new byte[(int)output.Size];
                using (var reader = new DataReader(output)) { await reader.LoadAsync((uint)result.Length); reader.ReadBytes(result); }
                return result;
            }
        }
    }
}
