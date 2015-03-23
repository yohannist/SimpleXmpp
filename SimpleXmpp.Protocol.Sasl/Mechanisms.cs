using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleXmpp.Protocol.Sasl
{
    /*
     *  <str:features xmlns:str="http://etherx.jabber.org/streams">
     *      <mechanisms xmlns="urn:ietf:params:xml:ns:xmpp-sasl">
     *          <mechanism>X-OAUTH2</mechanism>
     *          <mechanism>X-GOOGLE-TOKEN</mechanism>
     *          <mechanism>PLAIN</mechanism>
     *      </mechanisms>
     *  </str:features>
     * 
     *  <auth mechanism="PLAIN"
     *      xmlns="urn:ietf:params:xml:ns:xmpp-sasl">MTI2MjAwMzQ3OTMzQHByb2plY3RzLmdjbS5hb
     *      mFTeUIzcmNaTmtmbnFLZEZiOW1oekNCaVlwT1JEQTJKV1d0dw==</auth>
     *      
     *  <success xmlns="urn:ietf:params:xml:ns:xmpp-sasl"/>
     *  
     *  <failure xmlns="urn:ietf:params:xml:ns:xmpp-sasl"/>
     * */

    [XmppName(Mechanisms.Name, "urn:ietf:params:xml:ns:xmpp-sasl")]
    public class Mechanisms : XmppElement
    {
        public const string Name = "mechanisms";
        private const string MechanismElementArrayName = "mechanism";

        public Mechanisms()
            : base(Name)
        {
            
        }

        public IEnumerable<XmppElement> Mechanism
        {
            get
            {
                return base.GetElements(MechanismElementArrayName);
            }
        }
    }
}
