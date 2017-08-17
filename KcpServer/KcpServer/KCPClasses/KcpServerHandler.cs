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

        protected override void BuildCodecsBeforePlayerCreated(PeerContext x)
        {
            debug($"kcp {nameof(BuildCodecsBeforePlayerCreated)}");
            x.Codec = new Codec.KcpCodec(x);            
        }
    }
}
