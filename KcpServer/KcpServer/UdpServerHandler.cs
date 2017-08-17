using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Utilities;

namespace KcpServer
{
    public partial class UdpServerHandler : SimpleChannelInboundHandler<DatagramPacket>
    {


        protected void DebugLog(string s)
        {
#if DEBUG
            Console.WriteLine(s);
#endif
        }
        ApplicationBase App { get => connMan.App; }

        ToServerPackBuilder defpb;
        byte[] recdatabuff = new byte[ToServerPackBuilder.MAX_DATA_LEN + ToServerPackBuilder.HEADER_LEN];
        ConnectionManager connMan = null;
        public UdpServerHandler(ConnectionManager man)
        {
            defpb = new ToServerPackBuilder(man._SysId, 0);
            connMan = man;
        }
        protected override void ChannelRead0(IChannelHandlerContext ctx, DatagramPacket msg)
        {
            var len = defpb.Read(msg.Content.Array, recdatabuff, out var sid, out var sysbuff);
            if (len < 0)
            {
                //bad sysid.
                DebugLog("bad sysid:" + string.Join("", sysbuff));
            }
            else if (len >= 0)
            {
                //查看sid是不是0
                if (sid == 0)
                {
                    //请求握手
                    var appdata = new byte[len];
                    Array.Copy(recdatabuff, appdata, len);
                    var x = new PeerContext() { ApplicationData = appdata, RemoteEP = msg.Sender, LocalEP = msg.Recipient, SessionId = 0, ConnectionManager = connMan };
                    if (!connMan.App.PreCreatePeer(x))
                    {
                        DebugLog("app refuse connect request");
                        byte[] hsbuff = defpb.MakeHandshakeReturn((int)ClientErrorCode.APP_REFUSED);
                        ctx.Channel.WriteAndFlushAsync(new DatagramPacket(Unpooled.Buffer(hsbuff.Length).WriteBytes(hsbuff), msg.Sender));
                        return;
                    }
                    var newsid = connMan.EnumANewPeerId();
                    if (newsid == ConnectionManager.MAX_CONN_EXCEED)
                    {
                        DebugLog("server refuse connect request");
                        byte[] hsbuff = defpb.MakeHandshakeReturn((int)ClientErrorCode.SERVER_REFUSED);
                        ctx.Channel.WriteAndFlushAsync(new DatagramPacket(Unpooled.Buffer(hsbuff.Length).WriteBytes(hsbuff), msg.Sender));
                        return;
                    }
                    else
                    {
                        x.SessionId = newsid;
                        BuildCodecsBeforePlayerCreated(x);
                        PeerBase p = connMan.App.CreatePeer(x);
                        if (p == null)
                        {
                            connMan.RecycleSession(newsid);
                            DebugLog("app refuse create player");
                            byte[] hsbuff = defpb.MakeHandshakeReturn((int)ClientErrorCode.APP_REFUSED2);
                            ctx.Channel.WriteAndFlushAsync(new DatagramPacket(Unpooled.Buffer(hsbuff.Length).WriteBytes(hsbuff), msg.Sender));
                            return;
                        }
                        else
                        {
                            p.LastPackTime = DateTime.Now;
                            p.Context.RemoteEP = msg.Sender;
                            p.Context.LocalEP = msg.Recipient;
                            p.Channel = ctx.Channel;
                            byte[] hsbuff = defpb.MakeHandshakeReturn(newsid);
                            ctx.Channel.WriteAndFlushAsync(new DatagramPacket(Unpooled.Buffer(hsbuff.Length).WriteBytes(hsbuff), msg.Sender));
                            connMan.AddConn(p);
                            DebugLog($"{nameof(ConnectionManager)}:new session established {newsid}");

                        }
                    }

                }
                else
                {
                    //data arrival
                    var pc = connMan.FindPeer(sid);
                    if (pc == null)
                    {
                        DebugLog($"cant find peer:{sid}");
                    }
                    else
                    {
                        pc.LastPackTime = DateTime.Now;
                        if (len == 0)
                        {
                            //heartbeat
                        }
                        else
                        {
                            var recdata = new byte[len];
                            Array.Copy(recdatabuff, 0, recdata, 0, len);
                            ProcessIncomingData(pc, recdata);
                        }

                    }
                }
            }

        }

        protected virtual void BuildCodecsBeforePlayerCreated(PeerContext x)
        {
            x.Codec = new Codec.CodecBase();
        }

        protected virtual void ProcessIncomingData(PeerBase peer, byte[] recdata)
        {
            //var realdata = peer.Context.Codec.Decode(recdata);
            peer.AddRecData(recdata);
        }
    }
}
