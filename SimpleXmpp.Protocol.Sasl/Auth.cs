using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleXmpp.Protocol.Sasl
{
    [XmppName(Auth.Name, "urn:ietf:params:xml:ns:xmpp-sasl")]
    public class Auth : XmppElement
    {
        public const string Name = "auth";
        private const string MechanismAttributeName = "mechanism";

        public Auth()
            : base(Name)
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
