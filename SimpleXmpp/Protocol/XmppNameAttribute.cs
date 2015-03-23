using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleXmpp.Protocol
{
    public class XmppNameAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Namespace { get; private set; }

        public XmppNameAttribute(string name, string _namespace)
        {
            this.Name = name;
            this.Namespace = _namespace;
        }
    }
}
