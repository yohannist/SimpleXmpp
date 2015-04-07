using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SimpleXmpp.Protocol.stream
{
    [XmppName(Stream.NodeName, Stream.NodeNamespace)]
    public class Stream : XmppElement
    {
        public const string NodeName = "stream";
        public const string NodeNamespace = "http://etherx.jabber.org/streams";
        public const string DefaultNodePrefix = "stream";
        private const string IdAttributeName = "id";
        private const string ToAttributeName = "to";
        private const string VersionAttributeName = "version";

        public Stream(string prefix, XmlDocument parentDocument)
            : base(prefix, NodeName, NodeNamespace, parentDocument)
        {
        }

        public Stream(XmlDocument parentDocument)
            : base(DefaultNodePrefix, NodeName, NodeNamespace, parentDocument)
        {
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