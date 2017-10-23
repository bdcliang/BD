namespace BD.Net
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class TcpClient
    {
        private RecvData recvFun;
        private Socket socket;
        public const int ST_CLOSED = 0;
        public const int ST_CONNECT = 2;
        private int state;
        private SvrBreak svbkFun;

        public int Connect(string ip, int port, RecvData recvFun, SvrBreak svbkFun)
        {
            if (this.state != 0)
            {
                return (100 + this.state);
            }
            if (this.socket == null)
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            IPAddress address = IPAddress.Parse(ip);
            EndPoint remoteEP = new IPEndPoint(address, port);
            try
            {
                this.socket.Connect(remoteEP);
            }
            catch (Exception)
            {
                return 1;
            }
            this.state = 2;
            this.recvFun = recvFun;
            this.svbkFun = svbkFun;
            new Thread(new ThreadStart(this.Receive)) { Priority = ThreadPriority.BelowNormal }.Start();
            return 0;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool bDisposing)
        {
            if (this.state != 0)
            {
                this.state = 0;
                Thread.Sleep(50);
                if (this.socket != null)
                {
                    this.socket.Close();
                    this.socket = null;
                }
            }
        }

        public bool GetIPAndPort(ref int ip1, ref int ip2, ref int ip3, ref int ip4, ref int port)
        {
            if ((this.state == 0) || (this.socket == null))
            {
                return false;
            }
            IPEndPoint localEndPoint = (IPEndPoint) this.socket.LocalEndPoint;
            byte[] addressBytes = localEndPoint.Address.GetAddressBytes();
            ip1 = addressBytes[0];
            ip1 = addressBytes[1];
            ip1 = addressBytes[2];
            ip1 = addressBytes[3];
            port = localEndPoint.Port;
            return true;
        }

        private void Receive()
        {
            byte[] buffer = new byte[0x400];
            int len = 0;
            while (2 == this.state)
            {
                try
                {
                    len = this.socket.Receive(buffer, 0x400, SocketFlags.None);
                    if (len > 0)
                    {
                        if (this.recvFun != null)
                        {
                            this.recvFun(buffer, len);
                        }
                    }
                    else
                    {
                        if (this.svbkFun != null)
                        {
                            this.svbkFun();
                        }
                        this.Dispose(true);
                    }
                    continue;
                }
                catch (SocketException exception)
                {
                    if ((SocketError.ConnectionReset == exception.SocketErrorCode) && (this.svbkFun != null))
                    {
                        this.svbkFun();
                    }
                    this.Dispose(true);
                    break;
                }
            }
        }

        public int Send(byte[] data, int len)
        {
            if (2 != this.state)
            {
                return 100;
            }
            try
            {
                this.socket.Send(data, len, SocketFlags.None);
            }
            catch (SocketException exception)
            {
                if (SocketError.ConnectionReset == exception.SocketErrorCode)
                {
                    this.state = 0;
                    return 100;
                }
                return 1;
            }
            return 0;
        }

        public delegate void RecvData(byte[] data, int len);

        public delegate void SvrBreak();
    }
}

