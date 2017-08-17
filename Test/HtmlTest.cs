using HtmlAgilityPack;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    [TestClass]
    public class HtmlTest
    {
        [TestMethod]
        public void Test()
        {
            var html = new HtmlDocument();
            html.LoadHtml(@"
<html>
<head>
</head>
<body>
    <a id='a' href='http://example.com/index.htm?Q=%E6%B5%8B%E8%AF%95&and=&amp;+&quot;'>&lt;LINK&gt;</a>
</body>
</html>");
            var a = html.GetElementbyId("a");
            var attr = a.GetAttributeValue("href", "");
            var con = a.InnerText;
            var htm = a.OuterHtml;
            Assert.AreEqual("&lt;LINK&gt;", con);
            Assert.AreEqual("<LINK>", HtmlEntity.DeEntitize(con));
            Assert.AreEqual("http://example.com/index.htm?Q=%E6%B5%8B%E8%AF%95&and=&amp;+&quot;", attr);
            Assert.AreEqual("http://example.com/index.htm?Q=%E6%B5%8B%E8%AF%95&and=&+\"", HtmlEntity.DeEntitize(attr));
            Assert.AreEqual("<a id='a' href='http://example.com/index.htm?Q=%E6%B5%8B%E8%AF%95&and=&amp;+&quot;'>&lt;LINK&gt;</a>", htm);
            Assert.AreEqual("<a id='a' href='http://example.com/index.htm?Q=%E6%B5%8B%E8%AF%95&and=&+\"'><LINK></a>", HtmlEntity.DeEntitize(htm));
        }
    }
}
