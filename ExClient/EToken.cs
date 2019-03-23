using System;

namespace ExClient
{
    public readonly struct EToken : IEquatable<EToken>
    {
        internal const int TOKEN_BYTE_LENGTH = 5;
        internal const int TOKEN_STR_LENGTH = TOKEN_BYTE_LENGTH * 2; // 2 hex chars for 1 byte
        private const ulong TOKEN_MAX_VALUE = (1UL << (TOKEN_BYTE_LENGTH * 8)) - 1;

        private static readonly string _TokenStringFormat = "x" + TOKEN_STR_LENGTH;

        public ulong Value { get; }

        public EToken(ulong value)
        {
            if (value > TOKEN_MAX_VALUE)
                throw new ArgumentOutOfRangeException(nameof(value));
            Value = value;
        }

        public static bool TryParse(string str, out EToken value)
        {
            if (string.IsNullOrWhiteSpace(str))
                goto FAIL;
            if(!ulong.TryParse(str, System.Globalization.NumberStyles.HexNumber, null, out var v))
                goto FAIL;
            if(v > TOKEN_MAX_VALUE)
                goto FAIL;
            value = new EToken(v);
            return true;

            FAIL:
            value = default;
            return false;
        }

        public static EToken Parse(string str)
        {
            if (str is null)
                throw new ArgumentNullException(nameof(str));
            if (!TryParse(str, out var r))
                throw new FormatException($"`{str}` is wrong format for EToken.");
            return r;
        }

        public override string ToString() => Value.ToString(_TokenStringFormat);

        public static bool operator ==(EToken t1, EToken t2) => Equals(t1, t2);
        public static bool operator !=(EToken t1, EToken t2) => !Equals(t1, t2);

        public static bool Equals(EToken t1, EToken t2) => t1.Value == t2.Value;

        public bool Equals(EToken other) => Equals(this, other); 

        public override bool Equals(object obj) => obj is EToken o && Equals(o);

        public override int GetHashCode() => Value.GetHashCode();
    }
}
