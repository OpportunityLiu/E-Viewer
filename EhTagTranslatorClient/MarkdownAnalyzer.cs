using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.IO;

namespace EhTagTranslatorClient
{
    internal static class MarkdownAnalyzer
    {
        public static IEnumerable<Record> Analyze(IInputStream stream)
        {
            var s = stream.AsStreamForRead();
            var reader = new StreamReader(stream.AsStreamForRead());
            while(!reader.EndOfStream)
            {
                var r = analyzeLine(reader.ReadLine());
                if(r != null)
                    yield return r;
            }
        }

        private static Record analyzeLine(string line)
        {
             
        }
    }
}
