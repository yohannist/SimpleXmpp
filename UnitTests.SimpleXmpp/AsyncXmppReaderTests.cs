using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleXmpp;
using SimpleXmpp.Fakes;
using SimpleXmpp.Net.Fakes;
using SimpleXmpp.Protocol;
using SimpleXmpp.Protocol.Sasl;
using SimpleXmpp.Protocol.Sasl.Fakes;
using SimpleXmpp.Protocol.stream;
using SimpleXmpp.Protocol.stream.Fakes;
using SimpleXmpp.Readers;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace UnitTests.SimpleXmpp
{
    [TestClass]
    public class AsyncXmppReaderTests
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            // warm up the XmppElementFactory (initialize lazy loading etc)
            XmppElement element;
            XmppElementFactory.TryCreateXmppElement("", "", out element);
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
                    @"<Root xmlns=""defaultns"" xmlns:ns=""Random"" ns:Key=""55"">
                      <ns:Child ns:Key=""01"">
                        <GrandChild><![CDATA[ccc]]></GrandChild>
                      </ns:Child>
                      <ds:Child xmlns:ds=""Random"" ds:Key=""02"">
                        <ds:GrandChild>bbb</ds:GrandChild>
                      </ds:Child>
                      <Child Key=""03""></Child>
                    </Root>";

                // use object to preserve reference
                // ref/out parameters doesn't play well with lamba/anon functions
                var parsingStates = new XmlParsingStates();
                var reader = arrangeXmppReader(parsingStates);
                var stream = arrangeMarkupStream(markup);

                // act
                actConnectSleepDisconnect(reader, stream, parsingStates.WaitHandle);

                // assert
                Assert.IsTrue(parsingStates.StartCalled, "document start not called");
                Assert.IsTrue(parsingStates.EndCalled, "document end not called");
                Assert.AreEqual(6, parsingStates.ElementCount, "element count is not the same");
            }
        }

        /// <summary>
        /// complete xml
        /// ondocumentstart & ondocumentend called only once
        /// onelement called as much as elements count
        /// create XmppElement referenced in same assembly
        /// create Xmppelement reference in different assembly
        /// </summary>
        [TestMethod]
        public void CompleteXml_DerivedXmppElements()
        {
            using (ShimsContext.Create())
            {
                // arrange
                string markup =
                    @"<str:features xmlns:str=""http://etherx.jabber.org/streams"">
                      <mechanisms xmlns=""urn:ietf:params:xml:ns:xmpp-sasl"">
                        <mechanism>X-OAUTH2</mechanism>
                        <mechanism>X-GOOGLE-TOKEN</mechanism>
                        <mechanism>PLAIN</mechanism>
                      </mechanisms>
                    </str:features>";

                // use object to preserve reference
                // ref/out parameters doesn't play well with lamba/anon functions
                var parsingStates = new XmlParsingStates();
                var reader = arrangeXmppReader(parsingStates);
                var stream = arrangeMarkupStream(markup);

                var featuresConstructorCalled = false;
                var mechanismsConstructorCalled = false;
                var mechanismConstructorCalledTimes = 0;
                reader.OnXmlElementComplete += (node) => 
                {
                    if (node is Features)
                    {
                        featuresConstructorCalled = true;
                    }
                    else if (node is Mechanisms)
                    {
                        mechanismsConstructorCalled = true;
                    }
                    else if (node is Mechanism)
                    {
                        mechanismConstructorCalledTimes++;
                    }
                };

                // act
                actConnectSleepDisconnect(reader, stream, parsingStates.WaitHandle);

                // assert
                Assert.IsTrue(parsingStates.StartCalled, "document start not called");
                Assert.IsTrue(parsingStates.EndCalled, "document end not called");
                Assert.AreEqual(5, parsingStates.ElementCount, "element count is not the same");
                Assert.IsTrue(featuresConstructorCalled, "features constructor not called");
                Assert.IsTrue(mechanismsConstructorCalled, "mechanisms constructor not called");
                Assert.AreEqual(3, mechanismConstructorCalledTimes, "mechanism constructor not called 3 times");
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
                var reader = arrangeXmppReader(parsingStates);
                var stream = arrangeMarkupStream(markup);

                // act
                actConnectSleepDisconnect(reader, stream, parsingStates.WaitHandle);

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
                var reader = arrangeXmppReader(parsingStates);
                var stream = arrangeMarkupStream(markup);

                // act
                actConnectSleepDisconnect(reader, stream, parsingStates.WaitHandle);

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
                var reader = arrangeXmppReader(parsingStates);
                var stream = arrangeMarkupStream(markup);

                // act
                actConnectSleepDisconnect(reader, stream, parsingStates.WaitHandle);

                // assert
                Assert.IsTrue(parsingStates.XmlExceptionRaised, "xml exception not raised");
                Assert.IsFalse(parsingStates.EndCalled, "document end is called");
            }
        }

        private byte[] arrangeMarkupStream(string markup)
        {
            // create fake buffer to feed back into xmpp client 
            return Encoding.UTF8.GetBytes(markup);
        }

        private AsyncXmppReader arrangeXmppReader(XmlParsingStates parseStates)
        {
            var reader = new AsyncXmppReader();

            // set wait handle when any of these has run
            reader.OnXmlDocumentStart += (node) =>
            {
                parseStates.WaitHandle.Set();
                parseStates.StartCalled = true;
            };
            reader.OnXmlElementComplete += (node) =>
            {
                parseStates.WaitHandle.Set();
                parseStates.ElementCount++;
            };
            reader.OnXmlDocumentEnd += (node) =>
            {
                parseStates.WaitHandle.Set();
                parseStates.EndCalled = true;
            };
            reader.OnXmlException += (exception) =>
            {
                parseStates.WaitHandle.Set();
                parseStates.XmlExceptionRaised = exception != null;
                parseStates.XmlExceptionMessage = exception != null ? exception.Message : "";
            };

            return reader;
        }

        private void actConnectSleepDisconnect(AsyncXmppReader reader, byte[] buffer, WaitHandle waitHandle)
        {
            // connect
            reader.ParseXmppElements(buffer, buffer.Length);

            // wait up to a maximum of 5 sec for background thread to process
            // this merely indicates that the process has started
            waitHandle.WaitOne(5000);
            
            if (System.Diagnostics.Debugger.IsAttached) 
            {
                // wait for a long time in debugging mode so there's time to debug
                Thread.Sleep(1000000);
            }
            else
            {
                // wait another 1 sec to make sure the process has completed
                Thread.Sleep(1000);
            }
        }

        private class XmlParsingStates
        {
            public ManualResetEvent WaitHandle = new ManualResetEvent(false);
            public bool StartCalled = false;
            public bool EndCalled = false;
            public int ElementCount = 0;
            public bool XmlExceptionRaised = false;
            public string XmlExceptionMessage = "";
        }
        
        // onconnection exception ?

        // disconnect - asyncsocket.disconnect called
        // disconnect - infinite loop exits
        // disconnect - reader.ReadAsync gets flushed bytes
        // disconnect - catch exceptions?

        // on client disconnect ?
    }
}
