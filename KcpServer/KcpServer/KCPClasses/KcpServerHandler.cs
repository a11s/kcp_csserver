using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpServer
{
    public class KcpServerHandler : UdpServerHandler
    {
        public KcpServerHandler(ConnectionManager man) : base(man)
        {            
            debug($"ctor {nameof(KcpServerHandler)}");
        }

        protected override void ProcessIncomingData(PeerBase pc, byte[] recdata)
        {
            debug($"kcp {nameof(ProcessIncomingData)}");
            var realdata = pc.Context.Decoder.Decode(recdata);
            pc.AddRecData(realdata);
        }

        protected override void PrepCodecs(PeerContext x)
        {
            debug($"kcp {nameof(PrepCodecs)}");
            x.Encoder = new Codec.KcpEncoder(x);
            x.Decoder = new Codec.KcpDecoder(x);
        }
    }
}
