using SimpleXmpp.Protocol.stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleXmpp.Protocol.Sasl
{
    public static class FeaturesExtension
    {
        private const string MechanismsElementName = "mechanisms";

        /// <summary>
        /// Gets the "Mechanisms" child element. Could be written as a extension property when supported.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Mechanisms GetMechanisms(this Features self)
        {
            return self.GetElement(MechanismsElementName) as Mechanisms;
        }
    }
}
