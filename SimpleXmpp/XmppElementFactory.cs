using SimpleXmpp.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Xml;

namespace SimpleXmpp
{
    public static class XmppElementFactory
    {
        private static readonly XmppElementTypeWarehouse typeWarehouse;
        private static readonly Type[] derivedXmppElementConstuctorInfo = new Type[] { typeof(string), typeof(XmlDocument) };

        static XmppElementFactory()
        {
            // init storage
            typeWarehouse = new XmppElementTypeWarehouse();

            // load other assembiles indicated
            loadDynamicAssemblies();

            // populate storage
            populateWarehouse();
        }

        private static void loadDynamicAssemblies()
        {
            var dynamicAssemblies = (NameValueCollection)ConfigurationManager.GetSection("assemblies");
            if (dynamicAssemblies != null) 
            {
                foreach (string key in dynamicAssemblies)
                {
                    Assembly.LoadFrom(dynamicAssemblies[key]);
                }
            }
        }

        private static void populateWarehouse()
        {
            // get assemblies in current domain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // initialize element factory with all elements declared in application
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // foreach assembly, find all types with XmppName Attribute
                foreach (var type in assembly.GetTypes())
                {
                    // check whether it is inheritied from the XmppElement class
                    if (type.IsSubclassOf(typeof(XmppElement)))
                    {
                        // find the attribute that defines the name and namespace
                        var attributes = type.GetCustomAttributes(typeof(XmppNameAttribute), false) as XmppNameAttribute[];
                        if (attributes != null && attributes.Length > 0)
                        {
                            // only use the first attribute if there are multiple
                            typeWarehouse.AddOrUpdate(attributes[0].Name, attributes[0].Namespace, type);
                        }
                    }
                }
            }
        }

        public static bool TryCreateXmppElement(string name, string ns, string prefix, XmlDocument parentDocument, out XmppElement xmppElement)
        {
            Type xmppType;
            if (typeWarehouse.TryGet(name, ns, out xmppType))
            {
                // if we can find a specific xmpp type from the declared types
                // return a version of it
                var constructorInfo = xmppType.GetConstructor(derivedXmppElementConstuctorInfo);
                if (constructorInfo != null)
                {
                    // make sure invoking type has required constructor
                    xmppElement = (XmppElement)constructorInfo.Invoke(new object[] { prefix, parentDocument });
                    return true;
                }
                else
                {
                    // if there is no such constructor, it's as good as not present
                    xmppElement = null;
                    return false;
                }
            }
            else
            {
                // otherwise, let's return false so use can handle it
                xmppElement = null;
                return false;
            }
        }

        private class XmppElementTypeWarehouse
        {
            private Dictionary<string, Type> storage = new Dictionary<string,Type>();

            public int Count 
            {
                get 
                {
                    return storage.Count;
                }
            }

            public void AddOrUpdate(string name, string _namespace, Type type)
            {
                var key = computeKey(name, _namespace);
                storage[key] = type;
            }

            public Type Get(string name, string _namespace)
            {
                var key = computeKey(name, _namespace);
                return storage[key];
            }

            public bool TryGet(string name, string _namespace, out Type type)
            {
                var key = computeKey(name, _namespace);
                return storage.TryGetValue(key, out type);
            }

            private string computeKey(string name, string _namespace)
            {
                return name + ":" + _namespace;
            }
        }
    }
}
