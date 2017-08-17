using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static kcpwarpper.KCP;
namespace KcpServer.Codec
{
    public unsafe class KcpCodec : CodecBase
    {
        private PeerContext x;
        public KcpCodec(PeerContext x)
        {
            this.x = x;
            CreateKcp(x);

        }

        public static void CreateKcp(PeerContext x)
        {
            if (x.EncoderData == null)
            {
                var errno = ikcp_create((uint)x.SessionId, (void*)0);
                if ((int)errno != -1)
                {
                    x.EncoderData = errno;
#if PRINTPACK
                    Console.WriteLine($"create kcp {(int)x.EncoderData}");
#endif

                    ikcp_wndsize(x.EncoderData, 128, 128);
                    ikcp_nodelay(x.EncoderData, 1, 10, 2, 1);
                    x.EncoderData->rx_minrto = 10;
                    x.EncoderData->fastresend = 1;
                    x.EncoderData->mtu = Utilities.ToServerPackBuilder.MAX_DATA_LEN;//可能还要浪费几个字节
                }
                else
                {
                    x.EncoderData = null;
                    throw new InvalidCastException($"kcp create failed {(int)errno}");
                }
            }
        }


        public override void Close()
        {
            if (x.EncoderData != null)
            {
                Console.WriteLine($"release kcp {(int)x.EncoderData}");
                ikcp_release(x.EncoderData);
                x.EncoderData = null;
            }
            Console.WriteLine("kcp encoder close");
        }
    }
}
