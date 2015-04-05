using SimpleXmpp.Handlers;
using SimpleXmpp.Protocol.stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleXmpp.Protocol.Sasl
{
    public class SaslXmppHandler : XmppHandlerBase
    {
        private Dictionary<string, string> authenticationSets;
        private string lastMechanismUsed;

        public delegate void OnAuthenticationEventHandler(string mechanisn);
        public event OnAuthenticationEventHandler OnAuthenticationSuccess;
        public event OnAuthenticationEventHandler OnAuthenticationFailure;

        public SaslXmppHandler(XmppClient client)
            : base(client)
        {
            // create case insensitive dictionary for easier string comparison later
            this.authenticationSets = new Dictionary<string,string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void AddAuthenticationSet(string mechanism, string password)
        {
            this.authenticationSets[mechanism] = password;
        }

        public override void OnConnectException(Exception ex)
        {
            // do nothing
        }

        public override void OnConnected()
        {
            // do nothing
        }

        public override void OnSocketUnexpectedlyClosed(System.IO.IOException ex)
        {
            // do nothing
        }

        public override void OnNewDocument(XmppElement element)
        {
            // do nothing
        }

        public override void OnNewElement(XmppElement element)
        {
            // check if is features element
            // check if is success element
            // check if is failure element
            if (element is Features)
            {
                // get mechanisms
                var mechanisms = (element as Features).GetMechanisms();

                // loop through mechamisms and send authentications if they exist
                foreach (var mechanism in mechanisms.Mechanism)
                {
                    string password;
                    if (this.authenticationSets.TryGetValue(mechanism.Value, out password))
                    {
                        // if such authentication mechanism exists
                        // send the auth
                        this.sendAuthentication(mechanism.Value, password);

                        // save mechanism used for later events because it's not returned in auth response
                        lastMechanismUsed = mechanism.Value;

                        // no need to send multiple authentications
                        return;
                    }
                }
            }
            else if (element is Success)
            {
                // success element!
                // bubble event upwards
                if (this.OnAuthenticationSuccess != null)
                {
                    this.OnAuthenticationSuccess(lastMechanismUsed);
                }
            }
            else if (element is Failure)
            {
                // failure element =(
                // bubble event upwards
                if (this.OnAuthenticationFailure != null)
                {
                    this.OnAuthenticationFailure(lastMechanismUsed);
                }
            }
        }

        public override void OnDocumentComplete(XmppElement element)
        {
            // do nothing
        }

        public override void OnXmlException(System.Xml.XmlException ex)
        {
            // do nothing
        }

        private void sendAuthentication(string mechanism, string password)
        {
            // create authentication element
            var auth = new Auth()
            {
                Mechianism = mechanism,
                Value = password
            };

            // send it!
            this.client.Send(auth);
        }
    }
}
