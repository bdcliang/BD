namespace BD.Net
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class TcpServer
    {
        private const int BUFFER_LENGTH = 0x1000;
        private CliBreak clbkFun;
        private CliConn clcnFun;
        private ClientThread[] connClient;
        private int length;
        private int number;
        private const int QUEUE_INC = 100;
        private RecvData recvFun;
        private Socket socket;
        public const int ST_CLOSED = 0;
        public const int ST_CONNECT = 2;
        public const int ST_LISTEN = 1;
        private int[] stack;
        private int state;
        private int top;

        public bool BreakConn(int id)
        {
            if (id >= this.length)
            {
                return false;
            }
            if (((this.connClient[id] == null) || (this.connClient[id].state == 0)) || (this.connClient[id].socket == null))
            {
                return false;
            }
            this.connClient[id].state = 0;
            Thread.Sleep(5);
            this.connClient[id].socket.Close();
            this.connClient[id].socket = null;
            this.Push(id);
            return true;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool bDisposing)
        {
            if (this.state != 0)
            {
                int num;
                this.state = 0;
                Thread.Sleep(50);
                if (this.socket != null)
                {
                    this.socket.Close();
                    this.socket = null;
                }
                for (num = 0; num < this.number; num++)
                {
                    if (this.connClient[num] != null)
                    {
                        this.connClient[num].state = 0;
                    }
                }
                Thread.Sleep(10);
                for (num = 0; num < this.number; num++)
                {
                    if ((this.connClient[num] != null) && (this.connClient[num].socket != null))
                    {
                        this.connClient[num].socket.Close();
                        this.connClient[num].socket = null;
                    }
                    this.connClient[num] = null;
                }
                this.connClient = null;
                this.stack = null;
            }
        }

        public bool GetIPAndPort(int id,ref string ip, ref int port)
        {
            if (id >= this.length)
            {
                return false;
            }
            if (((this.connClient[id] == null) || (this.connClient[id].state == 0)) || (this.connClient[id].socket == null))
            {
                return false;
            }
            IPEndPoint localEndPoint = (IPEndPoint) this.connClient[id].socket.LocalEndPoint;
            ip = localEndPoint.Address.ToString();
            return true;
        }

        private int GetNextStation()
        {
            int number = -1;
            lock (this.stack)
            {
                if (this.top == 0)
                {
                    if (this.number == this.length)
                    {
                        this.IncLinkQueue();
                    }
                    number = this.number;
                    this.number++;
                    return number;
                }
                this.top--;
                return this.stack[this.top];
            }
        }

        private void IncLinkQueue()
        {
            ClientThread[] threadArray = new ClientThread[this.length + 100];
            int index = 0;
            while (index < this.length)
            {
                threadArray[index] = this.connClient[index];
                index++;
            }
            this.length += 100;
            while (index < this.length)
            {
                threadArray[index] = null;
                index++;
            }
            this.connClient = threadArray;
            this.stack = new int[this.length];
        }

        public int Listen(int port, int len, CliConn clcnFun, CliBreak clbkFun, RecvData recvFun)
        {
            if (this.state != 0)
            {
                return (100 + this.state);
            }
            if (this.socket == null)
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.connClient = new ClientThread[100];
                this.stack = new int[100];
                this.length = 100;
                this.number = 0;
                this.top = 0;
                for (int i = 0; i < 100; i++)
                {
                    this.connClient[i] = null;
                }
            }
            try
            {
                EndPoint localEP = new IPEndPoint(0L, port);
                this.socket.Bind(localEP);
                this.socket.Listen(len);
            }
            catch (Exception)
            {
                return 1;
            }
            this.clcnFun = clcnFun;
            this.clbkFun = clbkFun;
            this.recvFun = recvFun;
            this.state = 1;
            new Thread(new ThreadStart(this.Listening)) { Priority = ThreadPriority.BelowNormal }.Start();
            return 0;
        }

        private void Listening()
        {
            while (1 == this.state)
            {
                try
                {
                    Socket sock = this.socket.Accept();
                    if (sock != null)
                    {
                        int nextStation = this.GetNextStation();
                        IPEndPoint remoteEndPoint = (IPEndPoint) sock.RemoteEndPoint;
                        byte[] addressBytes = remoteEndPoint.Address.GetAddressBytes();
                        this.connClient[nextStation] = new ClientThread(nextStation, sock, this);
                        if (this.clcnFun != null)
                        {
                            this.clcnFun(nextStation, addressBytes[0], addressBytes[1], addressBytes[2], addressBytes[3], remoteEndPoint.Port);
                        }
                        new Thread(new ThreadStart(this.connClient[nextStation].Receive)) { Priority = ThreadPriority.BelowNormal }.Start();
                    }
                    continue;
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private void Push(int v)
        {
            if (this.stack != null)
            {
                lock (this.stack)
                {
                    this.stack[this.top] = v;
                    this.top++;
                }
            }
        }

        public int Send(int id, byte[] data, int len)
        {
            if (id >= this.length)
            {
                return 100;
            }
            if (this.connClient[id] == null)
            {
                return 100;
            }
            try
            {
                this.connClient[id].socket.Send(data, len, SocketFlags.None);
            }
            catch (SocketException exception)
            {
                if (SocketError.ConnectionReset == exception.SocketErrorCode)
                {
                    return 100;
                }
                return 0;
            }
            return 0;
        }

        public delegate void CliBreak(int id);

        public delegate void CliConn(int id, int ip1, int ip2, int ip3, int ip4, int port);

        internal class ClientThread
        {
            private int id;
            private TcpServer pSvr;
            public Socket socket;
            public int state;

            public ClientThread(int id, Socket sock, TcpServer pSvr)
            {
                this.socket = sock;
                this.pSvr = pSvr;
                this.id = id;
                this.state = 2;
            }

            public void Receive()
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
                            if (this.pSvr.recvFun != null)
                            {
                                this.pSvr.recvFun(this.id, buffer, len);
                            }
                        }
                        else
                        {
                            if (this.pSvr.clbkFun != null)
                            {
                                this.pSvr.clbkFun(this.id);
                            }
                            this.pSvr.BreakConn(this.id);
                        }
                        continue;
                    }
                    catch (SocketException exception)
                    {
                        if (((SocketError.ConnectionReset == exception.SocketErrorCode) && this.pSvr.BreakConn(this.id)) && (this.pSvr.clbkFun != null))
                        {
                            this.pSvr.clbkFun(this.id);
                        }
                        break;
                    }
                }
                this.pSvr = null;
            }
        }

        public delegate void RecvData(int id, byte[] data, int len);
    }
}

