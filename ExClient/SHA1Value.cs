using ExClient.Internal;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace ExClient
{
    public readonly struct SHA1Value : IEquatable<SHA1Value>, IFormattable
    {
        private const int HASH_SIZE = 20;
        private static readonly HashAlgorithmProvider _Sha1Compute = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);

        public static SHA1Value Compute(byte[] data)
        {
            return new SHA1Value(_Sha1Compute.HashData(data.AsBuffer()));
        }

        public static SHA1Value Compute(IBuffer data)
        {
            return new SHA1Value(_Sha1Compute.HashData(data));
        }

        public static IAsyncOperation<SHA1Value> ComputeAsync(IRandomAccessStream stream)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            return Task.Run(async () =>
            {
                stream.Seek(0);
                var buf = new Windows.Storage.Streams.Buffer((uint)stream.Size);
                await stream.ReadAsync(buf, (uint)stream.Size, InputStreamOptions.None);
                return new SHA1Value(_Sha1Compute.HashData(buf));
            }).AsAsyncOperation();
        }

        public static IAsyncOperation<SHA1Value> ComputeAsync(IRandomAccessStreamReference stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return AsyncInfo.Run(async token =>
            {
                var st = stream.OpenReadAsync();
                token.Register(st.Cancel);
                using (var s = await st)
                {
                    return await ComputeAsync(s);
                }
            });
        }

        public static SHA1Value Parse(string imageHash)
        {
            return new SHA1Value(CryptographicBuffer.DecodeFromHexString(imageHash));
        }

        private unsafe struct DataPack : IEquatable<DataPack>
        {
            public fixed byte Data[HASH_SIZE];

            public unsafe bool Equals(DataPack other)
            {
                fixed (byte* pThis = Data)
                {
                    var pOther = other.Data;
                    for (var i = 0; i < HASH_SIZE; i++)
                    {
                        if (pThis[i] != pOther[i])
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        private readonly DataPack data;

        public unsafe byte[] Data
        {
            get
            {
                var values = new byte[HASH_SIZE];
                fixed (void* pThis = data.Data, pValue = &values[0])
                {
                    System.Buffer.MemoryCopy(pThis, pValue, HASH_SIZE, HASH_SIZE);
                }

                return values;
            }
        }

        public EToken ToToken()
        {
            var data = Data;
            return new EToken(
                (ulong)data[0] << 32 |
                (ulong)data[1] << 24 |
                (ulong)data[2] << 16 |
                (ulong)data[3] << 8 |
                (ulong)data[4] << 0);
        }

        public SHA1Value(IBuffer values)
            : this(values.ToArray()) { }

        public unsafe SHA1Value(byte[] values)
        {
            if ((values ?? throw new ArgumentNullException(nameof(values))).Length != HASH_SIZE)
            {
                throw new ArgumentException($"Length must be {HASH_SIZE}.", nameof(values));
            }

            fixed (void* pThis = &this, pValue = &values[0])
            {
                System.Buffer.MemoryCopy(pValue, pThis, HASH_SIZE, HASH_SIZE);
            }
        }

        public bool Equals(SHA1Value other)
        {
            return data.Equals(other.data);
        }

        public override bool Equals(object obj)
        {
            if (obj is SHA1Value sha)
            {
                return Equals(sha);
            }
            return false;
        }

        public override unsafe int GetHashCode()
        {
            var r = 0;
            fixed (void* p = &this)
            {
                var bytes = (byte*)p;
                for (var i = 0; i < sizeof(int); i++)
                {
                    r |= bytes[i];
                    r <<= 8;
                }
            }
            return r;
        }

        public unsafe string ToString(string format) => ToString(format, null);

        public unsafe string ToString(string format, IFormatProvider formatProvider)
        {
            var fmt = string.IsNullOrEmpty(format) ? 'x' : format[0];
            switch (fmt)
            {
            case 'x':
                return _ToStringL(HASH_SIZE);
            case 'X':
                return _ToStringU(HASH_SIZE);
            case 't':
                return _ToStringL(EToken.TOKEN_BYTE_LENGTH);
            case 'T':
                return _ToStringU(EToken.TOKEN_BYTE_LENGTH);
            default:
                throw new FormatException("Unknown format specifier.");
            }
        }

        private unsafe string _ToStringL(int length)
        {
            var str = stackalloc char[length * 2];
            fixed (void* p = &this)
            {
                var data = (byte*)p;
                var pStr = str;
                for (var i = 0; i < length; i++)
                {
                    var b = data[i];
                    _GetHexValueL(pStr++, b >> 4);
                    _GetHexValueL(pStr++, b & 0xF);
                }
            }
            return new string(str, 0, length * 2);
        }

        private unsafe string _ToStringU(int length)
        {
            var str = stackalloc char[length * 2];
            fixed (void* p = &this)
            {
                var data = (byte*)p;
                var pStr = str;
                for (var i = 0; i < length; i++)
                {
                    var b = data[i];
                    _GetHexValueU(pStr++, b >> 4);
                    _GetHexValueU(pStr++, b & 0xF);
                }
            }
            return new string(str, 0, length * 2);
        }

        private static unsafe void _GetHexValueL(char* p, int i)
        {
            if (i < 10)
            {
                *p = (char)(i + '0');
            }
            else
            {
                *p = (char)(i - 10 + 'a');
            }
        }

        private static unsafe void _GetHexValueU(char* p, int i)
        {
            if (i < 10)
            {
                *p = (char)(i + '0');
            }
            else
            {
                *p = (char)(i - 10 + 'A');
            }
        }

        public override string ToString() => _ToStringL(HASH_SIZE);

        public static bool operator ==(SHA1Value left, SHA1Value right)
            => left.Equals(right);
        public static bool operator !=(SHA1Value left, SHA1Value right)
            => !left.Equals(right);
    }
}
