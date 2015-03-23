using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleXmpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.SimpleXmpp
{
    [TestClass]
    public class XmppClientTests
    {
        /// <summary>
        /// constructor - hostname, port, ssl saved
        /// </summary>
        [TestMethod]
        public void Constructor_ValuesSaved()
        {
            // arrange
            string hostname = "some.hostname";
            int port = 435;
            bool isSsl = true;

            // act
            var client = new XmppClient(hostname, port, isSsl);

            // assert
            Assert.AreEqual(hostname, client.Hostname, "hostname is not saved properly");
            Assert.AreEqual(port, client.Port, "port is not saved properly");
            Assert.AreEqual(isSsl, client.UsesSslConnection, "ssl is not saved properly");
        }
    }
}
