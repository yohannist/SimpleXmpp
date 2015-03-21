using System;
using System.Collections.Generic;

namespace SimpleXmpp
{
    using SimpleXmpp.Net;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    public class XmppClient
    {
        private static XmlReaderSettings DefaultXmlReaderSettings = new XmlReaderSettings()
        {
            Async = true,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        };

        public delegate void OnXElementHandler(XElement node);
        public delegate void OnXExceptionHandler(XmlException exception);
        public delegate void OnConnectExceptionHandler(Exception exception);
        public event OnXElementHandler OnXDocumentStart;
        public event OnXElementHandler OnXElementComplete;
        public event OnXElementHandler OnXDocumentEnd;
        public event OnXExceptionHandler OnXmlException;
        public event OnConnectExceptionHandler OnConnectException;

        private AsyncSocket asyncSocket;
        private bool IsConnected;

        public string Hostname { get; private set; }
        public int Port { get; private set; }
        public bool UsesSslConnection { get; private set; }

        public XmppClient(string hostname, int port, bool usesSslConnection)
        {
            this.Hostname = hostname;
            this.Port = port;
            this.UsesSslConnection = usesSslConnection;

            // create socket
            this.asyncSocket = new AsyncSocket(this.Hostname, this.Port, this.UsesSslConnection);
            this.asyncSocket.OnConnectException += handleConnectionException;
        }

        public void BeginConnect()
        {
            this.asyncSocket.BeginConnect(onConnected);
        }

        public void Disconnect()
        {
            this.IsConnected = false;
            this.asyncSocket.Disconnect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="networkStream"></param>
        /// <exception cref="">XmlException("Undeclared prefix")</exception>
        /// <exception cref="">XmlException("malformed tag")</exception>
        private async void onConnected(Stream networkStream)
        {
            this.IsConnected = true;

            // once connected, we can open an XmlReader and asynchronously read the stream
            using (var reader = XmlReader.Create(networkStream, DefaultXmlReaderSettings))
            {
                // go into an infinite reading cycle until this connection is terminated
                // this loop will "async wait" at await reader.ReadAsync(), thus not consuming CPU cycle when not in use
                while (true)
                {
                    // make connection check for better response
                    if (!this.IsConnected)
                    {
                        return;
                    }

                    // create root node variable
                    XElement currentNode = null;

                    try
                    {
                        // reader.ReadAsync() will only return false at the end of a document, otherwise it will keep waiting
                        while (await reader.ReadAsync())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    // start of a tag
                                    tagStart(ref currentNode, reader);
                                    break;
                                case XmlNodeType.Text:
                                case XmlNodeType.CDATA:
                                    // text or cdata
                                    addText(ref currentNode, reader);
                                    break;
                                case XmlNodeType.EndElement:
                                    // end of a tag
                                    tagEnd(ref currentNode, reader);
                                    break;
                            }

                            // make connection check for better response
                            if (!this.IsConnected)
                            {
                                return;
                            }
                        }
                    }
                    catch (XmlException ex)
                    {
                        // call private onXmlException
                        onXmlException(ex);
                        //throw;
                    }
                }
            }
        }

        private void onXmlException(XmlException ex)
        {
            // do something? like log?
            // raise event because the exception thown is in a background thread
            // there's no way for the exception to bubble back to the main thread
            if (this.OnXmlException != null)
            {
                this.OnXmlException(ex);
            }
        }

