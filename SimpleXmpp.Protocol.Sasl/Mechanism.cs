using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleXmpp.Protocol.Sasl
{
    [XmppName(Mechanism.NodeName, Mechanism.NodeNamespace)]
    public class Mechanism : XmppElement
    {
        public const string NodeName = "mechanism";
        public const string NodeNamespace = "urn:ietf:params:xml:ns:xmpp-sasl";
        public const string DefaultNodePrefix = "";
        
        public Mechanism(string prefix, XmlDocument parentDocument)
            : base(prefix, NodeName, NodeNamespace, parentDocument)
        {
        }

        public Mechanism(XmlDocument parentDocument)
            : base(DefaultNodePrefix, NodeName, NodeNamespace, parentDocument)
        {
        }
    }
}
