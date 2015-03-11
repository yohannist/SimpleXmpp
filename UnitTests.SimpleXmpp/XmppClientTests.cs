using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleXmpp;
using Microsoft.QualityTools.Testing.Fakes;
using SimpleXmpp.Net.Fakes;
using System.IO;
using SimpleXmpp.Fakes;
using System.Threading;
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

        /// <summary>
        /// complete xml
        /// ondocumentstart & ondocumentend called only once
        /// onelement called as much as elements count
        /// </summary>
        [TestMethod]
        public void CompleteXml_ValidEventsCalled()
        {
            using (ShimsContext.Create())
            {
                // arrange
                string hostname = "some.hostname";
                int port = 435;
                bool isSsl = true;
                var client = new XmppClient(hostname, port, isSsl);
                
                string markup = 
                    @"<ns:Root xmlns:ns=""Random"" ns:Key=""55"">
                      <ns:Child ns:Key=""01"">
                        <GrandChild><![CDATA[ccc]]></GrandChild>
                      </ns:Child>
                      <Child Key=""02"">
                        <GrandChild>bbb</GrandChild>
                      </Child>
                      <Child Key=""03""></Child>
                    </ns:Root>";

                ShimAsyncSocket.AllInstances.BeginConnectAsyncSocketOnConnectedEventHander = (self, onConnectedHandler) =>
                    {
                        // simulate async operation by socket.BeginConnect
                        Task.Run(() =>
                        {
                            // create fake stream to feed back into xmpp client 
                            var stream = new MemoryStream();
                            var writer = new StreamWriter(stream);
                            writer.Write(markup);
                            writer.Flush();
                            stream.Position = 0;
                            onConnectedHandler(stream);
                        });
                    };

                bool isDocumentStartCalled = false;
                bool isDocumentEndCalled = false;
                int elementCount = 0;
                client.OnXDocumentStart += (node) =>
                    {
                        isDocumentStartCalled = true;
                    };
                client.OnXElementComplete += (node) =>
                    {
                        elementCount++;
                    };
                client.OnXDocumentEnd += (node) =>
                    {
                        isDocumentEndCalled = true;
                    };

                // act
                client.BeginConnect();
                // wait 1 sec
                Thread.Sleep(1000);
                // disconnect
                client.Disconnect();

                // assert
                Assert.IsTrue(isDocumentStartCalled, "document start not called");
                Assert.IsTrue(isDocumentEndCalled, "document end not called");
                Assert.AreEqual(6, elementCount, "element count is not the same");
            }
        }

        // namespace declared in root, referenced in root
        // namespace declared in root, referenced in child
        // namespace declared in child, referenced in root
        // namespace declared in child, referenced in child's child
        // undefined namespace - exception
        // incomplete tag - exception
        // multiple root - exception

        // incomplete xml - ondocumentend not called

        // disconnect - asyncsocket.disconnect called
        // disconnect - infinite loop exits
        // disconnect - reader.ReadAsync gets flushed bytes
        // disconnect - catch exceptions?
    }
}
