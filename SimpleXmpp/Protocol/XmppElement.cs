using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SimpleXmpp.Protocol
{
    public class XmppElement : XmlElement
    {
        public XmppElement(string prefix, string name, string ns, XmlDocument parentDocument)
            : base(prefix, name, ns, parentDocument)
        {

        }

        public string GetAttributeValue(string name, string ns = "")
        {
            return base.GetAttribute(name, ns);
        }

        public void SetAttributeValue(string name, string value, string ns = "")
        {
            base.SetAttribute(name, ns, value);
        }

        /// <summary>
        /// Gets the first (in document order) child XmppElement with the specified name
        /// </summary>
        /// <param name="name">The name to match.</param>
        /// <returns></returns>
        public XmppElement GetElement(string nameIncludingPrefix)
        {
            return base.SelectSingleNode(nameIncludingPrefix) as XmppElement;
        }

        /// <summary>
        /// Returns a filtered collection of the child elements of this element or document, in document order with the specified name.
        /// </summary>
        /// <param name="name">The name to match.</param>
        /// <returns></returns>
        public IEnumerable<XmppElement> GetElements(string nameIncludingPrefix)
        {
            return base.SelectNodes(nameIncludingPrefix).Cast<XmppElement>();
        }
    }
}
