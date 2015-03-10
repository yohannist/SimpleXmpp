﻿using System;

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
        private event OnConnectedEventHander OnConnected;

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
        }

        public void BeginConnect(OnConnectedEventHander runOnConnected)
        {
            // create new socket
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // ipv6?
            //this.IsIpV6 ? new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)

            // save handler to call in onConnected() method
            this.OnConnected = runOnConnected;

            try
            {
                // begin async connect
                this.socket.BeginConnect(this.Hostname, this.Port, onConnected, socket);
            }
            catch
            {
                // todo: log error
                throw;
            }
        }

        public void Disconnect()
        {
            this.IsConnected = false;

            // flush stream so awaiting methods will complete
            this.networkStream.Flush();

            // close read/write stream
            this.networkStream.Close();

            // shutdown & disconnect socket
            this.socket.Shutdown(SocketShutdown.Both);
            this.socket.Disconnect(true);
        }

        public void Send(byte[] data)
        {
            this.networkStream.BeginWrite(data, 0, data.Length, onSendComplete, null);
        }

        private void onConnected(IAsyncResult result)
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

            this.IsConnected = true;
            if (this.OnConnected != null)
            {
                // return the network stream back to the caller
                // this is so that the caller can handle the stream reading & writing themselves
                this.OnConnected(this.networkStream);

                // remove event because it's not needed anymore
                this.OnConnected = null;
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
