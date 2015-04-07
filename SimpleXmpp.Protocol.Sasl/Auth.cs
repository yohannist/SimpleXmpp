using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SimpleXmpp.Protocol.Sasl
{
    [XmppName(Auth.NodeName, Auth.NodeNamespace)]
    public class Auth : XmppElement
    {
        public const string NodeName = "auth";
        public const string NodeNamespace = "urn:ietf:params:xml:ns:xmpp-sasl";
        public const string DefaultNodePrefix = "";
        private const string MechanismAttributeName = "mechanism";
        
        public Auth(string prefix, XmlDocument parentDocument)
            : base(prefix, NodeName, NodeNamespace, parentDocument)
        {
        }

        public Auth(XmlDocument parentDocument)
            : base(DefaultNodePrefix, NodeName, NodeNamespace, parentDocument)
        {
        }

        public string Mechianism
        {
            get
            {
                return base.GetAttributeValue(MechanismAttributeName);
            }
            set
            {
                base.SetAttributeValue(MechanismAttributeName, value);
            }
        }
    }
}
