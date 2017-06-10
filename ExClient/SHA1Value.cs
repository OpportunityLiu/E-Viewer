using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ExClient
{
    public struct SHA1Value : IEquatable<SHA1Value>, IFormattable
    {
        private const int HASH_SIZE = 20;
        private static readonly HashAlgorithmProvider sha1compute = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);

        public static SHA1Value Compute(byte[] data)
        {
            return new SHA1Value(sha1compute.HashData(data.AsBuffer()));
        }

        public static SHA1Value Compute(IBuffer data)
        {
            return new SHA1Value(sha1compute.HashData(data));
        }

        public static IAsyncOperation<SHA1Value> ComputeAsync(IRandomAccessStream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            return Task.Run(async () =>
            {
                stream.Seek(0);
                var buf = new Windows.Storage.Streams.Buffer((uint)stream.Size);
                await stream.ReadAsync(buf, (uint)stream.Size, InputStreamOptions.None);
                return new SHA1Value(sha1compute.HashData(buf));
            }).AsAsyncOperation();
        }

        public static IAsyncOperation<SHA1Value> ComputeAsync(IRandomAccessStreamReference stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            return AsyncInfo.Run(async token =>
            {
                var st = stream.OpenReadAsync();
                token.Register(st.Cancel);
                using (var s = await st)
                    return await ComputeAsync(s);
            });
        }

        public static SHA1Value Parse(string imageHash)
        {
            return new SHA1Value(CryptographicBuffer.DecodeFromHexString(imageHash));
        }

        private readonly ulong dataLow, dataHigh;
        private readonly uint dataMiddle;

        public byte[] Data
        {
            get
            {
                var r = new byte[HASH_SIZE];
                var dl = BitConverter.GetBytes(this.dataLow);
                var dm = BitConverter.GetBytes(this.dataMiddle);
                var dh = BitConverter.GetBytes(this.dataHigh);
                if (BitConverter.IsLittleEndian)
                {
                    System.Buffer.BlockCopy(dl, 0, r, 0, 8);
                    System.Buffer.BlockCopy(dm, 0, r, 8, 4);
                    System.Buffer.BlockCopy(dh, 0, r, 12, 8);
                }
                else
                {
                    System.Buffer.BlockCopy(dh, 0, r, 0, 8);
                    System.Buffer.BlockCopy(dm, 0, r, 8, 4);
                    System.Buffer.BlockCopy(dl, 0, r, 12, 8);
                }
                return r;
            }
        }

        public SHA1Value(IBuffer values)
            : this(values.ToArray()) { }

        public SHA1Value(byte[] values)
        {
            if (BitConverter.IsLittleEndian)
            {
                this.dataLow = BitConverter.ToUInt64(values, 0);
                this.dataMiddle = BitConverter.ToUInt32(values, 8);
                this.dataHigh = BitConverter.ToUInt64(values, 12);
            }
            else
            {
                this.dataHigh = BitConverter.ToUInt64(values, 0);
                this.dataMiddle = BitConverter.ToUInt32(values, 8);
                this.dataLow = BitConverter.ToUInt64(values, 12);
            }
        }

        public bool Equals(SHA1Value other)
        {
            return this.dataHigh == other.dataHigh
                && this.dataLow == other.dataLow
                && this.dataMiddle == other.dataMiddle;
        }

        public override bool Equals(object obj)
        {
            if (obj is SHA1Value sha)
                return this.Equals(sha);
            return false;
        }

        public override int GetHashCode() => this.dataHigh.GetHashCode();

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var r = $"{this.dataHigh:x16}{this.dataMiddle:x8}{this.dataLow:x16}";
            if (format != null && format.StartsWith("X"))
                return r.ToUpperInvariant();
            return r;
        }

        public override string ToString()
        {
            return this.ToString(null, null);
        }
    }
}
