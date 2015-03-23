using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleXmpp.Protocol.Sasl
{
    [XmppName(Mechanism.Name, "urn:ietf:params:xml:ns:xmpp-sasl")]
    public class Mechanism : XmppElement
    {
        public const string Name = "mechanism";

        public Mechanism()
            : base(Name)
        {
        
        }
    }
}
