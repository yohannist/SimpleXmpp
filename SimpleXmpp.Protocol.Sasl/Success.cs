using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleXmpp.Protocol.Sasl
{
    [XmppName(Success.NodeName, Success.NodeNamespace)]
    public class Success : XmppElement
    {
        public const string NodeName = "success";
        public const string NodeNamespace = "urn:ietf:params:xml:ns:xmpp-sasl";
        public const string DefaultNodePrefix = "";
        private const string MechanismElementArrayName = "mechanism";

        public Success(string prefix, XmlDocument parentDocument)
            : base(prefix, NodeName, NodeNamespace, parentDocument)
        {
        }

        public Success(XmlDocument parentDocument)
            : base(DefaultNodePrefix, NodeName, NodeNamespace, parentDocument)
        {
        }
    }
}
