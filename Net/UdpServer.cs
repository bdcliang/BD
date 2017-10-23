namespace BD.Net
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class UdpServer
    {
        private byte[] buff = new byte[0x10000];
        private Socket serverSocket;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public void Close()
        {
            if (this.serverSocket != null)
            {
                this.serverSocket.Close();
            }
        }

        public bool Listen(int port)
        {
            try
            {
                this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                this.serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                this.serverSocket.BeginReceiveFrom(this.buff, 0, this.buff.Length, SocketFlags.None, ref remoteEP, new AsyncCallback(this.Receive), remoteEP);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected void OnDataReceived(DataReceivedEventArgs e)
        {
            if (this.DataReceived != null)
            {
                this.DataReceived(this, e);
            }
        }

        private void Receive(IAsyncResult ar)
        {
            try
            {
                IPEndPoint point = new IPEndPoint(IPAddress.Any, 0);
                EndPoint endPoint = point;
                int num = this.serverSocket.EndReceiveFrom(ar, ref endPoint);
                List<byte> recBytes = new List<byte>();
                for (int i = 0; i < num; i++)
                {
                    recBytes.Add(this.buff[i]);
                }
                this.OnDataReceived(new DataReceivedEventArgs(endPoint, recBytes));
                this.serverSocket.BeginReceiveFrom(this.buff, 0, this.buff.Length, SocketFlags.None, ref endPoint, new AsyncCallback(this.Receive), endPoint);
            }
            catch
            {
            }
        }
    }
}

