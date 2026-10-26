using System;
using System.Threading.Tasks;

namespace ThumbnailRegression {
    internal static class ConsoleMain {
        public static async Task<int> Main(string[] args) {
            try {
                var tests = new Regression(Console.WriteLine);
                var smoke = Array.IndexOf(args, "--smoke") >= 0;
                await tests.RunAsync(smoke);
                var review = Array.IndexOf(args, "--review-cases");
                if (!smoke && review >= 0)
                    ReviewCases.Run(tests, args[review + 1]);
                Console.WriteLine("PASS: " + tests.Checks + " production-linked checks");
                return 0;
            } catch (Exception error) {
                Console.Error.WriteLine(error);
                return 1;
            }
        }
    }
}
