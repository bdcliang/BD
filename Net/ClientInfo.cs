namespace BD.Net
{
    using System;
    using System.IO;

    public class ClientInfo
    {
        public int id;
        public int ip1;
        public int ip2;
        public int ip3;
        public int ip4;
        public bool isSend;
        public int port;
        public int post;
        public long recvLength;
        public long sendLength;

        public ClientInfo(int id, int ip1, int ip2, int ip3, int ip4, int port)
        {
            this.id = id;
            this.ip1 = ip1;
            this.ip2 = ip2;
            this.ip3 = ip3;
            this.ip4 = ip4;
            this.port = port;
            this.recvLength = 0L;
            this.sendLength = 0L;
        }

        public string GetIP()
        {
            return string.Concat(new object[] { this.ip1, ".", this.ip2, ".", this.ip3, ".", this.ip4 });
        }
    }
}

