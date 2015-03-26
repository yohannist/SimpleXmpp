using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleXmpp.Protocol.stream
{
    [XmppName(Features.Name, "http://etherx.jabber.org/streams")]
    public class Features : XmppElement
    {
        public const string Name = "features";

        public Features()
            : base(Name)
        {
            
        }
    }
}
