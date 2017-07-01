using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Galleries.Rating
{
    public enum Score : byte
    {
        Score_0_5 = 1,
        Score_1_0 = 2,
        Score_1_5 = 3,
        Score_2_0 = 4,
        Score_2_5 = 5,
        Score_3_0 = 6,
        Score_3_5 = 7,
        Score_4_0 = 8,
        Score_4_5 = 9,
        Score_5_0 = 10
    }

    public static class ScoreExtension
    {
        public static double ToDouble(this Score score)
        {
            return (byte)score / 2.0;
        }
        public static float ToFloat(this Score score)
        {
            return (byte)score / 2.0f;
        }
        public static decimal ToDecimal(this Score score)
        {
            return decimal.Divide((byte)score, 2);
        }

        public static Score ToScore(this double value)
        {
            var score = (Score)(byte)Math.Round(value * 2);
            if (!score.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(value));
            return score;
        }
        public static Score ToScore(this float value)
        {
            var score = (Score)(byte)Math.Round(value * 2);
            if (!score.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(value));
            return score;
        }
        public static Score ToScore(this decimal value)
        {
            var score = (Score)(byte)Math.Round(value * 2);
            if (!score.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(value));
            return score;
        }
    }
}
