using System;

namespace SimpleXmpp.Net
{
    using System.IO;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;

    public class AsyncSocket
    {
        private const int BufferSize = 4096;
        private const SslProtocols DefaultSslProtocol = SslProtocols.Tls;

        public delegate void OnConnectedEventHander(Stream networkStream);
        public delegate void OnConnectExceptionHandler(Exception ex);
        private event OnConnectedEventHander OnConnected;
        public event OnConnectExceptionHandler OnConnectException;

        private Socket socket;
        private Stream networkStream;
        private byte[] buffer = new byte[BufferSize];

        public string Hostname { get; private set; }
        public int Port { get; private set; }
        public bool IsSslConnection { get; private set; }
        public bool IsConnected { get; private set; }

        public AsyncSocket(string hostname, int port, bool isSslConnection)
        {
            this.Hostname = hostname;
            this.Port = port;
            this.IsSslConnection = isSslConnection;

            // create new socket
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // ipv6?
            //this.IsIpV6 ? new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)
        }

        public void BeginConnect(OnConnectedEventHander runOnConnected)
        {
            // save handler to call in onConnected() method
            this.OnConnected += runOnConnected;

            try
            {
                // begin async connect
                this.socket.BeginConnect(this.Hostname, this.Port, onConnected, socket);
            }
            catch (Exception ex)
            {
                // bubble exception using events 
                onConnectException(ex);
            }
        }

        public void Disconnect()
        {
            this.IsConnected = false;

            if (this.networkStream != null)
            {
                // flush stream so awaiting methods will complete
                this.networkStream.Flush();

                // close read/write stream
                this.networkStream.Close();
            }

            if (this.socket.Connected)
            {
                // shutdown & disconnect socket
                this.socket.Shutdown(SocketShutdown.Both);
                this.socket.Disconnect(true);
            }
        }

        public void Send(byte[] data)
        {
            this.networkStream.BeginWrite(data, 0, data.Length, onSendComplete, null);
        }

        private void onConnected(IAsyncResult result)
        {
            try
            {
                // end connect when connected
                var socket = (Socket)result.AsyncState;
                socket.EndConnect(result);

                // create stream
                this.networkStream = new NetworkStream(socket);

                // init ssl stream
                if (this.IsSslConnection)
                {
                    this.networkStream = initXmppSslConnection(this.networkStream, this.Hostname, DefaultSslProtocol);
                }
            }
            catch (Exception ex)
            {
                // bubble exception using events 
                onConnectException(ex);
            }

            // connected & streams created successfully
            this.IsConnected = true;
            if (this.OnConnected != null)
            {
                // return the network stream back to the caller
                // this is so that the caller can handle the stream reading & writing themselves
                this.OnConnected(this.networkStream);

                // event clean up because not needed
                this.OnConnected = null;
            }
        }

        private void onConnectException(Exception ex)
        {
            // todo: log

            // bubble exception upwards
            if (this.OnConnectException != null)
            {
                this.OnConnectException(ex);
            }
        }

        private void onSendComplete(IAsyncResult result)
        {
            // stop sending ~
            this.networkStream.EndWrite(result);
        }

        private static Stream initXmppSslConnection(Stream innerStream, string hostname, SslProtocols protocol)
        {
            // create ssl stream
            var sslStream = new SslStream(innerStream, false);

            // authenticate ssl stream
            sslStream.AuthenticateAsClient(hostname, null, protocol, false);

            return sslStream;
        }
    }
}
