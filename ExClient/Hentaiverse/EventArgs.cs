using System;
using System.Collections.Generic;

namespace ExClient.HentaiVerse
{
    public sealed class RandomEncounterEventArgs : EventArgs
    {
        internal RandomEncounterEventArgs(Uri uri)
        {
            Uri = uri;
        }

        public Uri Uri { get; }
    }

    public sealed class DawnOfDayRewardsEventArgs : EventArgs
    {
        internal DawnOfDayRewardsEventArgs(IReadOnlyDictionary<string, double> data)
        {
            Data = data;
        }

        public IReadOnlyDictionary<string, double> Data { get; }
    }
}
