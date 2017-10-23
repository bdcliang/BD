using System;
using System.Collections.Generic;
using System.Text;

namespace BD.Net
{
    using System.Net;
    using Net = System.Net.Sockets;
    public class Multicast
    {
        private Net.UdpClient receiveUdp;
        public Multicast()
        {
            receiveUdp = new Net.UdpClient(4533);
        }
        private string group;
        public void Join(string group="224.0.0.122")
        {
            receiveUdp.JoinMulticastGroup(IPAddress.Parse(group));
            receiveUdp.Ttl = 50;
            this.group = group;
            Receive();
        }
        public void Drop()
        {
            receiveUdp.DropMulticastGroup(IPAddress.Parse(group));
            if(receiveUdp.Client!=null)
                  receiveUdp.Client.Close();
        }
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        private void Receive()
        {
            try
            {
                receiveUdp.BeginReceive(ar =>
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                    int num = receiveUdp.Available;
                    byte[] buff= receiveUdp.EndReceive(ar, ref endPoint);
                    List<byte> recBytes = new List<byte>(buff);
                    this.OnDataReceived(new DataReceivedEventArgs(endPoint, recBytes));
                    Receive();
                }, null);
            }
            catch (System.Exception ex)
            {
                return;
            }
            
        }
        protected void OnDataReceived(DataReceivedEventArgs e)
        {
            if (this.DataReceived != null)
            {
                this.DataReceived(this, e);
            }
        }
    }
}
