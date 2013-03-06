using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        
    }
}
