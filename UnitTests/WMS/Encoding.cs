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
            System.Diagnostics.Trace.WriteLine($"Color           : {color}");
            var htmlColor = ColorTranslator.ToHtml(Color.FromArgb(color.ToArgb()));
            System.Diagnostics.Trace.WriteLine($"htmlColor       : {htmlColor}");
            var urlEncode = System.Web.HttpUtility.UrlEncode(htmlColor);
            System.Diagnostics.Trace.WriteLine($"urlEncode       : {urlEncode}");
            var escapeDataString = Uri.EscapeDataString(htmlColor);
            System.Diagnostics.Trace.WriteLine($"escapeDataString: {escapeDataString}");
            System.Diagnostics.Trace.WriteLine("");

            Assert.IsTrue(string.Compare(urlEncode, escapeDataString, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        
    }
}
