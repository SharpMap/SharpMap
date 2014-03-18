using System;
using System.Drawing;
using NUnit.Framework;

namespace UnitTests.WMS
{
    [TestFixture]
    public class Encoding
    {
        [Test]
        public void TestEncodingWebName()
        {
            var enc = System.Text.Encoding.GetEncoding(1252);
            Assert.AreEqual("Windows-1252", enc.WebName);
            enc = System.Text.Encoding.UTF8;
            Assert.AreEqual("utf-8", enc.WebName);
        }

        [Test]
        public void TestUrlColorEncoding()
        {
            TestUrlColorEncoding(Color.Red);
            TestUrlColorEncoding(Color.FromArgb(Color.Red.ToArgb()));
            TestUrlColorEncoding(Color.FromArgb(127, 87, 205, 93));
        }

        private static void TestUrlColorEncoding(Color color)
        {
            Console.WriteLine("Color           : {0}", color);
            var htmlColor = ColorTranslator.ToHtml(Color.FromArgb(color.ToArgb()));
            Console.WriteLine("htmlColor       : {0}", htmlColor);
            var urlEncode = System.Web.HttpUtility.UrlEncode(htmlColor);
            Console.WriteLine("urlEncode       : {0}", urlEncode);
            var escapeDataString = Uri.EscapeDataString(htmlColor);
            Console.WriteLine("escapeDataString: {0}", escapeDataString);
            Console.WriteLine();

            Assert.IsTrue(string.Compare(urlEncode, escapeDataString, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        
    }
}
