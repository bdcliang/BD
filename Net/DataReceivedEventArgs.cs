namespace BD.Net
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class DataReceivedEventArgs : EventArgs
    {
        public DataReceivedEventArgs(EndPoint remoteEP, List<byte> recBytes)
        {
            this.IP = ((IPEndPoint) remoteEP).Address.ToString();
            this.RemoteEP = remoteEP;
            this.RecBytes = recBytes;
            this.RecString = Encoding.UTF8.GetString(recBytes.ToArray());
        }

        public string IP { get; private set; }

        public List<byte> RecBytes { get; private set; }

        public string RecString { get; private set; }

        public EndPoint RemoteEP { get; private set; }
    }
}

