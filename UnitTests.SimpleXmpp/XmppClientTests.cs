using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleXmpp;
using Microsoft.QualityTools.Testing.Fakes;
using SimpleXmpp.Net.Fakes;
using System.IO;
using SimpleXmpp.Fakes;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

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
        /// namespace declared in root, referenced in root
        /// namespace declared in root, referenced in child
        /// namespace declared in child, referenced in child's child
        /// </summary>
        [TestMethod]
        public void CompleteXml_ValidEventsCalled()
        {
            using (ShimsContext.Create())
            {
                // arrange
                string markup =
                    @"<ns:Root xmlns:ns=""Random"" ns:Key=""55"">
                      <ns:Child ns:Key=""01"">
                        <GrandChild><![CDATA[ccc]]></GrandChild>
                      </ns:Child>
                      <ds:Child xmlns:ds=""Random"" ds:Key=""02"">
                        <ds:GrandChild>bbb</ds:GrandChild>
                      </ds:Child>
                      <Child Key=""03""></Child>
                    </ns:Root>";

                // use object to preserve reference
                // ref/out parameters doesn't play well with lamba/anon functions
                var parsingStates = new XmlParsingStates();
                var client = arrangeXml(markup, parsingStates);

                // act
                actConnectSleepDisconnect(client);

                // assert
                Assert.IsTrue(parsingStates.StartCalled, "document start not called");
                Assert.IsTrue(parsingStates.EndCalled, "document end not called");
                Assert.AreEqual(6, parsingStates.ElementCount, "element count is not the same");
            }
        }

        /// <summary>
        /// incomplete xml
        /// namespace declared in child, referenced in root
        /// undefined namespace - exception
        /// incomplete xml - xmlexception raised
        /// incomplete xml - ondocumentend not called
        /// </summary>
        [TestMethod]
        public void InvalidXml_ValidEventsCalled()
        {
            using (ShimsContext.Create())
            {
                // arrange
                string markup =
                    @"<Root ns:Key=""55"">
                      <Child xmlns:ns=""Random"" Key=""01"">
                        <GrandChild><![CDATA[ccc]]></GrandChild>
                      </Child>
                      <Child Key=""02"">
                        <GrandChild>bbb</GrandChild>
                      </Child>
                      <Child Key=""03""></Child>
                    </Root>";

                // use object to preserve reference
                // ref/out parameters doesn't play well with lamba/anon functions
                var parsingStates = new XmlParsingStates();
                var client = arrangeXml(markup, parsingStates);

                // act
                actConnectSleepDisconnect(client);

                // assert
                Assert.IsTrue(parsingStates.XmlExceptionRaised, "xml exception not raised");
                Assert.IsFalse(parsingStates.EndCalled, "document end is called");
            }
        }
        
        /// <summary>
        /// incomplete xml
        /// multiple root - exception
        /// incomplete xml - xmlexception raised
        /// incomplete xml - ondocumentend not called
        /// </summary>
        [TestMethod]
        public void MultipleRootXml_ValidEventsCalled()
        {
            using (ShimsContext.Create())
            {
                // arrange
                string markup =
                    @"<Root1>
                        </Root1>
                        <Root2>
                        </Root2>";

                // use object to preserve reference
                // ref/out parameters doesn't play well with lamba/anon functions
                var parsingStates = new XmlParsingStates();
                var client = arrangeXml(markup, parsingStates);

                // act
                actConnectSleepDisconnect(client);

                // assert
                Assert.IsTrue(parsingStates.XmlExceptionRaised, "xml exception not raised");
                Assert.IsTrue(parsingStates.EndCalled, "document end is not called");
            }
        }

        /// <summary>
        /// incomplete xml
        /// incomplete tag - exception
        /// incomplete xml - xmlexception raised
        /// incomplete xml - ondocumentend not called
        /// </summary>
        [TestMethod]
        public void IncompleteXml_ValidEventsCalled()
        {
            using (ShimsContext.Create())
            {
                // arrange
                string markup =
                    @"<Root ns:Key=""55"">";

                // use object to preserve reference
                // ref/out parameters doesn't play well with lamba/anon functions
                var parsingStates = new XmlParsingStates();
                var client = arrangeXml(markup, parsingStates);

                // act
                actConnectSleepDisconnect(client);

                // assert
                Assert.IsTrue(parsingStates.XmlExceptionRaised, "xml exception not raised");
                Assert.IsFalse(parsingStates.EndCalled, "document end is called");
            }
        }

        private XmppClient arrangeXml(string markup, XmlParsingStates parseStates)
        {
            string hostname = "some.hostname";
            int port = 435;
            bool isSsl = true;
            var client = new XmppClient(hostname, port, isSsl);

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

            client.OnXDocumentStart += (node) =>
            {
                parseStates.StartCalled = true;
            };
            client.OnXElementComplete += (node) =>
            {
                parseStates.ElementCount++;
            };
            client.OnXDocumentEnd += (node) =>
            {
                parseStates.EndCalled = true;
            };
            client.OnXmlException += (exception) =>
            {
                parseStates.XmlExceptionRaised = exception != null;
            };

            return client;
        }

        private void actConnectSleepDisconnect(XmppClient client)
        {
            // connect
            client.BeginConnect();
            // wait 1 sec for background thread to process
            Thread.Sleep(1000);
            // disconnect
            client.Disconnect();
        }

        private class XmlParsingStates
        {
            public bool StartCalled = false;
            public bool EndCalled = false;
            public int ElementCount = 0;
            public bool XmlExceptionRaised = false;
        }
        
        // onconnection exception ?

        // disconnect - asyncsocket.disconnect called
        // disconnect - infinite loop exits
        // disconnect - reader.ReadAsync gets flushed bytes
        // disconnect - catch exceptions?

        // on client disconnect ?
    }
}
