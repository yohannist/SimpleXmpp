using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleXmpp.Protocol.Sasl
{
    [XmppName(Failure.NodeName, Failure.NodeNamespace)]
    public class Failure : XmppElement
    {
        public const string NodeName = "failure";
        public const string NodeNamespace = "urn:ietf:params:xml:ns:xmpp-sasl";
        public const string DefaultNodePrefix = "";
        
        public Failure(string prefix, XmlDocument parentDocument)
            : base(prefix, NodeName, NodeNamespace, parentDocument)
        {
        }

        public Failure(XmlDocument parentDocument)
            : base(DefaultNodePrefix, NodeName, NodeNamespace, parentDocument)
        {
        }
    }
}
