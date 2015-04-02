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

        public delegate void OnConnectedEventHander();
        public delegate void OnConnectExceptionHandler(Exception ex);
        public delegate void OnDataReceivedEventHandler(byte[] data, int length);
        public delegate void OnSocketUnexpectedlyClosedExceptionHandler(IOException ex);
        public event OnConnectedEventHander OnConnected;
        public event OnConnectExceptionHandler OnConnectException;
        public event OnDataReceivedEventHandler OnDataReceived;
        public event OnSocketUnexpectedlyClosedExceptionHandler OnSocketUnexpectedClosed;

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

        public void BeginConnect()
        {
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

                // use a BufferedStream to increase performance
                // this can only be done in .NET4.5 because BufferedStream has been upgraded to support async functions
                this.networkStream = new BufferedStream(this.networkStream);
            }
            catch (Exception ex)
            {
                // bubble exception using events 
                onConnectException(ex);
            }

            // connected & streams created successfully
            this.IsConnected = true;

            // bubble event up
            if (this.OnConnected != null)
            {
                this.OnConnected();
            }

            // begin asynchronously reading
            this.beginReading();
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

        private void beginReading()
        {
            try
            {
                this.networkStream.BeginRead(this.buffer, 0, BufferSize, onDataReceived, this.networkStream);
            }
            catch (IOException ex)
            {
                // IOException means that underling socket is closed
                this.onSocketUnexpectedClosed(ex);
            }
        }

        private void onDataReceived(IAsyncResult result)
        {
            int bytesRead = 0;
            try
            {
                // complete the reading process
                bytesRead = this.networkStream.EndRead(result);
            }
            catch (IOException ex)
            {
                // IOException means that underling socket is closed
                this.onSocketUnexpectedClosed(ex);

                // socket closed, no need to continue to process
                return;
            }

            // only process if bytes received is more than 0
            if (bytesRead > 0)
            {
                // send it upwards
                if (this.OnDataReceived != null)
                {
                    // make a copy of the buffer to send upwards
                    // otherwise when this buffer is overriden, the data above will also be overriden
                    var copy = new byte[bytesRead];
                    Buffer.BlockCopy(this.buffer, 0, copy, 0, bytesRead);
                    this.OnDataReceived(copy, bytesRead);
                }
            }

            // check if disconnected for a responsive return
            if (!this.IsConnected)
            {
                return;
            }

            // continue reading
            this.beginReading();
        }

        private void onSocketUnexpectedClosed(IOException ex)
        {
            // IOException means that underling socket is closed
            // make sure disconnect processes run
            this.Disconnect();

            // bubble event up
            if (this.OnSocketUnexpectedClosed != null)
            {
                this.OnSocketUnexpectedClosed(ex);
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
