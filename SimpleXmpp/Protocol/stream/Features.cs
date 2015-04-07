using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleXmpp.Protocol.stream
{
    [XmppName(Features.NodeName, "http://etherx.jabber.org/streams")]
    public class Features : XmppElement
    {
        public const string NodeName = "features";
        public const string NodeNamespace = "http://etherx.jabber.org/streams";
        public const string DefaultNodePrefix = "stream";

        public Features(string prefix, XmlDocument parentDocument)
            : base(prefix, NodeName, NodeNamespace, parentDocument)
        {
        }

        public Features(XmlDocument parentDocument)
            : base(DefaultNodePrefix, NodeName, NodeNamespace, parentDocument)
        {
        }
    }
}
