using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SimpleXmpp.Protocol.stream
{
    [XmppName(Stream.NodeName, "http://etherx.jabber.org/streams")]
    public class Stream : XmppElement
    {
        public const string NodeName = "stream";
        public const string NodeNamespace = "http://etherx.jabber.org/streams";
        public const string NodePrefix = "stream";
        private const string IdAttributeName = "id";
        private const string ToAttributeName = "to";
        private const string VersionAttributeName = "version";

        public Stream()
            : base(NodeName)
        {
        }

        public Stream(bool applyDefaultNamespace)
            : base(NodeName)
        {
            if (applyDefaultNamespace) 
            {
                // this node has to be on it's own name space
                XNamespace stream = NodeNamespace;
                this.Add(new XAttribute(XNamespace.Xmlns + NodePrefix, stream));
                this.Name = stream + this.Name.LocalName;
            }
        }

        public string Id
        {
            get
            {
                return base.GetAttributeValue(IdAttributeName);
            }
            set
            {
                base.SetAttributeValue(IdAttributeName, value);
            }
        }

        public string To 
        { 
            get
            {
                return base.GetAttributeValue(ToAttributeName);
            }
            set 
            {
                base.SetAttributeValue(ToAttributeName, value);
            }
        }

        public string Version
        {
            get 
            {
                return base.GetAttributeValue(VersionAttributeName);
            }
            set
            {
                base.SetAttributeValue(VersionAttributeName, value);
            }
        }
    }
}