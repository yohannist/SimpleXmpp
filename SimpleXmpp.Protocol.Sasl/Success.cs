using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleXmpp.Protocol.Sasl
{
    [XmppName(Success.Name, "urn:ietf:params:xml:ns:xmpp-sasl")]
    public class Success : XmppElement
    {
        public const string Name = "success";

        public Success()
            : base(Name)
        {
        
        }
    }
}
