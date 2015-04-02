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

        public delegate void OnConnectExceptionHandler(Exception exception);
        public delegate void OnConnectedEventHandler();
        /// <summary>
        /// Called when there is an exception thrown while trying to connect to the remote location.
        /// </summary>
        public event OnConnectExceptionHandler OnConnectException;
        public event OnConnectedEventHandler OnConnected;

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
            // // bind connect handlers
            this.xmppClient = new XmppClient(this.Endpoint, this.Port, true);
            this.xmppClient.OnConnected += onConnected;
            this.xmppClient.OnConnectException += onConnectException;

            // setup sasl hander
        }

        public void BeginConnect()
        {
            // connect to xmpp client
            this.xmppClient.BeginConnect();
        }

        private void onConnected()
        {
            // send init
            var stream = new Stream(true)
            {
                To = "gcm.googleapis.com",
                Version = "1.0",
            };

            this.xmppClient.Send(stream);
        }

        private void onConnectException(Exception ex)
        {

        }
    }
}
