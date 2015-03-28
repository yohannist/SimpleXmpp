using System;

namespace SimpleXmpp
{
    using SimpleXmpp.Net;
    using SimpleXmpp.Protocol;
    using SimpleXmpp.Readers;
    using System.IO;
    using System.Text;
    using System.Xml;

    public class XmppClient
    {
        public delegate void OnConnectExceptionHandler(Exception exception);
        public delegate void OnConnectedEventHandler();
        /// <summary>
        /// Called when there is an exception thrown while trying to connect to the remote location.
        /// </summary>
        public event OnConnectExceptionHandler OnConnectException;
        public event OnConnectedEventHandler OnConnected;

        private AsyncSocket asyncSocket;
        private AsyncXmppReader asyncXmlReader;
        
        /// <summary>
        /// Gets the hostname set in the constructor
        /// </summary>
        public string Hostname { get; private set; }
        /// <summary>
        /// Gets the port set in the constructor
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// Gets whether a secure connection is used for this connection
        /// </summary>
        public bool UsesSslConnection { get; private set; }

        /// <summary>
        /// Creates an asynchronous client that deals with the xmpp protocol
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="usesSslConnection"></param>
        public XmppClient(string hostname, int port, bool usesSslConnection)
        {
            this.Hostname = hostname;
            this.Port = port;
            this.UsesSslConnection = usesSslConnection;

            // create socket
            this.asyncSocket = new AsyncSocket(this.Hostname, this.Port, this.UsesSslConnection);
            this.asyncSocket.OnConnectException += handleConnectionException;

            // create reader object and bind events
            this.asyncXmlReader = new AsyncXmppReader();
            this.asyncXmlReader.OnXmlDocumentStart += handleXmlDocumentStart;
            this.asyncXmlReader.OnXmlDocumentEnd += handleXmlDocumentEnd;
            this.asyncXmlReader.OnXmlElementComplete += handleNewXmlElement;
            this.asyncXmlReader.OnXmlException += handleXmlException;
        }

        public void BeginConnect()
        {
            this.asyncSocket.BeginConnect(onConnected);
        }

        public void Disconnect()
        {
            this.asyncSocket.Disconnect();
            this.asyncXmlReader.StopReading();
        }

        public void Send(XmppElement xmppDocument)
        {
            // convert document into bytes
            var data = Encoding.UTF8.GetBytes(xmppDocument.ToString());

            // send through our socket
            this.asyncSocket.Send(data);
        }

        /// <summary>
        /// When connected, this method takes the connected stream and waits for incoming data. It expects XML data to be sent.
        /// </summary>
        /// <param name="networkStream">The network stream returned by an open socket</param>
        /// <exception cref="XmlException">Any XML parsing exceptions thrown by XmlReader</exception>
        /// <exception cref="XmlException">When there are more than 1 root element</exception>
        private void onConnected(Stream networkStream)
        {
            // once connected, we can open an XmlReader and asynchronously read the stream
            this.asyncXmlReader.BeginReading(networkStream);
        }

        private void handleXmlDocumentStart(XmppElement root)
        {
            // create xmpp element
            // call document start event
        }

        private void handleNewXmlElement(XmppElement node)
        {
            // create xmpp element
            
        }

        private void handleXmlDocumentEnd(XmppElement root)
        {

        }

        private void handleXmlException(XmlException ex)
        {

        }

        private void handleConnectionException(Exception ex)
        {
            // to do: log something
            // otherwise..nothing to do, really.

            // bubble exception upwards
            if (this.OnConnectException != null)
            {
                this.OnConnectException(ex);
            }
        }
    }
}
