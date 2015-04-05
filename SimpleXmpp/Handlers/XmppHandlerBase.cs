using SimpleXmpp.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleXmpp.Handlers
{
    public abstract class XmppHandlerBase
    {
        protected XmppClient client;

        public XmppHandlerBase(XmppClient client)
        {
            this.client = client;

            this.client.OnConnected += this.OnConnected;
            this.client.OnConnectException += this.OnConnectException;
            this.client.OnSocketUnexpectedlyClosed += this.OnSocketUnexpectedlyClosed;
            this.client.OnNewDocument += this.OnNewDocument;
            this.client.OnNewElement += this.OnNewElement;
            this.client.OnDocumentComplete += this.OnDocumentComplete;
            this.client.OnXmlException += this.OnXmlException;
        }

        public abstract void OnConnected();
        public abstract void OnConnectException(Exception ex);
        public abstract void OnSocketUnexpectedlyClosed(IOException ex);
        public abstract void OnNewDocument(XmppElement element);
        public abstract void OnNewElement(XmppElement element);
        public abstract void OnDocumentComplete(XmppElement element);
        public abstract void OnXmlException(XmlException ex);
    }
}