        private void tagStart(ref XElement currentNode, XmlReader reader)
        {
            // create new node
            var newNode = new XElement(reader.LocalName);

            // parse and add namespace information
            // this has to be run first so we have namespace information to run for attributes and such later
            // reader will be moved to last attribute
            parseNamespaceDeclarations(ref newNode, reader);

            // attach the current node to the document
            bool isNewDocument = false;
            if (currentNode == null)
            {
                // this is the start of a new document
                // init node
                currentNode = newNode;
                isNewDocument = true;
            }
            else
            {
                // if the current node is not null & the reader depth is 0,
                // it means that there is more than 1 root element
                // in xml, there cannot be more than 1 root element
                if (reader.Depth <= 0 && currentNode != null)
                {
                    throw new XmlException(string.Format("There cannot be multiple root elements, {0} and {1}", currentNode.Name, reader.LocalName));
                }

                // this is a child element
                // so we append it to the current element
                currentNode.Add(newNode);

                // after appending, the newnode becomes the current node
                currentNode = newNode;
            }

            // parse namespace for element
            // this has to be run after adding to the current tree
            // reader will be moved to the element ndoe
            parseNamespace(ref newNode, reader);

            // parse attributes if any
            // this has to be run after adding to the current tree
            // reader will be moved to last attribute
            parseAttributes(ref newNode, reader);

            // call document on start if required
            if (isNewDocument && this.OnXDocumentStart != null)
            {
                this.OnXDocumentStart(currentNode);
            }
        }

        private void tagEnd(ref XElement currentNode, XmlReader reader)
        {
            // tag has ended
            // must make sure the opening and closing tag is the same name
            if (currentNode.Name.LocalName != reader.LocalName)
            {
                throw new XmlException(string.Format("Opening element name ({0}) does not match closing element name ({1})", currentNode.Name, reader.LocalName));
            }

            // call onElement because a complete element is parsed
            if (this.OnXElementComplete != null)
            {
                this.OnXElementComplete(currentNode);
            }

            // end by setting current to parent
            if (currentNode.Parent != null)
            {
                currentNode = currentNode.Parent;
            }
            else
            {
                // parent node is null, this means that we have hit the root node
                // call onDocumentEnd
                if (this.OnXDocumentEnd != null)
                {
                    this.OnXDocumentEnd(currentNode);
                }
            }
        }

        private void addText(ref XElement currentNode, XmlReader reader)
        {
            // set the value of the current node
            currentNode.Value = reader.Value;
        }

        private void parseNamespaceDeclarations(ref XElement node, XmlReader reader)
        {
            if (reader.HasAttributes)
            {
                // create container
                var namespaces = new List<XAttribute>(reader.AttributeCount);

                // reset attribute position (just in case)
                reader.MoveToFirstAttribute();

                // populate
                do
                {
                    // only pick up attributes that start with "xmlns"
                    if (string.Equals(reader.Prefix, "xmlns", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // create new NS attribute
                        namespaces.Add(new XAttribute(XNamespace.Xmlns + reader.LocalName, reader.Value));
                    }
                } while (reader.MoveToNextAttribute());

                // add namespaces to current node
                if (namespaces.Count > 0)
                {
                    node.Add(namespaces);
                }
            }
        }

        private void parseNamespace(ref XElement currentNode, XmlReader reader)
        {
            // in case the reader is on the attribute
            reader.MoveToElement();

            if (!string.IsNullOrWhiteSpace(reader.Prefix))
            {
                // there is a namespace
                // let's find it (ns because "namespace" is a keyword)
                var ns = currentNode.GetNamespaceOfPrefix(reader.Prefix);

                // add it to our node
                currentNode.Name = ns + currentNode.Name.LocalName;
            }
        }

        private void parseAttributes(ref XElement currentNode, XmlReader reader)
        {
            if (reader.HasAttributes)
            {
                // create container
                var attributes = new List<XAttribute>(reader.AttributeCount);

                // reset attribute position (just in case)
                reader.MoveToFirstAttribute();

                // populate
                do
                {
                    // skip namespace declarations
                    if (!string.Equals(reader.Prefix, "xmlns", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(reader.Prefix))
                        {
                            // there is a namespace
                            // let's find it (ns because "namespace" is a keyword)
                            var ns = currentNode.GetNamespaceOfPrefix(reader.Prefix);
                            attributes.Add(new XAttribute(ns + reader.LocalName, reader.Value));
                        }
                        else
                        {
                            attributes.Add(new XAttribute(reader.LocalName, reader.Value));
                        }
                    }
                } while (reader.MoveToNextAttribute());

                // add namespaces to current node
                if (attributes.Count > 0)
                {
                    currentNode.Add(attributes);
                }
            }
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
