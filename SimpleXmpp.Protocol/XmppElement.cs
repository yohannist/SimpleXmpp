using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SimpleXmpp.Protocol
{
    public class XmppElement : XElement
    {
        public XmppElement(string name)
            : base(name)
        {

        }

        protected string GetAttributeValue(string name)
        {
            var attribute = base.Attribute(name);
            return attribute == null ? null : attribute.Value;
        }

        protected void SetAttributeValue(string name, string value)
        {
            var attribute = base.Attribute(name);
            if (attribute != null)
            {
                attribute.Value = value;
            }
            else
            {
                this.Add(new XAttribute(name, value));
            }
        }

        /// <summary>
        /// Gets the first (in document order) child XmppElement with the specified name
        /// </summary>
        /// <param name="name">The name to match.</param>
        /// <returns></returns>
        protected XmppElement GetElement(string name)
        {
            return base.Element(name) as XmppElement;
        }

        /// <summary>
        /// Returns a filtered collection of the child elements of this element or document, in document order with the specified name.
        /// </summary>
        /// <param name="name">The name to match.</param>
        /// <returns></returns>
        protected IEnumerable<XmppElement> GetElements(string name)
        {
            return base.Elements(name).Select(e => e as XmppElement);
        }
    }
}
