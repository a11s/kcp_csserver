using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KcpServer
{
    internal class UdpServer
    {
        void debug(string s)
        {
#if DEBUG
            Console.WriteLine(s);
#endif
        }
        //if tcp use ServerBootstrap
        Bootstrap bootstrap;
        //IChannel channel;
        IEventLoopGroup iogroup;
        //if tcp you need workers
        //IEventLoopGroup workergroup;
        internal Task<bool> InitServerAsync(ChannelHandlerAdapter handler,IPEndPoint localipep)
        {
            iogroup = new MultithreadEventLoopGroup();
            //workergroup = new MultithreadEventLoopGroup();
            try
            {
                if (bootstrap != null)
                {
                    throw new InvalidOperationException("重复init");
                }
                bootstrap = new Bootstrap();
                bootstrap.Group(iogroup)
                    .Channel<SocketDatagramChannel>()
                    .Option(ChannelOption.SoBroadcast, true)
                    .Handler(handler);
                var _channel = bootstrap.BindAsync(localipep);
                //channel = _channel.Result;
                debug("inited");
                return Task.FromResult(true);
            }
            catch (System.Threading.ThreadInterruptedException e)
            {
                debug(e.ToString());
                debug("shutdown");
                iogroup.ShutdownGracefullyAsync();
            }
            return Task.FromResult(false);
        }

        internal Task CloseAsync()
        {
            return iogroup.ShutdownGracefullyAsync();
        }
    }

    
}
