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
        private const string MechanismsElementName = "mechanisms";

        public Features()
            : base(Name)
        {
            
        }

        public XmppElement Mechanisms
        {
            get 
            {
                return base.GetElement(MechanismsElementName);
            }
        }
    }
}
