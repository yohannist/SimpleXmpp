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
        public delegate void OnConnectExceptionHandler(Exception ex);
        public delegate void OnConnectedEventHandler();
        public delegate void OnSocketUnexpectedlyClosedHandler(IOException ex);
        public delegate void OnXmppElementEventHandler(XmppElement element);
        public delegate void OnXmlExceptionHandler(XmlException ex);
        /// <summary>
        /// Called when there is an exception thrown while trying to connect to the remote location.
        /// </summary>
        public event OnConnectExceptionHandler OnConnectException;
        public event OnConnectedEventHandler OnConnected;
        public event OnSocketUnexpectedlyClosedHandler OnSocketUnexpectedlyClosed;
        public event OnXmppElementEventHandler OnNewDocument;
        public event OnXmppElementEventHandler OnNewElement;
        public event OnXmppElementEventHandler OnDocumentComplete;
        public event OnXmlExceptionHandler OnXmlException;

        private AsyncSocket asyncSocket;
        private AsyncXmppReader asyncXmppReader;
        private XmlDocument receivedDocument;
        
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
            this.asyncSocket.OnConnected += onConnected;
            this.asyncSocket.OnConnectException += onConnectionException;
            this.asyncSocket.OnDataReceived += onDataReceived;
            this.asyncSocket.OnSocketUnexpectedClosed += onSocketUnexpectedlyClosed;

            // init received document
            receivedDocument = new XmlDocument();

            // create reader object and bind events
            this.asyncXmppReader = new AsyncXmppReader(receivedDocument);
            this.asyncXmppReader.OnXmlDocumentStart += onXmppDocumentStart;
            this.asyncXmppReader.OnXmlDocumentEnd += onXmppDocumentEnd;
            this.asyncXmppReader.OnXmlElementComplete += onXmppElement;
            this.asyncXmppReader.OnXmlException += onXmlException;
        }

        public void BeginConnect()
        {
            this.asyncSocket.BeginConnect();
        }

        public void Disconnect()
        {
            this.asyncSocket.Disconnect();
        }

        public void Send(XmppElement xmppDocument)
        {
            // convert document into bytes
            var data = Encoding.UTF8.GetBytes(xmppDocument.ToString());

            // send through our socket
            this.asyncSocket.Send(data);
        }

        private void onConnected()
        {   
            // bubble event upwards
            if (this.OnConnected != null)
            {
                this.OnConnected();
            }
        }

        private void onConnectionException(Exception ex)
        {
            // to do: log something
            // otherwise..nothing to do, really.

            // bubble exception upwards
            if (this.OnConnectException != null)
            {
                this.OnConnectException(ex);
            }
        }

        private void onDataReceived(byte[] buffer, int length)
        {
            // send data into xmpp parser
            // results will be return in events bound to this object
            this.asyncXmppReader.ParseXmppElements(buffer, length);
        }

        private void onSocketUnexpectedlyClosed(IOException ex)
        {
            // to do: log something

            // bubble exception upwards
            if (this.OnSocketUnexpectedlyClosed != null)
            {
                this.OnSocketUnexpectedlyClosed(ex);
            }
        }

        private void onXmppDocumentStart(XmppElement root)
        {
            // bubble event upwards
            if (this.OnNewDocument != null)
            {
                this.OnNewDocument(root);
            }
        }

        private void onXmppElement(XmppElement node)
        {
            // bubble event upwards
            if (this.OnNewElement != null)
            {
                this.OnNewElement(node);
            }
        }

        private void onXmppDocumentEnd(XmppElement root)
        {
            // bubble event upwards
            if (this.OnDocumentComplete != null)
            {
                this.OnDocumentComplete(root);
            }
        }

        private void onXmlException(XmlException ex)
        {
            // bubble exception upwards
            if (this.OnXmlException != null)
            {
                this.OnXmlException(ex);
            }
        }
    }
}
