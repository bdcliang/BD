namespace BD.Net
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    public class UdpClient
    {
        private System.Net.Sockets.UdpClient udp = null;
        private IPEndPoint Client;
        /// <summary>
        /// Start a listening
        /// </summary>
        /// <param name="ableBroadcast">enable broadcast</param>
        public void Listen(bool ableBroadcast = false)
        {
            try
            {
                IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 0);
                udp = new System.Net.Sockets.UdpClient(localEP);
                if (ableBroadcast) udp.EnableBroadcast = true;
                BeginReceive();
            }
            catch { }
        }
        private void BeginReceive()
        {
            udp.BeginReceive(ar =>
            {
                if(ar.IsCompleted)
                { 
                try
                {
                    if (udp == null) return;
                    if (udp.Client == null) return;
                    byte[] buffer = udp.EndReceive(ar, ref Client);
                    OnUdpDataInEvent(new List<byte>(buffer), Client);
                }
                catch { return; }
                BeginReceive();
                }
            }, null);
    }


        /// <summary>
        /// Close the Udp Service
        /// </summary>
        public void UdpClose()
        {
            if (udp != null)
            { udp.Close(); udp = null; GC.Collect(); }

        }
        #region  Event handler
        /// <summary>
        /// Udp Data Receive Event
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> UdpDataInEvent;
        private void OnUdpDataInEvent(List<byte> data, IPEndPoint ep)
        {
            UdpDataInEvent?.Invoke(this, new DataReceivedEventArgs(ep,data));
        }
        #endregion




        public static bool Send(EndPoint ep, byte[] data)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                socket.SendTo(data, data.Length, SocketFlags.None, ep);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Send(EndPoint ep, string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            return Send(ep, bytes);
        }

        public static bool Send(EndPoint ep, string data, Encoding enc)
        {
            byte[] bytes = enc.GetBytes(data);
            return Send(ep, bytes);
        }

        public static bool Send(string ip, int port, byte[] data)
        {
            return Send(new IPEndPoint(IPAddress.Parse(ip), port), data);
        }

        public static bool Send(string ip, int port, string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            return Send(new IPEndPoint(IPAddress.Parse(ip), port), bytes);
        }

        public static bool Send(string ip, int port, string data, Encoding enc)
        {
            byte[] bytes = enc.GetBytes(data);
            return Send(new IPEndPoint(IPAddress.Parse(ip), port), bytes);
        }

        public static bool SendMulticast(byte[] data, string ip="224.0.0.122")
        {
            return Send(new IPEndPoint(IPAddress.Parse(ip), 4533), data);
        }

        public static bool SendMulticast(string data, string ip = "224.0.0.122")
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            return SendMulticast(bytes,ip);
        }
    }
}

