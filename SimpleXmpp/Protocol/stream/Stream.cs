using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleXmpp.Protocol.stream
{
    [XmppName(Stream.Name, "http://etherx.jabber.org/streams")]
    public class Stream : XmppElement
    {
        public const string Name = "stream";
        private const string IdAttributeName = "id";
        private const string ToAttributeName = "to";
        private const string VersionAttributeName = "version";

        public Stream()
            : base(Name)
        {

        }

        public string Id
        {
            get
            {
                return base.GetAttributeValue(IdAttributeName);
            }
            set
            {
                base.SetAttributeValue(IdAttributeName, value);
            }
        }

        public string To 
        { 
            get
            {
                return base.GetAttributeValue(ToAttributeName);
            }
            set 
            {
                base.SetAttributeValue(ToAttributeName, value);
            }
        }

        public string Version
        {
            get 
            {
                return base.GetAttributeValue(VersionAttributeName);
            }
            set
            {
                base.SetAttributeValue(VersionAttributeName, value);
            }
        }
    }
}