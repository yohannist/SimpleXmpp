using SimpleXmpp;
using SimpleXmpp.Protocol.stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCloudMessaging.Ccs
{
    public class CcsClient
    {
        private const string CcsEndpoint = "gcm.googleapis.com";
        private const int CcsPort = 5235;
        private const string StagingCcsEndpoint = "gcm-preprod.googleapis.com";
        private const int StagingCcsPort = 5236;

        private XmppClient xmppClient;

        public string SenderId { get; private set; }
        private string apiKey { get; set; }
        public bool UseStagingGcmServers { get; private set; }
        public string Endpoint { get; private set; }
        public int Port { get; private set; }

        public CcsClient(string senderId, string apiKey, bool useStagingGcmServers = false)
        {
            this.SenderId = senderId;
            this.apiKey = apiKey;
            this.UseStagingGcmServers = useStagingGcmServers;

            // set endpoing:port to use
            this.Endpoint = useStagingGcmServers ? StagingCcsEndpoint : CcsEndpoint;
            this.Port = useStagingGcmServers ? StagingCcsPort : CcsPort;

            // must use ssl connection for gcm-ccs
            this.xmppClient = new XmppClient(this.Endpoint, this.Port, true);

            // setup sasl hander
        }

        public void BeginConnect()
        {
            // bind connected handler
            this.xmppClient.OnConnected += onConnected;

            // connect to xmpp client
            this.xmppClient.BeginConnect();
        }

        private void onConnected()
        {
            // send init
            var stream = new Stream()
            {
                To = "",
                Version = "1.0",
            };
        }
    }
}
