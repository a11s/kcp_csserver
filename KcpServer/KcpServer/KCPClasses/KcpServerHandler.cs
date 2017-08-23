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
            DebugLog($"ctor {nameof(KcpServerHandler)}");
        }

        protected override void BuildCodecsBeforePlayerCreated(PeerContext x)
        {
            DebugLog($"kcp {nameof(BuildCodecsBeforePlayerCreated)}");
            x.Codec = new Codec.KcpCodec(x);            
        }
    }
    public class KcpServerExHandler : UdpServerHandler
    {
        public KcpServerExHandler(ConnectionManager man) : base(man)
        {
            DebugLog($"ctor {nameof(KcpServerExHandler)}");
        }

        protected override void BuildCodecsBeforePlayerCreated(PeerContext x)
        {
            DebugLog($"kcp {nameof(BuildCodecsBeforePlayerCreated)}");
            x.Codec = new Codec.KcpCodecEx(x);
        }
    }
}
