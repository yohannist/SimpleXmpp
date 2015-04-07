using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleXmpp.Protocol.Sasl
{
    /*
     * <str:stream from="xxx" version="xxx">
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
     *  ...
     *  ..
     *  ..
     *  ..
     * */

    [XmppName(Mechanisms.NodeName, Mechanisms.NodeNamespace)]
    public class Mechanisms : XmppElement
    {
        public const string NodeName = "mechanisms";
        public const string NodeNamespace = "urn:ietf:params:xml:ns:xmpp-sasl";
        public const string DefaultNodePrefix = "";
        private const string MechanismElementArrayName = "mechanism";

        public Mechanisms(string prefix, XmlDocument parentDocument)
            : base(prefix, NodeName, NodeNamespace, parentDocument)
        {
        }

        public Mechanisms(XmlDocument parentDocument)
            : base(DefaultNodePrefix, NodeName, NodeNamespace, parentDocument)
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
