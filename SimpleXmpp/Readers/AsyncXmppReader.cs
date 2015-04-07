using SimpleXmpp.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SimpleXmpp.Readers
{
    public class AsyncXmppReader
    {
        private static readonly XmlReaderSettings DefaultXmlReaderSettings = new XmlReaderSettings()
        {
            Async = true,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true,
            ConformanceLevel = ConformanceLevel.Fragment,
        };

        private XmlDocument receivingDocument;
        private XmlNamespaceManager namespaceManager;
        private XmlParserContext xmlContext;
        private XmppElement root;
        private XmppElement currentElement;

        public delegate void OnXmppElementEventHandler(XmppElement node);
        public delegate void OnXmlExceptionHandler(XmlException exception);
        /// <summary>
        /// Called on the opening tag of a new document. Only contains the opening tag.
        /// </summary>
        public event OnXmppElementEventHandler OnXmlDocumentStart;
        /// <summary>
        /// Called on the closing tag of a xml node. Contains the entire node.
        /// </summary>
        public event OnXmppElementEventHandler OnXmlElementComplete;
        /// <summary>
        /// Called on the closing tag of the root element. Contains the entire document.
        /// </summary>
        public event OnXmppElementEventHandler OnXmlDocumentEnd;
        /// <summary>
        /// Called when there is an exception thrown while parsing the document.
        /// </summary>
        public event OnXmlExceptionHandler OnXmlException;
        
        /// <summary>
        /// Creates
        /// </summary>
        public AsyncXmppReader(XmlDocument receivingDocument)
        {
            this.receivingDocument = receivingDocument;
            this.namespaceManager = new XmlNamespaceManager(receivingDocument.NameTable);
            this.xmlContext = new XmlParserContext(null, this.namespaceManager, null, XmlSpace.None);
        }

        public void ParseXmppElements(byte[] buffer, int length)
        {
            // run this in a background thread so the caller can be released
            // it can be run in the background because the results are returned in events
            Task.Run(() =>
            {
                // create stream from buffer
                var stream = new MemoryStream(buffer, 0, length);
                
                // create reader
                using (var reader = XmlReader.Create(stream, DefaultXmlReaderSettings, xmlContext))
                {
                    try
                    {
                        // reader.Read() will only return false at the end of a document, otherwise it will keep waiting
                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    // start of a tag
                                    this.tagStart(reader);
                                    if (reader.IsEmptyElement)
                                    {
                                        // self closing element
                                        // <name />
                                        this.tagEnd(reader);
                                    }
                                    break;
                                case XmlNodeType.Text:
                                    // add text
                                    this.addText(reader);
                                    break;
                                case XmlNodeType.CDATA:
                                    // add cdata
                                    this.addCData(reader);
                                    break;
                                case XmlNodeType.EndElement:
                                    // end of a tag
                                    this.tagEnd(reader);
                                    break;
                            }
                        }
                    }
                    catch (XmlException ex)
                    {
                        // client has returned invalid xml data.
                        // could be just data corruption, so trigger the exception event and continue reading
                        // call private onXmlException
                        onXmlException(ex);
                    }
                }
            });
        }

        private void tagStart(XmlReader reader)
        {
            // must parse namespaces first
            // reader will be moved to last attribute
            this.parseNamespaces(reader);
            // move it back to element
            reader.MoveToElement();

            // get element name and ns
            var name = reader.LocalName;
            var ns = namespaceManager.LookupNamespace(reader.Prefix);

            // create new node using the XmppFactory!
            XmppElement newNode = null;
            if (!XmppElementFactory.TryCreateXmppElement(name, ns, reader.Prefix, receivingDocument, out newNode))
            {
                // if no specific xmpp element is found, let's go with a regular one
                newNode = new XmppElement(reader.Prefix, name, ns, receivingDocument);
            }

            // if the code has reached this point, it means the tag/xml is parsable/no namespace issues
            // so we can parse and add namespace information directly into our document
            // this has to be run first so we have namespace information to run for attributes and such later
            // reader will be moved to last attribute
            //parseNamespaceDeclarations(ref newNode, reader);

            // attach the current node to the document
            bool isNewDocument = false;
            if (this.root == null)
            {
                // this is the start of a new document
                // init node
                root = currentElement = newNode;
                isNewDocument = true;
            }
            else
            {
                // this is a child element
                // so we append it to the current element
                currentElement.AppendChild(newNode);

                // after appending, the newnode becomes the current node
                currentElement = newNode;
            }

            // parse attributes if any
            // this has to be run after adding to the current tree
            // reader will be moved to last attribute
            this.parseAttributes(reader);

            // call document on start if required
            if (isNewDocument && this.OnXmlDocumentStart != null)
            {
                this.OnXmlDocumentStart(currentElement);
            }
        }

        private void tagEnd(XmlReader reader)
        {
            // tag has ended
            // call onElement because a complete element is parsed
            if (this.OnXmlElementComplete != null)
            {
                this.OnXmlElementComplete(this.currentElement);
            }

            // end by setting current to parent
            if (this.currentElement.ParentNode != null)
            {
                // this is a safe cast because we know every node is created as an XmppNode
                this.currentElement = (XmppElement)this.currentElement.ParentNode;
            }
            else
            {
                // parent node is null, this means that we have hit the root node
                // call onDocumentEnd
                if (this.OnXmlDocumentEnd != null)
                {
                    this.OnXmlDocumentEnd(this.currentElement);
                }
            }
        }

        private void addText(XmlReader reader)
        {
            // set the value of the current node
            this.currentElement.AppendChild(this.receivingDocument.CreateTextNode(reader.Value));
        }

        private void addCData(XmlReader reader)
        {
            // set the value of the current node
            this.currentElement.AppendChild(this.receivingDocument.CreateCDataSection(reader.Value));
        }

        private void parseNamespaces(XmlReader reader)
        {
            if (reader.HasAttributes)
            {
                // reset attribute position (just in case)
                reader.MoveToFirstAttribute();

                // populate
                do
                {
                    // only pick up attributes that start with "xmlns"
                    if (string.Equals(reader.Prefix, "xmlns", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // add scoped namespace to manager
                        this.namespaceManager.AddNamespace(reader.LocalName, reader.Value);
                    }
                    else if (string.Equals(reader.LocalName, "xmlns", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // add default namespace to manager
                        this.namespaceManager.AddNamespace(string.Empty, reader.Value);
                    }

                } while (reader.MoveToNextAttribute());
            }
        }

        private void parseAttributes(XmlReader reader)
        {
            if (reader.HasAttributes)
            {
                // create temp variable
                XmlAttribute attribute;

                // reset attribute position (just in case)
                reader.MoveToFirstAttribute();

                // populate
                do
                {
                    // skip namespace (and default namespace) declarations
                    // because they have already been parsed into the namespace manager (connected to current document) earlier
                    if (!string.Equals(reader.Prefix, "xmlns", StringComparison.InvariantCultureIgnoreCase)
                        && !string.Equals(reader.LocalName, "xmlns", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(reader.Prefix))
                        {
                            // there is a namespace
                            // let's find it (ns because "namespace" is a keyword)
                            var ns = this.namespaceManager.LookupNamespace(reader.Prefix);
                            attribute = this.receivingDocument.CreateAttribute(reader.Prefix, reader.LocalName, ns);
                            attribute.Value = reader.Value;
                        }
                        else
                        {
                            // create attribute without namespace
                            attribute = this.receivingDocument.CreateAttribute(reader.LocalName);
                            attribute.Value = reader.Value;
                        }

                        // append to element
                        this.currentElement.Attributes.Append(attribute);
                    }
                } while (reader.MoveToNextAttribute());
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
    }
}
