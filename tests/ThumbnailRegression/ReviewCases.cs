using ExClient.Internal;

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace ThumbnailRegression {
    internal static class ReviewCases {
        [DataContract]
        private sealed class Cases {
            [DataMember] public Case[] valid { get; set; }
            [DataMember] public Case[] reject { get; set; }
        }
        [DataContract]
        private sealed class Case {
            [DataMember] public string name { get; set; }
            [DataMember] public string style { get; set; }
            [DataMember] public string image { get; set; }
            [DataMember] public long[] expectedRegion { get; set; }
        }

        public static void Run(Regression tests, string path) {
            Cases cases;
            using (var file = File.OpenRead(path))
                cases = (Cases)new DataContractJsonSerializer(typeof(Cases)).ReadObject(file);
            foreach (var item in cases.valid) {
                var actual = ThumbnailParser.ParseStyle(item.style);
                if (actual == null) {
                    tests.Check(item.name == "size-reset", "CSS oracle safe rejection of unsupported combination: " + item.name);
                    continue;
                }
                var r = item.expectedRegion;
                var expected = new ThumbnailSource(new Uri(item.image.Substring(5, item.image.Length - 7)),
                    new ThumbnailRegion(r[0], r[1], r[2], r[3]));
                tests.Check(actual.Equals(expected), "CSS oracle exact URI/region: " + item.name);
            }
            foreach (var item in cases.reject)
                tests.Check(ThumbnailParser.ParseStyle(item.style) == null, "CSS oracle rejects: " + item.name);
        }
    }
}
