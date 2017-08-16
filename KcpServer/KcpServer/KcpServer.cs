using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;

namespace KcpServer
{
    public class KcpServer : UdpServer
    {
        protected override ChannelHandlerAdapter GetServerHandler(ConnectionManager cm)
        {
            return new KcpServerHandler(cm);
        }
    }
}
