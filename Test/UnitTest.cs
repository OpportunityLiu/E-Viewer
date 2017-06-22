using ExClient;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            for (int i = 0; i < 10000000; i++)
            {
                var ig = Enum.IsDefined(typeof(Category), ExClient.Category.All);
            }
        }
        [TestMethod]
        public void TestMethod2()
        {
            for (int i = 0; i < 10000000; i++)
            {
                var ig = ExClient.Category.All.IsDefined();
            }
        }
    }
}
