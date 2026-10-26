using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;

using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace ExClient.Internal {
    internal static class ThumbnailBitmap {
        private const ulong MaxPixels = 16 * 1024 * 1024;
        private const uint MaxDimension = 32768;
        private static readonly SemaphoreSlim Decoders = new SemaphoreSlim(2);

        internal static async Task<bool> LoadAsync(byte[] bytes, ThumbnailRegion? region, BitmapImage target) {
            try {
                using (var output = await CreateStreamAsync(bytes, region)) {
                    if (output == null) return false;
                    await target.SetSourceAsync(output);
                    return true;
                }
            } catch (Exception) {
                return false;
            }
        }

        internal static async Task<IRandomAccessStream> CreateStreamAsync(byte[] bytes, ThumbnailRegion? region) {
            if (bytes == null || bytes.Length > 16 * 1024 * 1024) return null;
            await Decoders.WaitAsync();
            try {
                using (var input = new MemoryStream(bytes, false).AsRandomAccessStream()) {
                    var decoder = await BitmapDecoder.CreateAsync(input);
                    var width = decoder.PixelWidth;
                    var height = decoder.PixelHeight;
                    if (width == 0 || height == 0 || width > MaxDimension || height > MaxDimension
                        || (ulong)width * height > MaxPixels)
                        return null;
                    if (!region.HasValue) {
                        return new MemoryStream(bytes, false).AsRandomAccessStream();
                    }
                    var crop = region.Value;
                    if (!crop.Fits(width, height)) return null;
                    var transform = new BitmapTransform {
                        Bounds = new BitmapBounds {
                            X = (uint)crop.X, Y = (uint)crop.Y, Width = (uint)crop.Width, Height = (uint)crop.Height
                        }
                    };
                    using (var bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Premultiplied, transform, ExifOrientationMode.IgnoreExifOrientation,
                        ColorManagementMode.DoNotColorManage)) {
                        var output = new InMemoryRandomAccessStream();
                        try {
                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, output);
                            encoder.SetSoftwareBitmap(bitmap);
                            await encoder.FlushAsync();
                            output.Seek(0);
                            return output;
                        } catch {
                            output.Dispose();
                            throw;
                        }
                    }
                }
            } finally {
                Decoders.Release();
            }
        }
    }
}
