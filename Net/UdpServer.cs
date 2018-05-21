namespace BD.Net
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// 模块支持UDP客户端，需要调用模块的Send的发送方法
    /// </summary>
    public class UdpServer
    {
        private byte[] buff = new byte[0x10000];
        private Socket serverSocket;

        private static object locker = new object();
        /// <summary>
        /// Udp 接收数据事件
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>
        /// 关闭UDP 并释放资源
        /// </summary>
        public void Close()
        {           
            lock (locker)
            {
                if (this.serverSocket != null)
                {
                    this.serverSocket.Close();
                    this.serverSocket = null;
                }
            }
        }

        /// <summary>
        /// 监听UDP
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Listen(int port)
        {
            try
            {                
                this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                this.serverSocket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.Broadcast,true);
                this.serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                this.serverSocket.BeginReceiveFrom(this.buff, 0, this.buff.Length, SocketFlags.None, ref remoteEP, new AsyncCallback(this.Receive), remoteEP);
                return true;
            }
            catch(Exception e)
            {
                this.log(e.Message);
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
        public void Send(byte[] buffer,string ip,int port)
        {
            serverSocket.SendTo(buffer,new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public void Send(string data, string ip, int port)
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            Send(buffer,ip,port);
        }
        public void SendBroadCast(byte[] buffer,int port)
        {
            serverSocket.SendTo(buffer, new IPEndPoint(IPAddress.Broadcast, port));
        }

        public void SendBroadCast(string data,int port)
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            SendBroadCast(buffer,port);
        }
        private void Receive(IAsyncResult ar)
        {
            IPEndPoint point = new IPEndPoint(IPAddress.Any, 0);
            EndPoint endPoint = point;
            try
            {
                lock (locker)
                {
                    if (serverSocket != null)
                    {
                        int num = this.serverSocket.EndReceiveFrom(ar, ref endPoint);
                        List<byte> recBytes = new List<byte>();
                        for (int i = 0; i < num; i++)
                        {
                            recBytes.Add(this.buff[i]);
                        }
                        this.OnDataReceived(new DataReceivedEventArgs(endPoint, recBytes));
                    }
                }
            }
            catch
            {
            }
            finally
            {
                lock (locker)
                {
                    if(serverSocket!=null)
                    this.serverSocket.BeginReceiveFrom(this.buff, 0, this.buff.Length, SocketFlags.None, ref endPoint, new AsyncCallback(this.Receive), endPoint);
                }
            }

        }
    }
}

