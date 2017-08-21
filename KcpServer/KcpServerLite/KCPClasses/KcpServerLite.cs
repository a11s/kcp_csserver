using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpServer.Lite
{
    /// <summary>
    /// 不创建任何线程或者Task,用于某些不能创建线程的环境.一切由外部信号驱动.
    /// </summary>
    public class KcpServerLite : UdpServerLite
    {
        protected override void BuildCodecsBeforePlayerCreated(PeerContext x)
        {
            DebugLog($"kcp {nameof(BuildCodecsBeforePlayerCreated)}");
            x.Codec = new Codec.KcpCodec(x);
        }
    }
}
