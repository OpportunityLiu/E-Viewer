using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Threading.Tasks;

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
