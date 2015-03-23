using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleXmpp.Protocol.Sasl
{
    [XmppName(Failure.Name, "urn:ietf:params:xml:ns:xmpp-sasl")]
    public class Failure : XmppElement
    {
        public const string Name = "failure";

        public Failure()
            : base(Name)
        {
        
        }
    }
}
