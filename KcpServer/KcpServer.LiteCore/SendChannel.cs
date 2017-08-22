using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KcpServer.Lite
{
    public class SendChannel
    {
        private Socket sock=null;
        public SendChannel(Socket udp)
        {
            sock = udp;
        }
        internal void Send(byte[] sendbuf, EndPoint remoteEP)
        {
            sock.SendTo(sendbuf, remoteEP);
        }
    }
}
