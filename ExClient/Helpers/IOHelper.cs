using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Storage.Streams;

namespace Windows.Storage
{
    public static class IOHelper
    {
        public static IRandomAccessStream AsRandomAccessStream(this IBuffer buffer)
        {
            return buffer.AsStream().AsRandomAccessStream();
        }

        public static IRandomAccessStream AsRandomAccessStream(this byte[] data)
        {
            return data.AsBuffer().AsRandomAccessStream();
        }
    }
}
