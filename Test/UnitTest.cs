using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var t = await EhTagTranslatorClient.EhTagDatabase.LoadDatabaseAsync();
        }
    }
}
