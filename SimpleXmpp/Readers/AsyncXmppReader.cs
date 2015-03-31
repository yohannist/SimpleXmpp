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
        private static XmlReaderSettings DefaultXmlReaderSettings = new XmlReaderSettings()
        {
            Async = true,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        };

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
        public AsyncXmppReader()
        {

        }

        public void ParseXmppElements(byte[] buffer, int length)
        {
            // run this in a background thread so the caller can be released
            // it can be run in the background because the results are returned in events
            Task.Run(() =>
            {
                // create stream from buffer
                var stream = new MemoryStream(buffer, 0, length);

                // create root node variable
                XmppElement currentNode = null;

                // create reader
                using (var reader = XmlReader.Create(stream, DefaultXmlReaderSettings))
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
        
        private void tagStart(ref XmppElement currentNode, XmlReader reader)
        {
            var name = reader.LocalName;
            var _namespace = reader.LookupNamespace(reader.Prefix);

            // create new node using the XmppFactory!
            XmppElement newNode = null;
            if (!XmppElementFactory.TryCreateXmppElement(name, _namespace, out newNode))
            {
                // if no specific xmpp element is found, let's go with a regular one
                newNode = new XmppElement(name);
            }

            // if the code has reached this point, it means the tag/xml is parsable/no namespace issues
            // so we can parse and add namespace information directly into our document
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
            if (isNewDocument && this.OnXmlDocumentStart != null)
            {
                this.OnXmlDocumentStart(currentNode);
            }
        }

        private void tagEnd(ref XmppElement currentNode, XmlReader reader)
        {
            // tag has ended
            // must make sure the opening and closing tag is the same name
            if (currentNode.Name.LocalName != reader.LocalName)
            {
                throw new XmlException(string.Format("Opening element name ({0}) does not match closing element name ({1})", currentNode.Name, reader.LocalName));
            }

            // call onElement because a complete element is parsed
            if (this.OnXmlElementComplete != null)
            {
                this.OnXmlElementComplete(currentNode);
            }

            // end by setting current to parent
            if (currentNode.Parent != null)
            {
                // this is a safe move because we know every node is created as an XmppNode
                currentNode = (XmppElement)currentNode.Parent;
            }
            else
            {
                // parent node is null, this means that we have hit the root node
                // call onDocumentEnd
                if (this.OnXmlDocumentEnd != null)
                {
                    this.OnXmlDocumentEnd(currentNode);
                }
            }
        }

        private void addText(ref XmppElement currentNode, XmlReader reader)
        {
            // set the value of the current node
            currentNode.Value = reader.Value;
        }

        private void parseNamespaceDeclarations(ref XmppElement node, XmlReader reader)
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
                    else if (string.Equals(reader.LocalName, "xmlns", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // default NS
                        namespaces.Add(new XAttribute("xmlns", reader.Value));
                    }

                } while (reader.MoveToNextAttribute());

                // add namespaces to current node
                if (namespaces.Count > 0)
                {
                    node.Add(namespaces);
                }
            }
        }

        private void parseNamespace(ref XmppElement currentNode, XmlReader reader)
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

        private void parseAttributes(ref XmppElement currentNode, XmlReader reader)
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
                    // skip namespace (and default namespace) declarations
                    if (!string.Equals(reader.Prefix, "xmlns", StringComparison.InvariantCultureIgnoreCase)
                        && !string.Equals(reader.LocalName, "xmlns", StringComparison.InvariantCultureIgnoreCase))
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
