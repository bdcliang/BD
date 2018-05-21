namespace BD.Net
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    public class UdpClient
    {

        private static string HostName;
        private static List<IPEndPoint> LocalEPs = new List<IPEndPoint>();
        
        static UdpClient()
        {
            HostName = Dns.GetHostName();
            foreach (var ipa in Dns.GetHostAddresses(HostName))
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                    LocalEPs.Add(new IPEndPoint(ipa,0));
            } 
        }

        public static List<string> LocalIPs
        {
            get
            {
                List<string> list = new List<string>();
                HostName = Dns.GetHostName();
                foreach (var ipa in Dns.GetHostAddresses(HostName))
                {
                    if (ipa.AddressFamily == AddressFamily.InterNetwork)
                        list.Add(ipa.ToString());
                }
                return list;
            }
        }
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
        
        public static void SendBroadCast(byte[] buffer,int port)
        {
            IPEndPoint BroadCastEP = new IPEndPoint(IPAddress.Broadcast, port);
            foreach (var ep in LocalEPs)
            {
                System.Net.Sockets.UdpClient udp = new System.Net.Sockets.UdpClient(ep);
                udp.EnableBroadcast = true;
                udp.Send(buffer, buffer.Length, BroadCastEP);
                udp.Close();
            }
        }

        public static void SendBroadCast(string data,int port,Encoding enc=null)
        {            
            if (enc == null)
                enc = Encoding.UTF8;
            byte[] buffer;
            buffer = enc.GetBytes(data);
            SendBroadCast(buffer,port);
        }
    }
}

