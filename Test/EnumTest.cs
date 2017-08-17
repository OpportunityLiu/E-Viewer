using ExClient;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;

namespace Test
{
    [TestClass]
    public class EnumTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            for (var i = 0; i < 10000000; i++)
            {
                var ig = Enum.IsDefined(typeof(Category), Category.All);
            }
        }
        [TestMethod]
        public void TestMethod1_A()
        {
            var t = typeof(Category);
            for (var i = 0; i < 10000000; i++)
            {
                var ig = Enum.IsDefined(t, Category.All);
            }
        }
        [TestMethod]
        public void TestMethod1_B()
        {
            var v = (object)Category.All;
            for (var i = 0; i < 10000000; i++)
            {
                var ig = Enum.IsDefined(typeof(Category), v);
            }
        }
        [TestMethod]
        public void TestMethod1_AB()
        {
            var t = typeof(Category);
            var v = (object)Category.All;
            for (var i = 0; i < 10000000; i++)
            {
                var ig = Enum.IsDefined(t, v);
            }
        }
        [TestMethod]
        public void TestMethod2()
        {
            for (var i = 0; i < 10000000; i++)
            {
                var ig = Category.All.IsDefined();
            }
        }
    }
}
